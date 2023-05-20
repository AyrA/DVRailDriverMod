namespace DVRailDriverMod.Interface
{
    public class AnalogInputValue
    {
        public double ProcessedValue { get; set; }
        public byte RawValue { get; set; }

        public AnalogInputValue(double processedValue, byte rawValue)
        {
            ProcessedValue = processedValue;
            RawValue = rawValue;
        }
    }
}
