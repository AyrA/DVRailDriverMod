using System;
using System.IO;

namespace DVRailDriverMod.Interface.Calibration
{
    /// <summary>
    /// Calibration data for buttons with 3 states
    /// </summary>
    /// <remarks>This currently is the wiper and lights button</remarks>
    public class TriButtonCalibration : ICalibratedValue, IStreamSerializable
    {
        /// <summary>
        /// Gets or sets the minimum button value
        /// </summary>
        public byte Min { get; set; } = 86;
        /// <summary>
        /// Gets or sets the maximum button value
        /// </summary>
        public byte Max { get; set; } = 180;
        /// <summary>
        /// Gets or sets the middle button value
        /// </summary>
        public byte Middle { get; set; } = 135;

        /// <summary>
        /// Gets or sets if the min and max values should be adjusted if out of bounds
        /// </summary>
        public bool AutoTune { get; set; } = false;

        public void Deserialize(Stream source)
        {
            using (var BR = new BinaryReader(source, System.Text.Encoding.Default, true))
            {
                Min = BR.ReadByte();
                Max = BR.ReadByte();
                Middle = BR.ReadByte();
            }
        }

        /// <summary>
        /// Gets the button value
        /// </summary>
        /// <param name="value">Raw value</param>
        /// <returns>-1.0: left, 0.0: middle, 1.0: right</returns>
        public double GetValue(byte value)
        {
            if (value < Min)
            {
                if (!AutoTune)
                {
                    return -1.0;
                }
                Min = value;
            }
            if (value > Max)
            {
                if (!AutoTune)
                {
                    return 1.0;
                }
                Max = value;
            }
            if (Math.Abs(value - Middle) < 15)
            {
                return 0.0;
            }
            return value < Middle ? -1.0 : 1.0;
        }

        public void Serialize(Stream destination)
        {
            using (var BW = new BinaryWriter(destination, System.Text.Encoding.Default, true))
            {
                BW.Write(Min);
                BW.Write(Max);
                BW.Write(Middle);
            }
        }
    }
}
