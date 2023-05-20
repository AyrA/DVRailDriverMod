using System.IO;

namespace DVRailDriverMod.Calibration
{
    /// <summary>
    /// Calibration data for the reverser lever
    /// </summary>
    internal class ReverserCalibration : ICalibratedValue, IStreamSerializable
    {
        /// <summary>
        /// Gets or sets the reverse position
        /// </summary>
        public byte PosReverse { get; set; } = 221;
        /// <summary>
        /// Gets or sets the forward position
        /// </summary>
        public byte PosForward { get; set; } = 67;
        /// <summary>
        /// Gets or sets the neutral position
        /// </summary>
        public byte PosNeutral { get; set; } = 115;

        /// <summary>
        /// Gets or sets if <see cref="PosForward"/>
        /// and <see cref="PosReverse"/> should be adjusted if out of bounds
        /// </summary>
        public bool AutoTune { get; set; } = false;

        public void Deserialize(Stream source)
        {
            using (var BR = new BinaryReader(source, System.Text.Encoding.Default, true))
            {
                PosReverse = BR.ReadByte();
                PosForward = BR.ReadByte();
                PosNeutral = BR.ReadByte();
            }
        }

        /// <summary>
        /// Gets the normalized reverser value
        /// </summary>
        /// <param name="value">Raw value</param>
        /// <returns>Range -1.0 (rev) to 0.0 (N) to 1.0 (fwd)</returns>
        public double GetValue(byte value)
        {
            if (value > PosReverse)
            {
                if (!AutoTune)
                {
                    return -1.0;
                }
                PosReverse = value;
            }
            if (value < PosForward)
            {
                if (!AutoTune)
                {
                    return 1.0;
                }
                PosForward = value;
            }
            if (value > PosNeutral)
            {
                return (value - PosNeutral) * 1.0 / (PosReverse - PosNeutral) * -1.0;
            }

            return 1.0 - ((value - PosForward) * 1.0 / (PosNeutral - PosForward));
        }

        public void Serialize(Stream destination)
        {
            using (var BW = new BinaryWriter(destination, System.Text.Encoding.Default, true))
            {
                BW.Write(PosReverse);
                BW.Write(PosForward);
                BW.Write(PosNeutral);
            }
        }

        public int GetTriState(byte value)
        {
            var v = GetValue(value);
            if (v < -0.33)
            {
                return -1;
            }
            if (v > 0.33)
            {
                return 1;
            }
            return 0;
        }
    }
}
