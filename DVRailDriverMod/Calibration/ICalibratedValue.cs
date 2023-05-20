namespace DVRailDriverMod.Calibration
{
    /// <summary>
    /// Interface for classes that provide normalized values
    /// </summary>
    internal interface ICalibratedValue
    {
        /// <summary>
        /// Converts a raw RailDriver control value into a normalized value
        /// </summary>
        /// <param name="value">Raw value</param>
        /// <returns>Normalized value (usually 0.0 to 1.0)</returns>
        double GetValue(byte value);
    }
}
