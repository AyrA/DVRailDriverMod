using DVRailDriverMod.Interface.Enums;
using System;
using System.Linq;
using System.Threading;

namespace DVRailDriverMod.RailDriverDevice
{
    /// <summary>
    /// Handles RailDriver events and decodes event data into button and lever values
    /// </summary>
    internal class RDEventHandler : IDisposable
    {
        //00 - Start Marker
        //41 - Reverser: Larger towards "Reverse"
        //39 - Throttle: Larger towards "Throttle"
        //42 - Train brake: Larger towards "Release"
        //25 - Ind. brake: Larger towards "Release"
        //89 - Ind. brake side: Larger towards "Right"
        //83 - Wiper: Larger towards "Full"
        //B1 - Lights: Larger towards "Full"
        //00 - Top row first 8 buttons
        //00 - Top row next 6 buttons + bottom first 2 buttons
        //00 - Bottom next 8 buttons
        //00 - Bottom next 4 buttons + up,down,DU,DR
        //00 - DD,DL,RangeUp,RangeDown,E-Up,E-Down,Alert,Sand
        //00 - P,Bell,HornUp,HornDown
        //35 - End Marker

        /// <summary>
        /// Event that is raised when a button or lever value changes
        /// </summary>
        public event Action<RDEventHandler, ButtonType> ButtonChange = delegate { };

        /// <summary>
        /// Value of the first data byte
        /// </summary>
        private const byte READ_START = 0x0;
        /// <summary>
        /// Value of the last data byte
        /// </summary>
        private const byte READ_END = 0x35;

        /// <summary>
        /// RailDriver device
        /// </summary>
        private readonly HID.HidPieDevice device;

        /// <summary>
        /// Gets the reverser value
        /// </summary>
        public byte Reverser { get; private set; }
        /// <summary>
        /// Gets the throttle value
        /// </summary>
        public byte Throttle { get; private set; }
        /// <summary>
        /// Gets the train brake value
        /// </summary>
        public byte TrainBrake { get; private set; }
        /// <summary>
        /// Gets the independent lever value (up/down motion)
        /// </summary>
        public byte IndependentBrake { get; private set; }
        /// <summary>
        /// Gets the independent lever value (side to side motion)
        /// </summary>
        public byte IndependentSide { get; private set; }
        /// <summary>
        /// Gets the wipers button value
        /// </summary>
        public byte Wipers { get; private set; }
        /// <summary>
        /// Gets the lights button value
        /// </summary>
        public byte Lights { get; private set; }

        /// <summary>
        /// Gets the top 14 buttons
        /// </summary>
        public RowButtons TopRow { get; private set; }
        /// <summary>
        /// Gets the bottom 14 buttons
        /// </summary>
        public RowButtons BottomRow { get; private set; }
        /// <summary>
        /// Gets the up-down button state
        /// </summary>
        public UpDownButtons UpDown { get; private set; }
        /// <summary>
        /// Gets the D-Pad state
        /// </summary>
        public CrossButtons DPad { get; private set; }
        /// <summary>
        /// Gets the auxiliary (top left) button group states
        /// </summary>
        public AuxButtons Auxiliary { get; private set; }

        private Thread dataReader;
        private CancellationTokenSource source = new CancellationTokenSource();


        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="dev">RailDriver device</param>
        internal RDEventHandler(HID.HidPieDevice dev)
        {
            device = dev ?? throw new ArgumentNullException(nameof(dev));
        }

        /// <summary>
        /// Starts device listening and event raising
        /// </summary>
        /// <remarks>
        /// This immediately raises many <see cref="ButtonChange"/> events
        /// because all values default to zero, but no alalog values in the RailDriver do
        /// </remarks>
        public void Start()
        {
            if (dataReader != null)
            {
                throw new InvalidOperationException("Device has already been started");
            }
            dataReader = new Thread(DeviceReader)
            {
                IsBackground = true,
                Name = "PIE Device Reader"
            };
            dataReader.Start();
        }

        /// <summary>
        /// Disposes this instance and stops event listening
        /// </summary>
        public void Dispose()
        {
            var r = dataReader;
            dataReader = null;
            source.Cancel();
            r?.Join();
        }

