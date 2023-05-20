namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// Button types that can raise events
    /// </summary>
    public enum ButtonType
    {
        /// <summary>
        /// No button is pushed
        /// </summary>
        None,
        /// <summary>
        /// At least one aux button state changed
        /// </summary>
        Aux,
        /// <summary>
        /// Top 14 button row state changed
        /// </summary>
        TopRow,
        /// <summary>
        /// Bottom 14 button row state changed
        /// </summary>
        BottomRow,
        /// <summary>
        /// Up-Down button state changed
        /// </summary>
        UpDown,
        /// <summary>
        /// D-Pad button state changed
        /// </summary>
        Cross,
        /// <summary>
        /// Reverser value changed
        /// </summary>
        Reverser,
        /// <summary>
        /// Throttle value changed
        /// </summary>
        Throttle,
        /// <summary>
        /// Train brake value changed
        /// </summary>
        TrainBrake,
        /// <summary>
        /// Independent brake value changed
        /// </summary>
        /// <remarks>This does not include the side-to-side movement</remarks>
        IndBrake,
        /// <summary>
        /// Independent brake side-to-side movement changed
        /// </summary>
        /// <remarks>
        /// Due to inadequate build quality, this tends to change even when the lever is not pushed side to side,
        /// but just up and down.
        /// </remarks>
        IndBrakeSide,
        /// <summary>
        /// Wiper button value changed
        /// </summary>
        /// <remarks>For some reason, this is an analog control like the levers</remarks>
        Wiper,
        /// <summary>
        /// Lights button value changed
        /// </summary>
        /// <remarks>For some reason, this is an analog control like the levers</remarks>
        Lights
    }
}
