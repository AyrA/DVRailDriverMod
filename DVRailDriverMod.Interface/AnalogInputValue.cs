using System;

namespace DVRailDriverMod.Interface
{
    /// <summary>
    /// Represents the value from an analog control
    /// </summary>
    public class AnalogInputValue : ICloneable, IEquatable<AnalogInputValue>
    {
        /// <summary>
        /// Gets or sets the value that was processed using current calibration data
        /// </summary>
        /// <remarks>
        /// The value depends on the lever, but usually ranges from 0.0 to 1.0 or from -1.0 to 1.0.
        /// Setting the value outside of the range could have unintended consequences.
        /// </remarks>
        public double ProcessedValue { get; set; }

        /// <summary>
        /// Gets or sets the raw, unprocessed value from the RD controller
        /// </summary>
        /// <remarks>
        /// This is for informational purposes only.
        /// Internal mod behavior ignores this value.
        /// </remarks>
        public byte RawValue { get; set; }

        /// <summary>
        /// Creates an empty instance
        /// </summary>
        public AnalogInputValue() : this(0.0, 0) { }

        /// <summary>
        /// Creates an instance using the given values
        /// </summary>
        /// <param name="processedValue">Value from calibration data</param>
        /// <param name="rawValue">Raw device value</param>
        public AnalogInputValue(double processedValue, byte rawValue)
        {
            ProcessedValue = processedValue;
            RawValue = rawValue;
        }

        /// <summary>
        /// Creates a copy of this instance
        /// </summary>
        /// <returns>Copy</returns>
        public object Clone()
        {
            return new AnalogInputValue(ProcessedValue, RawValue);
        }

        /// <summary>
        /// Checks if the values of this instance equal the values of another instance
        /// </summary>
        /// <param name="otherValues">Other instance</param>
        /// <returns>true, if identical, false if not, or if <paramref name="otherValues"/> is null</returns>
        public bool Equals(AnalogInputValue otherValues)
        {
            if (otherValues == null)
            {
                return false;
            }
            return otherValues.RawValue == RawValue &&
                otherValues.ProcessedValue == ProcessedValue;
        }
    }
}
