using System;

namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// 7-segment values
    /// </summary>
    [Flags]
    public enum Segment : byte
    {
        /// <summary>
        /// No value. All segments off
        /// </summary>
        None = 0,
        /// <summary>
        /// Top horizontal bar
        /// </summary>
        Top = 1,
        /// <summary>
        /// Middle horizontal bar
        /// </summary>
        Middle = 64,
        /// <summary>
        /// Bottom horizontal bar
        /// </summary>
        Bottom = 8,
        /// <summary>
        /// Top left vertical bar
        /// </summary>
        TopLeft = 32,
        /// <summary>
        /// Bottom left vertical bar
        /// </summary>
        BottomLeft = 16,
        /// <summary>
        /// Top right vertical bar
        /// </summary>
        TopRight = 2,
        /// <summary>
        /// Bottom right vertical bar
        /// </summary>
        BottomRight = 4,
        /// <summary>
        /// The dot
        /// </summary>
        /// <remarks>The dot is located at the bottom right of the segments</remarks>
        Dot = 128,
        /// <summary>
        /// All vertical segments on the left
        /// </summary>
        Left = TopLeft | BottomLeft,
        /// <summary>
        /// All horizontal segments
        /// </summary>
        Center = Top | Middle | Bottom,
        /// <summary>
        /// All vertical segments on the right
        /// </summary>
        Right = TopRight | BottomRight,
        /// <summary>
        /// All segments
        /// </summary>
        /// <remarks>This excludes the dot</remarks>
        All = Left | Center | Right
    }
}
