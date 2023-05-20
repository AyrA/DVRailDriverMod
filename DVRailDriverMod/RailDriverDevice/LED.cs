using DVRailDriverMod.Interface.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace DVRailDriverMod.RailDriverDevice
{
    /// <summary>
    /// Provides interaction with the 3 digit 7-segment LED display
    /// </summary>
    internal sealed class LED : IDisposable
    {
        /// <summary>
        /// Byte to initialize display writes
        /// </summary>
        public const byte LED_INIT = 134;
        /// <summary>
        /// Write fail code
        /// </summary>
        /// <remarks>This code indicates that the caller should retry</remarks>
        public const int WRITE_FAILED = 404;
        /// <summary>
        /// Write success
        /// </summary>
        public const int WRITE_SUCCESS = 0;

        /// <summary>
        /// Event that is triggered when the marquee string has been fully displayed
        /// </summary>
        /// <remarks>This is also fired when the marquee is programmed to repeat itself</remarks>
        public event Action<LED> MarqueeEnd = delegate { };

        /// <summary>
        /// Maps character to 7-segment values
        /// </summary>
        private static readonly Dictionary<char, Segment> CharMap;
        /// <summary>
        /// Figure-8 loading animation
        /// </summary>
        private static readonly List<Segment> LoadAnimation;

        /// <summary>
        /// Marquee scroll intervall in ms
        /// </summary>
        private int interval = 0;
        /// <summary>
        /// Gets the device associated with this instance
        /// </summary>
        public HID.HidPieDevice Dev { get; private set; }

        /// <summary>
        /// Gets or sets the marquee delay
        /// </summary>
        /// <remarks>This is accurate to 100 ms</remarks>
        public int MarqueeDelay
        {
            get
            {
                return interval;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentOutOfRangeException();
                }
                interval = value / 100 * 100;
            }
        }
        /// <summary>
        /// Gets or sets if the marquee text should repeat
        /// </summary>
        public bool MarqueeRepeat { get; set; }

        /// <summary>
        /// Segment values for the marquee text
        /// </summary>
        private Segment[] marqueeCode;
        /// <summary>
        /// Current offset into <see cref="marqueeCode"/>
        /// </summary>
        private int marqueeOffset;
        /// <summary>
        /// if true, the loader image is used in the <see cref="MarqueeThread"/>
        /// </summary>
        private bool useLoader = false;
        private string lastString = null;

        /// <summary>
        /// Fills the <see cref="CharMap"/> dictionary and <see cref="LoadAnimation"/> list
        /// </summary>
        static LED()
        {
            LoadAnimation = new List<Segment>();
            #region Fill Loader
            LoadAnimation.Add(Segment.Top);
            LoadAnimation.Add(Segment.TopRight);
            LoadAnimation.Add(Segment.Middle);
            LoadAnimation.Add(Segment.BottomLeft);
            LoadAnimation.Add(Segment.Bottom);
            LoadAnimation.Add(Segment.BottomRight);
            LoadAnimation.Add(Segment.Middle);
            LoadAnimation.Add(Segment.TopLeft);
            #endregion
            CharMap = new Dictionary<char, Segment>();
            #region Fill Map
            CharMap['0'] = Segment.Top | Segment.Bottom | Segment.Left | Segment.Right;
            CharMap['1'] = Segment.Right;
            CharMap['2'] = Segment.Center | Segment.TopRight | Segment.BottomLeft;
            CharMap['3'] = Segment.Center | Segment.Right;
            CharMap['4'] = Segment.TopLeft | Segment.Middle | Segment.Right;
            CharMap['5'] = Segment.Center | Segment.TopLeft | Segment.BottomRight;
            CharMap['6'] = Segment.Center | Segment.Left | Segment.BottomRight;
            CharMap['7'] = Segment.Top | Segment.Right;
            CharMap['8'] = Segment.All;
            CharMap['9'] = Segment.Center | Segment.Right | Segment.TopLeft;
            CharMap['A'] = Segment.Left | Segment.Right | Segment.Top | Segment.Middle;
            CharMap['B'] = Segment.Left | Segment.Middle | Segment.BottomRight | Segment.Bottom;
            CharMap['C'] = Segment.BottomLeft | Segment.Bottom | Segment.Middle;
            CharMap['D'] = Segment.Right | Segment.BottomLeft | Segment.Middle | Segment.Bottom;
            CharMap['E'] = Segment.Center | Segment.Left;
            CharMap['F'] = Segment.Left | Segment.Middle | Segment.Top;
            CharMap['G'] = Segment.Left | Segment.Top | Segment.Bottom | Segment.BottomRight;
            CharMap['H'] = Segment.Left | Segment.Middle | Segment.Right;
            CharMap['I'] = Segment.Left;
            CharMap['J'] = Segment.Right | Segment.Bottom;
            CharMap['L'] = Segment.Left | Segment.Bottom;
            CharMap['N'] = Segment.BottomLeft | Segment.BottomRight | Segment.Middle;
            CharMap['O'] = Segment.Bottom | Segment.Middle | Segment.BottomLeft | Segment.BottomRight;
            CharMap['P'] = Segment.Left | Segment.Top | Segment.Middle | Segment.TopRight;
            CharMap['Q'] = Segment.TopLeft | Segment.Top | Segment.Middle | Segment.Right;
            CharMap['R'] = Segment.BottomLeft | Segment.Middle;
            CharMap['S'] = CharMap['5'];
            CharMap['T'] = Segment.Left | Segment.Bottom | Segment.Middle;
            CharMap['U'] = Segment.Left | Segment.Bottom | Segment.Right;
            CharMap['Y'] = Segment.Right | Segment.Middle | Segment.TopLeft | Segment.Bottom;
            CharMap['Z'] = CharMap['2'];
            CharMap['!'] = Segment.TopRight | Segment.Dot;
            CharMap['"'] = Segment.TopLeft | Segment.TopRight;
            CharMap['\''] = Segment.TopLeft;
            CharMap['('] = Segment.Top | Segment.Bottom | Segment.Left;
            CharMap[')'] = Segment.Top | Segment.Bottom | Segment.Right;
            CharMap[','] = Segment.Dot;
            CharMap['_'] = Segment.Bottom;
            CharMap['-'] = Segment.Middle;
            CharMap['¯'] = Segment.Top;
            CharMap['='] = Segment.Bottom | Segment.Middle;
            CharMap['?'] = Segment.Top | Segment.TopRight | Segment.Dot;
            CharMap['.'] = Segment.Dot;
            CharMap['°'] = Segment.Top | Segment.Middle | Segment.TopLeft | Segment.TopRight;
            CharMap['@'] = Segment.All ^ Segment.BottomRight;
            #endregion
        }

        /// <summary>
        /// Initializes a new LED instance
        /// </summary>
        /// <param name="device">RailDriver device</param>
        /// <param name="ownsDevice">true, to close device handles when this instance is disposed</param>
        /// <exception cref="ArgumentNullException"><paramref name="device"/> is null</exception>
        internal LED(HID.HidPieDevice device)
        {
            Dev = device ?? throw new ArgumentNullException(nameof(device));
            MarqueeDelay = 300;
            new Thread(MarqueeThread) { IsBackground = true }.Start();
        }

        /// <summary>
        /// Clears the marquee string and ends the marquee display mode
        /// </summary>
        public void ClearMarquee()
        {
            lock (this)
            {
                marqueeCode = null;
                MarqueeRepeat = false;
                marqueeOffset = 0;
                useLoader = false;
            }
        }

        /// <summary>
        /// Sets text that should scroll over the display
        /// </summary>
        /// <param name="text">Text</param>
        /// <param name="repeat">
        /// true to infinitely repeat the text.
        /// Must call <see cref="ClearMarquee"/> to stop it.
        /// </param>
        /// <remarks>
        /// This just transforms the string using <see cref="GetLEDCode(char)"/>
        /// and internally calls <see cref="SetMarquee(IEnumerable{Segment}, bool)"/>
        /// </remarks>
        public void SetMarquee(string text, bool repeat)
        {
            if (text == lastString)
            {
                return;
            }
            lastString = text;
            SetMarquee(MergeDot(text.ToUpper().Select(GetLEDCode)), repeat);
        }

        /// <summary>
        /// Sets display segments that should scroll over the display
        /// </summary>
        /// <param name="codes">Display segments</param>
        /// <param name="repeat">
        /// true to infinitely repeat the text.
        /// Must call <see cref="ClearMarquee"/> to stop it.
        /// </param>
        public void SetMarquee(IEnumerable<Segment> codes, bool repeat)
        {
            lock (this)
            {
                var Text = codes.ToArray();
                ClearMarquee();
                if (Text.Length > 3)
                {
                    //Prefix with two spaces so it comes in from the right
                    //rather than starting with an already full display
                    marqueeCode = new Segment[] { Segment.None, Segment.None }.Concat(Text).ToArray();
                    marqueeOffset = 0;
                    MarqueeRepeat = repeat;
                }
                else
                {
                    if (Text.Length > 0)
                    {
                        //Pad to 3 elements
                        Text = Text
                            .Concat(new Segment[] { Segment.None, Segment.None })
                            .Take(3)
                            .ToArray();
                        SetLED(Text[0], Text[1], Text[2]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the 7 segment values to display the specified character
        /// </summary>
        /// <param name="c">Character</param>
        /// <returns>
        /// 7-segment code. If the character is not in the map, <see cref="Segment.None"/> is returned
        /// </returns>
        public static Segment GetLEDCode(char c)
        {
            return CharMap.ContainsKey(c) ? CharMap[c] : Segment.None;
        }

        /// <summary>
        /// Get characters that the display knows
        /// </summary>
        /// <returns>Known characters</returns>
        public static IEnumerable<char> GetKnownCharacters()
        {
            return CharMap.Keys;
        }

        /// <summary>
        /// Gets characters in the range A-Z that cannot be displayed
        /// </summary>
        /// <returns>Impossible characters</returns>
        public static IEnumerable<char> GetUnsupportedAlphaChars()
        {
            foreach (var c in "ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            {
                if (!CharMap.ContainsKey(c))
                {
                    yield return c;
                }
            }
        }

        /// <summary>
        /// Clears all display segments
        /// </summary>
        public void ClearDisplay()
        {
            SetLED(Segment.None, Segment.None, Segment.None);
        }

        /// <summary>
        /// Sets the 3 display segments to the specified values
        /// </summary>
        /// <param name="First">Leftmost unit</param>
        /// <param name="Center">Center unit</param>
        /// <param name="Last">Rightmost unit</param>
        /// <exception cref="ObjectDisposedException">LED instance was disposed</exception>
        public void SetLED(Segment First, Segment Center, Segment Last)
        {
            if (Dev == null)
            {
                throw new ObjectDisposedException(nameof(LED));
            }
            var data = new byte[Dev.DeviceInfo.WriteSize];
            data[1] = LED_INIT;
            data[2] = (byte)Last;
            data[3] = (byte)Center;
            data[4] = (byte)First;
            Dev.WriteData(data);
        }

        /// <summary>
        /// Sets text string to display
        /// </summary>
        /// <remarks>Automatically uses <see cref="SetMarquee(string, bool)"/> to set longer text strings</remarks>
        /// <param name="text">Text to show</param>
        /// <returns>True, if the command was sent</returns>
        public void SetText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                ClearDisplay();
                lastString = null;
                return;
            }
            if (text == lastString)
            {
                return;
            }
            lastString = text;
            var Codes = MergeDot(text
                .Select(GetLEDCode))
                .ToArray();
            if (Codes.Length < 4)
            {
                Codes = Codes.Concat(new Segment[] { Segment.None, Segment.None, Segment.None }).ToArray();
                SetLED(Codes[0], Codes[1], Codes[2]);
            }
            else
            {
                SetMarquee(text, true);
            }
        }

        /// <summary>
        /// Shows the given number on the display
        /// </summary>
        /// <remarks>
        /// This will correctly display negative numbers.
        /// It also rounds the number appropriately to fit into the display.
        /// The range is capped at -99 to 999.
        /// Can handle <see cref="double.NaN"/> and shows it as such
        /// </remarks>
        /// <param name="d">Number</param>
        public void SetNumber(double d)
        {
            //Negative only works down to -99 because of the "-"
            if (d < -99.0)
            {
                SetNumber(-99.0);
                return;
            }
            //Positive numbers works up to 999
            if (d > 999.0)
            {
                SetNumber(999.0);
                return;
            }

            //Add "NAN" support. Could also use zero instead
            if (double.IsNaN(d))
            {
                SetText("NAN");
                return;
            }
            //At -10 and less we don't display decimals anymore
            if (d <= -10.0)
            {
                SetText(Math.Round(d).ToString());
                return;
            }
            //At 100 and more we don't display decimals anymore
            if (d >= 100.0)
            {
                SetText(Math.Round(d).ToString());
                return;
            }
            //Round number to a single digit and set it
            var text = Math.Round(d, 1).ToString("0.0").PadLeft(4);
            SetText(text);
        }

        /// <summary>
        /// Shows the loader animation
        /// </summary>
        /// <param name="Repeat">true, to repeat the animation indefinitely</param>
        public void Loader(bool Repeat)
        {
            if (lastString == "\0")
            {
                return;
            }
            lastString = "\0";
            MarqueeDelay = 100;
            marqueeCode = LoadAnimation.ToArray();
            marqueeOffset = 0;
            MarqueeRepeat = Repeat;
            useLoader = true;
        }

        /// <summary>
        /// Ends loader animation display
        /// </summary>
        public void EndLoader()
        {
            marqueeCode = null;
            useLoader = false;
            ClearDisplay();
            lastString = null;
        }

        /// <summary>
        /// Disposes this instance
        /// </summary>
        /// <remarks>
        /// Also closes device handle if this instance is set as the device owner</remarks>
        public void Dispose()
        {
            lock (this)
            {
                var d = Dev;
                if (d != null)
                {
                    ClearDisplay();
                    Dev = null;
                }
            }
        }

        /// <summary>
        /// Merges a lone dot into the previous character if possible
        /// </summary>
        /// <param name="Segments">Segment instructions</param>
        /// <returns>Segment instructions</returns>
        private Segment[] MergeDot(IEnumerable<Segment> Segments)
        {
            var Ret = new List<Segment>();
            foreach (var S in Segments)
            {
                //Add the new character if either of these conditions are true:
                //- List is empty
                //- Character is not a sole dot
                //- Last entry in list already has a dot
                if (Ret.Count == 0 || S != Segment.Dot || Ret[Ret.Count - 1].HasFlag(Segment.Dot))
                {
                    Ret.Add(S);
                }
                else
                {
                    Ret[Ret.Count - 1] |= Segment.Dot;
                }
            }
            return Ret.ToArray();
        }

        /// <summary>
        /// Thread that handles marquee text display
        /// </summary>
        private void MarqueeThread()
        {
            HID.HidPieDevice Device;
            while (Dev != null)
            {
                Device = Dev;
                //Multi threading guard
                if (Device == null)
                {
                    return;
                }
                var Text = marqueeCode;
                if (Text != null)
                {
                    //Text strings are usually followed with 3 spaces
                    //to make the text disappear completely before restarting,
                    //but for the loader this is not desired
                    if (useLoader)
                    {
                        SetLED(Text[marqueeOffset], Text[marqueeOffset], Text[marqueeOffset]);
                    }
                    else
                    {
                        //Get up to 3 characters from the current marquee position
                        var Letters = Text
                            .Skip(marqueeOffset)
                            .Take(3)
                            .ToArray();
                        //If less than 3 available, get as many as needed from the start
                        if (Letters.Length < 3)
                        {
                            Letters = Letters.Concat(Text.Take(3 - Letters.Length)).ToArray();
                        }
                        SetLED(Letters[0], Letters[1], Letters[2]);
                    }
                    if (++marqueeOffset >= Text.Length)
                    {
                        if (MarqueeRepeat)
                        {
                            marqueeOffset = 0;
                        }
                        else
                        {
                            marqueeCode = null;
                        }
                        //Do not block the current thread with this event
                        new Thread(delegate ()
                        {
                            MarqueeEnd(this);
                        }).Start();
                    }
                }
                int delay = interval / 100;
                for (var i = 0; i < delay && Dev != null; i++)
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
