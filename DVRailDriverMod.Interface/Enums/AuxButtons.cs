using System;

namespace DVRailDriverMod.Interface.Enums
{
    /// <summary>
    /// Auxiliary button flags
    /// </summary>
    [Flags]
    public enum AuxButtons
    {
        /// <summary>
        /// No aux button is pushed
        /// </summary>
        None = 0,
        /// <summary>
        /// Range pushed up
        /// </summary>
        RangeUp = 1,
        /// <summary>
        /// Range pulled down
        /// </summary>
        RangeDown = RangeUp << 1,
        /// <summary>
        /// E-Stop pushed up
        /// </summary>
        EUp = RangeDown << 1,
        /// <summary>
        /// E-Stop pulled down
        /// </summary>
        EDown = EUp << 1,
        /// <summary>
        /// Alert button push
        /// </summary>
        Alert = EDown << 1,
        /// <summary>
        /// Sand button push
        /// </summary>
        Sand = Alert << 1,
        /// <summary>
        /// P button push
        /// </summary>
        P = Sand << 1,
        /// <summary>
        /// Bell button push
        /// </summary>
        Bell = P << 1,
        /// <summary>
        /// Horn button pushed up
        /// </summary>
        /// <remarks>
        /// Even though physically not possible,
        /// using the horn with too many other aux buttons pushed will
        /// simultaneously activate <see cref="HornUp"/> and <see cref="HornDown"/>
        /// </remarks>
        HornUp = Bell << 1,
        /// <summary>
        /// Horn button pushed down
        /// </summary>
        /// <remarks>
        /// Even though physically not possible,
        /// using the horn with too many other aux buttons pushed will
        /// simultaneously activate <see cref="HornUp"/> and <see cref="HornDown"/>
        /// </remarks>
        HornDown = HornUp << 1
    }
}
