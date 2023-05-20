using DVRailDriverMod.Interface.Enums;

namespace DVRailDriverMod.Interface
{
    public delegate void RailDriverInputChangeEventHandler(object sender, RailDriverEventArgs eventArgs);

    /// <summary>
    /// Provides capabilities to safely interact with a raildriver device
    /// </summary>
    public abstract class RailDriverBase
    {
        public static RailDriverBase Instance { get; protected set; }

        /// <summary>
        /// Event that is fired whenever the control values on the RD controller change
        /// </summary>
        public abstract event RailDriverInputChangeEventHandler RailDriverInputChange;

        /// <summary>
        /// Gets if the display is in custom operation mode.
        /// This automatically happens when calling any of the display functions.
        /// In this mode, the mod will no longer update the display in any way by itself whatsoever,
        /// and leave other mods at full control of the display
        /// </summary>
        /// <remarks>
        /// This can be reset using <see cref="ResetDisplayBehavior"/>
        /// </remarks>
        public bool IsDisplayInCustomMode { get; protected set; }

        /// <summary>
        /// Gets the button state from the device
        /// </summary>
        /// <remarks>
        /// It's usually better to use the <see cref="RailDriverInputChange"/> event.
        /// The device updates states at most 4-5 times per second,
        /// so calling this more often will not have any benefits
        /// </remarks>
        /// <returns>Button states</returns>
        public abstract RailDriverButtonValues GetButtonStates();

        /// <summary>
        /// Sets the given display text.
        /// Automatically scrolls the text if necessary
        /// </summary>
        /// <remarks>
        /// If the text is null or empty, the display reverts to the default behavior.
        /// Instead, use 1-3 spaces to clear the display and leave it that way
        /// </remarks>
        /// <param name="text">Text to display</param>
        public abstract void SetDisplayContents(string text);

        /// <summary>
        /// Shows the given number on the display
        /// </summary>
        /// <param name="number">Number</param>
        /// <remarks>
        /// The number is clamped to always be shown in full.
        /// This effectively limits the range to -99 to 999.
        /// Decimal places are shown whenever there's enough space for them
        /// </remarks>
        public abstract void SetDisplayContents(double number);

        /// <summary>
        /// Manually sets the segments of the display
        /// </summary>
        /// <param name="left">Leftmost segment</param>
        /// <param name="middle">Middle segment</param>
        /// <param name="right">Rightmost segment</param>
        public abstract void SetDisplayContents(Segment left, Segment middle, Segment right);

        /// <summary>
        /// Sets the delay for the text scroll effect
        /// </summary>
        /// <param name="ms">Delay in milliseconds</param>
        /// <remarks>
        /// The loop in the background is only accurate to 100 ms intervals
        /// and will round <paramref name="ms"/> as needed
        /// </remarks>
        public abstract void SetDisplayScrollDelay(int ms);

        /// <summary>
        /// Resets the display to the default mod behavior
        /// </summary>
        public abstract void ResetDisplayBehavior();
    }
}
