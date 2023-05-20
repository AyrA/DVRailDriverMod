using System;

namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// Represents the 14 buttons
    /// </summary>
    [Flags]
    public enum RowButtons : int
    {
        /// <summary>
        /// No button is pressed
        /// </summary>
        None = 0,
        /// <summary>
        /// Leftmost button
        /// </summary>
        Button1 = 1,
        Button2 = Button1 << 1,
        Button3 = Button2 << 1,
        Button4 = Button3 << 1,
        Button5 = Button4 << 1,
        Button6 = Button5 << 1,
        Button7 = Button6 << 1,
        Button8 = Button7 << 1,
        Button9 = Button8 << 1,
        Button10 = Button9 << 1,
        Button11 = Button10 << 1,
        Button12 = Button11 << 1,
        Button13 = Button12 << 1,
        /// <summary>
        /// Rightmost button
        /// </summary>
        Button14 = Button13 << 1,
    }
}
