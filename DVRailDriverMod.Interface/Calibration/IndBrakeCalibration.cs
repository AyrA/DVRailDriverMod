using System.IO;

namespace DVRailDriverMod.Interface.Calibration
{
    /// <summary>
    /// Calibration values for the independent brake lever
    /// </summary>
    public class IndBrakeCalibration : ICalibratedValue, IStreamSerializable
    {
        /// <summary>
        /// Gets or sets the value for minimum brake application
        /// </summary>
        public byte BrakeMin { get; set; } = 199;
        /// <summary>
        /// Gets or sets the value for maximum brake application
        /// </summary>
        public byte BrakeMax { get; set; } = 37;

        /// <summary>
        /// Allows changing <see cref="BrakeMin"/> and <see cref="BrakeMin"/> if the raw value is out of bounds
        /// </summary>
        public bool AutoTune { get; set; } = false;

        public void Deserialize(Stream source)
        {
            using (var BR = new BinaryReader(source, System.Text.Encoding.Default, true))
            {
                BrakeMin = BR.ReadByte();
                BrakeMax = BR.ReadByte();
            }
        }

        /// <summary>
        /// Gets normalized value
        /// </summary>
        /// <param name="value">raw value</param>
        /// <returns>0.0 (released) to 1.0 (full)</returns>
        public double GetValue(byte value)
        {
            if (value > BrakeMin)
            {
                if (!AutoTune)
                {
                    return 0.0;
                }
                BrakeMin = value;
            }
            if (value < BrakeMax)
            {
                if (!AutoTune)
                {
                    return 1.0;
                }
                BrakeMax = value;
            }
            return 1.0 - ((value - BrakeMax) * 1.0 / (BrakeMin - BrakeMax));
        }

        public void Serialize(Stream destination)
        {
            using (var BW = new BinaryWriter(destination, System.Text.Encoding.Default, true))
            {
                BW.Write(BrakeMin);
                BW.Write(BrakeMax);
            }
        }
    }
}
