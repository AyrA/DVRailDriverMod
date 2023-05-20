using System.IO;

namespace DVRailDriverMod.Interface.Calibration
{
    /// <summary>
    /// Train brake calibration values
    /// </summary>
    public class TrainBrakeCalibration : ICalibratedValue, IStreamSerializable
    {
        /// <summary>
        /// Gets or sets the minimum brake value
        /// </summary>
        public byte BrakeMin { get; set; } = 204;
        /// <summary>
        /// Gets or sets the maximum brake value
        /// </summary>
        public byte BrakeMax { get; set; } = 84;
        /// <summary>
        /// Gets or sets the emergency brake value
        /// </summary>
        public byte BrakeEmg { get; set; } = 70;

        /// <summary>
        /// Gets or sets if the min brake value should be adjusted if the value is out of bounds
        /// </summary>
        /// <remarks>
        /// This will not adjust the maximum brake value
        /// because it would render the <see cref="BrakeEmg"/> setting unreachable
        /// </remarks>
        public bool AutoTune { get; set; } = false;

        public void Deserialize(Stream source)
        {
            using (var BR = new BinaryReader(source, System.Text.Encoding.Default, true))
            {
                BrakeMin = BR.ReadByte();
                BrakeMax = BR.ReadByte();
                BrakeEmg = BR.ReadByte();
            }
        }

        /// <summary>
        /// Gets the train brake value
        /// </summary>
        /// <param name="value">Raw value</param>
        /// <returns>Ranges from 0.0 (release) to 1.0 (full), or jumps to -1.0 if in emergency position</returns>
        public double GetValue(byte value)
        {
            if (value < BrakeEmg)
            {
                return -1.0;
            }
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
                return 1.0;
            }
            return 1.0 - ((value - BrakeMax) * 1.0 / (BrakeMin - BrakeMax));
        }

        public void Serialize(Stream destination)
        {
            using (var BW = new BinaryWriter(destination, System.Text.Encoding.Default, true))
            {
                BW.Write(BrakeMin);
                BW.Write(BrakeMax);
                BW.Write(BrakeEmg);
            }
        }
    }
}
