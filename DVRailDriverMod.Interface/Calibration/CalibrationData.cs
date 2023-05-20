using System.IO;

namespace DVRailDriverMod.Interface.Calibration
{
    /// <summary>
    /// Represents calibration data used to turn raw device values into normalized values
    /// </summary>
    public class CalibrationData : IStreamSerializable
    {
        /// <summary>
        /// Order in which data is serialized and deserialized
        /// </summary>
        private IStreamSerializable[] Serializable => new IStreamSerializable[]
        {
            ReverserCalibration,
            ThrottleCalibration,
            TrainBrakeCalibration,
            IndBrakeCalibration,
            WiperCalibration,
            LightsCalibration
        };

        /// <summary>
        /// Gets calibration data for the reverser lever
        /// </summary>
        public ReverserCalibration ReverserCalibration { get; private set; }

        /// <summary>
        /// Gets calibration data for the throttle lever
        /// </summary>
        public ThrottleCalibration ThrottleCalibration { get; private set; }

        /// <summary>
        /// Gets calibration data for the train brake lever
        /// </summary>
        public TrainBrakeCalibration TrainBrakeCalibration { get; private set; }

        /// <summary>
        /// Gets calibration data for the independent brake lever
        /// </summary>
        public IndBrakeCalibration IndBrakeCalibration { get; private set; }

        /// <summary>
        /// Gets calibration data for the wiper button
        /// </summary>
        public TriButtonCalibration WiperCalibration { get; private set; }

        /// <summary>
        /// Gets calibration data for the lights button
        /// </summary>
        public TriButtonCalibration LightsCalibration { get; private set; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <remarks>
        /// Default values are applied that should provide an acceptable but possibly less than optimal experience.
        /// </remarks>
        public CalibrationData()
        {
            ReverserCalibration = new ReverserCalibration();
            ThrottleCalibration = new ThrottleCalibration();
            TrainBrakeCalibration = new TrainBrakeCalibration();
            IndBrakeCalibration = new IndBrakeCalibration();
            WiperCalibration = new TriButtonCalibration();
            LightsCalibration = new TriButtonCalibration();
        }

        /// <summary>
        /// Deserializes all calibration data
        /// </summary>
        /// <param name="source">Data source previously saved using <see cref="Serialize(Stream)"/></param>
        public void Deserialize(Stream source)
        {
            foreach (var cal in Serializable)
            {
                cal.Deserialize(source);
            }
        }

        /// <summary>
        /// Serializes all calibration data
        /// </summary>
        /// <param name="destination">Data destination that can later be used in <see cref="Deserialize(Stream)"/></param>
        public void Serialize(Stream destination)
        {
            foreach (var cal in Serializable)
            {
                cal.Serialize(destination);
            }
        }
    }
}
