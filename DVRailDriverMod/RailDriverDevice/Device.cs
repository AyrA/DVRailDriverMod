using DVRailDriverMod.Interface.Calibration;
using DVRailDriverMod.Interface.Enums;
using System;
using System.Linq;

namespace DVRailDriverMod.RailDriverDevice
{
    /// <summary>
    /// Represents a raildriver device
    /// </summary>
    internal sealed class Device : IDisposable
    {
        /// <summary>
        /// Device id for the RailDriver
        /// </summary>
        private const int VID = 0x05F3;

        /// <summary>
        /// Command to set speaker state
        /// </summary>
        private const byte SpeakerCommand = 133;

        /// <summary>
        /// Event that is fired whenever a button/lever value changes
        /// </summary>
        public event Action<Device, ButtonType> Input = delegate { };

        /// <summary>
        /// Exception thrown for when <see cref="Start"/> has not been called yet
        /// </summary>
        private static InvalidOperationException NotStarted =>
            new InvalidOperationException($"{nameof(Start)}() has not been called yet");

        /// <summary>
        /// Handles RailDriver events
        /// </summary>
        private RDEventHandler handler;

        /// <summary>
        /// Sets display values
        /// </summary>
        internal LED LED { get; private set; }

        /// <summary>
        /// Gets if the device is open
        /// </summary>
        public bool IsOpen => device.IsOpen;

        /// <summary>
        /// RailDriver device
        /// </summary>
        private readonly HID.HidPieDevice device;

        /// <summary>
        /// Lever calibration data
        /// </summary>
        public CalibrationData CalibrationData { get; }

        #region forwarded levers and buttons

        /// <summary>
        /// Gets the state of the 14 buttons in the top row
        /// </summary>
        public RowButtons ButtonsTop => handler?.TopRow ?? throw NotStarted;

        /// <summary>
        /// Gets the state of the 14 buttons in the bottom row
        /// </summary>
        public RowButtons ButtonsBottom => handler?.BottomRow ?? throw NotStarted;

        /// <summary>
        /// Gets the state of the D-Pad
        /// </summary>
        public CrossButtons DPad => handler?.DPad ?? throw NotStarted;

        /// <summary>
        /// Gets the state of the Up-Down button next to the D-Pad
        /// </summary>
        public UpDownButtons UpDown => handler?.UpDown ?? throw NotStarted;

        /// <summary>
        /// Gets the auxiliary buttons in the top left corner
        /// </summary>
        /// <remarks>This includes the horn</remarks>
        public AuxButtons AuxButtons => handler?.Auxiliary ?? throw NotStarted;

        /// <summary>
        /// Gets the raw reverser value
        /// </summary>
        public byte RawReverser => handler?.Reverser ?? throw NotStarted;

        /// <summary>
        /// Gets the parsed reverser value
        /// </summary>
        /// <remarks>Ranges from -1 (Rev) to +1 (Fwd). 0.0 is the neutral position</remarks>
        public double ParsedReverser => CalibrationData.ReverserCalibration.GetValue(RawReverser);

        /// <summary>
        /// Gets the parsed reverser value suitable for a tristate reverser type
        /// </summary>
        public ReverserPosition TriStateReverser => (ReverserPosition)CalibrationData.ReverserCalibration.GetTriState(RawReverser);

        /// <summary>
        /// Gets the raw throttle value
        /// </summary>
        public byte RawThrottle => handler?.Throttle ?? throw NotStarted;

        /// <summary>
        /// Gets the parsed throttle value
        /// </summary>
        /// <remarks>
        /// Ranges from -1.0 (Full brake) to +1.0 (full throttle). 0.0 is the gate position
        /// </remarks>
        public double ParsedThrottle => CalibrationData.ThrottleCalibration.GetValue(RawThrottle);

        /// <summary>
        /// Gets the raw train brake value
        /// </summary>
        public byte RawTrainBrake => handler?.TrainBrake ?? throw NotStarted;

        /// <summary>
        /// Gets the parsed train brake value
        /// </summary>
        /// <remarks>
        /// Ranges from 0.0 (Released) to 1.0 (fully applied).
        /// -1.0 represents the emergency setting
        /// </remarks>
        public double ParsedTrainBrake => CalibrationData.TrainBrakeCalibration.GetValue(RawTrainBrake);

