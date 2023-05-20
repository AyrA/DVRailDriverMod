using System;

namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// Values for the D-Pad
    /// </summary>
    /// <remarks>
    /// In theory the D-Pad can send opposite values.
    /// Diagonal values are possible by regular means
    /// </remarks>
    [Flags]
    public enum CrossButtons : byte
    {
        /// <summary>
        /// No direction is pressed
        /// </summary>
        None = 0,
        /// <summary>
        /// Up direction
        /// </summary>
        Up = 1,
        /// <summary>
        /// Right direction
        /// </summary>
        Right = Up << 1,
        /// <summary>
        /// Down direction
        /// </summary>
        Down = Right << 1,
        /// <summary>
        /// Left direction
        /// </summary>
        Left = Down << 1
    }
}