        /// <summary>
        /// Handles data from a PIE device
        /// </summary>
        /// <param name="data">Data</param>
        /// <param name="sourceDevice">Device that generated <paramref name="data"/></param>
        /// <param name="error">Device errors</param>
        public void HandlePIEHidData(byte[] data, HID.HidPieDevice sourceDevice, int error)
        {
            //Don't read data on error
            if (error != 0)
            {
                Logging.LogDebug("Got Device Error={0}", error);
                return;
            }
            //Failed read
            if (data == null || data.Length != sourceDevice.DeviceInfo.ReadSize)
            {
                Logging.LogDebug("data parameter size not valid");
                return;
            }
            if (data[0] != READ_START || data[data.Length - 1] != READ_END)
            {
                Logging.LogDebug("data parameter doesn't contains valid start and stop bytes");
                return;
            }

            Logging.LogDebug("Data={0}", string.Join("-", data.Select(m => m.ToString("X2"))));

            //Process each analog value byte and raise events when values change
            if (Reverser != data[1])
            {
                Reverser = data[1];
                ButtonChange(this, ButtonType.Reverser);
            }
            if (Throttle != data[2])
            {
                Throttle = data[2];
                ButtonChange(this, ButtonType.Throttle);
            }
            if (TrainBrake != data[3])
            {
                TrainBrake = data[3];
                ButtonChange(this, ButtonType.TrainBrake);
            }
            if (IndependentBrake != data[4])
            {
                IndependentBrake = data[4];
                ButtonChange(this, ButtonType.IndBrake);
            }
            if (IndependentSide != data[5])
            {
                IndependentSide = data[5];
                ButtonChange(this, ButtonType.IndBrakeSide);
            }
            if (Wipers != data[6])
            {
                Wipers = data[6];
                ButtonChange(this, ButtonType.Wiper);
            }
            if (Lights != data[7])
            {
                Lights = data[7];
                ButtonChange(this, ButtonType.Lights);
            }

            //Process digital button states and raise an event when the state changes
            var btn1 = DecodeTopButtons(data);
            var btn2 = DecodeBottomButtons(data);
            if (TopRow != btn1)
            {
                TopRow = btn1;
                ButtonChange(this, ButtonType.TopRow);
            }
            if (BottomRow != btn2)
            {
                BottomRow = btn2;
                ButtonChange(this, ButtonType.BottomRow);
            }
            var ud = DecodeUpDownButtons(data);
            if (UpDown != ud)
            {
                UpDown = ud;
                ButtonChange(this, ButtonType.UpDown);
            }
            var dPad = DecodeCrossButtons(data);
            if (DPad != dPad)
            {
                DPad = dPad;
                ButtonChange(this, ButtonType.Cross);
            }
            var aux = DecodeAuxButtons(data);
            if (Auxiliary != aux)
            {
                Auxiliary = aux;
                ButtonChange(this, ButtonType.Aux);
            }

            Logging.LogDebug("Input event processing complete in {0}", nameof(RDEventHandler));
        }

        private void DeviceReader()
        {
            Logging.LogDebug("Starting device reader thread");
            source = new CancellationTokenSource();
            while (dataReader != null)
            {
                try
                {
                    HandlePIEHidData(device.ReadData(source.Token), device, 0);
                }
                catch (Exception ex)
                {
                    Logging.LogException(ex, "Error in device reader thread");
                }
            }
            Logging.LogDebug("Device reader set to null. Exiting reader thread.");
        }

        /// <summary>
        /// Decodes to 14 buttons
        /// </summary>
        /// <param name="data">PIE data</param>
        /// <returns>Button states</returns>
        private static RowButtons DecodeTopButtons(byte[] data)
        {
            var ret = RowButtons.None;
            ret |= (RowButtons)data[8];
            ret |= (RowButtons)((data[9] & 0x3F) << 8);
            return ret;
        }

        /// <summary>
        /// Decodes lower 14 buttons
        /// </summary>
        /// <param name="data">PIE data</param>
        /// <returns>Button states</returns>
        private static RowButtons DecodeBottomButtons(byte[] data)
        {
            var ret = RowButtons.None;
            ret |= (RowButtons)(data[9] >> 6);
            ret |= (RowButtons)(data[10] << 2);
            ret |= (RowButtons)((data[11] & 0x0F) << 10);
            return ret;
        }

        /// <summary>
        /// Decodes the Up-Down button state
        /// </summary>
        /// <param name="data">PIE data</param>
        /// <returns>Button state</returns>
        private static UpDownButtons DecodeUpDownButtons(byte[] data)
        {
            var ret = UpDownButtons.None;
            ret |= (UpDownButtons)((data[11] >> 4) & 0x3);
            return ret;
        }

        /// <summary>
        /// Decides the D-Pad button state
        /// </summary>
        /// <param name="data">PIE data</param>
        /// <returns>Button state</returns>
        private static CrossButtons DecodeCrossButtons(byte[] data)
        {
            var ret = CrossButtons.None;
            ret |= (CrossButtons)(data[11] >> 6);
            ret |= (CrossButtons)((data[12] & 0x3) << 2);
            return ret;
        }

        /// <summary>
        /// Decodes auxiliary button states
        /// </summary>
        /// <param name="data">PIE data</param>
        /// <returns>Button states</returns>
        private static AuxButtons DecodeAuxButtons(byte[] data)
        {
            var ret = AuxButtons.None;
            ret |= (AuxButtons)(data[12] >> 2);
            ret |= (AuxButtons)((data[13] & 0xF) << 6);
            return ret;
        }
    }
}
