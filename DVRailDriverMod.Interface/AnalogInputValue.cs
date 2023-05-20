using System;

namespace DVRailDriverMod.Interface
{
    public class AnalogInputValue : ICloneable, IEquatable<AnalogInputValue>
    {
        public double ProcessedValue { get; set; }
        public byte RawValue { get; set; }

        public AnalogInputValue(double processedValue, byte rawValue)
        {
            ProcessedValue = processedValue;
            RawValue = rawValue;
        }

        public object Clone()
        {
            return new AnalogInputValue(ProcessedValue, RawValue);
        }

        public bool Equals(AnalogInputValue other)
        {
            if (other == null)
            {
                return false;
            }
            return other.RawValue == RawValue &&
                other.ProcessedValue == ProcessedValue;
        }
    }
}