        /// <summary>
        /// Gets the raw independent brake value
        /// </summary>
        public byte RawIndBrake => handler?.IndependentBrake ?? throw NotStarted;
        /// <summary>
        /// Gets the parsed independent brake value
        /// </summary>
        /// <remarks>Ranges from 0.0 (Released) to 1.0 (fully applied)</remarks>
        public double ParsedIndBrake => CalibrationData.IndBrakeCalibration.GetValue(RawIndBrake);

        /// <summary>
        /// Gets the raw wipers button
        /// </summary>
        /// <remarks>This is an analog value even though the button has 3 distinct values</remarks>
        public byte RawWipers => handler?.Wipers ?? throw NotStarted;
        /// <summary>
        /// Gets the parsed wiper values
        /// </summary>
        /// <remarks>
        /// 0.0 is off, 0.5 is half, 1.0 is full</remarks>
        public double ParsedWipers => CalibrationData.WiperCalibration.GetValue(RawWipers);

        /// <summary>
        /// Gets the raw lights button
        /// </summary>
        public byte RawLights => handler?.Lights ?? throw NotStarted;
        /// <summary>
        /// Gets the parsed lights button
        /// </summary>
        /// <remarks>0.0 is off, 0.5 is dim, 1.0 is full</remarks>
        public double ParsedLights => CalibrationData.LightsCalibration.GetValue(RawLights);

        #endregion

        /// <summary>
        /// Initializes a new device handler
        /// </summary>
        /// <param name="data">Calibration data. Can be null if no data is available</param>
        /// <remarks>This will initialize the first device it finds</remarks>
        /// <exception cref="InvalidOperationException">Device not found or initialization error</exception>
        public Device(CalibrationData data)
        {
            CalibrationData = data ?? new CalibrationData();
            try
            {
                device = new HID.HidPieDevice(HID.HidPieDeviceFinder.FindPieDevices().First())
                {
                    SuppressIdenticalInputs = true
                };
                Logging.LogInfo("Using device '{0}'", device.DeviceInfo.Path);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No PIE devices found", ex);
            }
        }

        /// <summary>
        /// Starts device operation
        /// </summary>
        /// <remarks>
        /// This must be called for the properites to have sensible values assigned.
        /// Will also start event processing and raising from the device
        /// </remarks>
        /// <exception cref="InvalidOperationException">Device already started</exception>
        public void Start()
        {
            if (handler != null)
            {
                throw new InvalidOperationException("Device already started");
            }
            Logging.LogInfo("Device started: Path={0}", device.DeviceInfo.Path);
            device.Open();
            LED = new LED(device);
            handler = new RDEventHandler(device);
            handler.ButtonChange += Handler_ButtonChange;
            handler.Start();
        }

        /// <summary>
        /// Enables and disables the speaker
        /// </summary>
        /// <param name="enabled">true, to enable, false to disable</param>
        public void SetSpeakerState(bool enabled)
        {
            if (handler == null)
            {
                throw NotStarted;
            }
            Logging.LogInfo("Setting speaker state to '{0}'", enabled);
            var data = new byte[device.DeviceInfo.WriteSize];
            data[1] = SpeakerCommand;
            data[7] = (byte)(enabled ? 1 : 0);
            device.WriteData(data);
        }

        /// <summary>
        /// Stops device operation and disposes this instance
        /// </summary>
        public void Dispose()
        {
            var h = handler;
            var l = LED;
            if (l != null)
            {
                Logging.LogInfo("Disposing LED instance");
                l.Dispose();
                LED = null;
            }
            if (h != null)
            {
                Logging.LogInfo("Disposing device handler instance");
                h.Dispose();
                handler = null;
            }
            device.Close();
        }

        /// <summary>
        /// Button state changed
        /// </summary>
        /// <param name="sender"><see cref="handler"/></param>
        /// <param name="buttonType">Button value that changed</param>
        private void Handler_ButtonChange(RDEventHandler sender, ButtonType buttonType)
        {
            Logging.LogDebug("Got button event for button '{0}'", buttonType);
            Input(this, buttonType);
        }
    }
}
