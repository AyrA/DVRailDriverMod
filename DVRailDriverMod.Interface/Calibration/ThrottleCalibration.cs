using System.IO;

namespace DVRailDriverMod.Interface.Calibration
{
    /// <summary>
    /// Calibration data for the throttle lever
    /// </summary>
    /// <remarks>
    /// The throttle lever is special because it has a gate,
    /// and the entire gated area should be 0.0
    /// </remarks>
    public class ThrottleCalibration : ICalibratedValue, IStreamSerializable
    {
        /// <summary>
        /// Gets or sets the maximum throttle value
        /// </summary>
        public byte MaxThrottle { get; set; } = 226;
        /// <summary>
        /// Gets or sets the minimum throttle value
        /// </summary>
        public byte MinThrottle { get; set; } = 176;
        /// <summary>
        /// Gets or sets the maximum brake value
        /// </summary>
        public byte MaxBrake { get; set; } = 59;
        /// <summary>
        /// Gets or sets the minimum brake value
        /// </summary>
        public byte MinBrake { get; set; } = 123;
        /// <summary>
        /// Gets or sets if the maximum brake and throttle values should be automatically adjusted if out of bounds
        /// </summary>
        public bool AutoTune { get; set; } = false;

        public void Deserialize(Stream source)
        {
            using (var BR = new BinaryReader(source, System.Text.Encoding.Default, true))
            {
                MaxThrottle = BR.ReadByte();
                MinThrottle = BR.ReadByte();
                MaxBrake = BR.ReadByte();
                MinBrake = BR.ReadByte();
            }
        }

        /// <summary>
        /// Gets throttle lever position
        /// </summary>
        /// <param name="value">Raw value</param>
        /// <returns>Range -1.0 (full brake) to 0.0 (gated) to 1.0 (full throttle)</returns>
        public double GetValue(byte value)
        {
            if (value > MaxThrottle)
            {
                if (!AutoTune)
                {
                    return 1.0;
                }
                MaxThrottle = value;
            }
            if (value < MaxBrake)
            {
                if (!AutoTune)
                {
                    return -1.0;
                }
                MaxBrake = value;
            }
            if (value <= MinBrake)
            {
                return -1.0 + ((value - MaxBrake) * 1.0 / (MinBrake - MaxBrake));
            }
            if (value > MinThrottle)
            {
                return (value - MinThrottle) * 1.0 / (MaxThrottle - MinThrottle);
            }
            return 0.0;
        }

        public void Serialize(Stream destination)
        {
            using (var BW = new BinaryWriter(destination, System.Text.Encoding.Default, true))
            {
                BW.Write(MaxThrottle);
                BW.Write(MinThrottle);
                BW.Write(MaxBrake);
                BW.Write(MinBrake);
            }
        }
    }
}
