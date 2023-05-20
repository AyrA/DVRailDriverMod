using System;

namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// State of the up/down button
    /// </summary>
    [Flags]
    public enum UpDownButtons : byte
    {
        /// <summary>
        /// No button pressed
        /// </summary>
        None = 0,
        /// <summary>
        /// Button pressed in up position
        /// </summary>
        Up = 1,
        /// <summary>
        /// Button pressed in down position
        /// </summary>
        Down = 2
    }
}
