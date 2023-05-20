using DVRailDriverMod.Interface.Enums;

namespace DVRailDriverMod.Interface
{
    /// <summary>
    /// Arguments passed to the <see cref="RailDriverBase.RailDriverInputChange"/> event
    /// </summary>
    public class RailDriverEventArgs
    {
        /// <summary>
        /// Gets or sets custom data.
        /// This data is passed to all event handlers that follow
        /// </summary>
        /// <remarks>This is not retained between events, nor is it used by DVRailDriverMod internally</remarks>
        public object Tag { get; set; }
        /// <summary>
        /// Gets or sets whether the event was handled
        /// </summary>
        /// <remarks>
        /// If this is set by a mod, all internal behavior will be skipped
        /// </remarks>
        public bool Handled { get; set; }
        /// <summary>
        /// Gets or sets whether the event should be cancelled
        /// </summary>
        /// <remarks>
        /// If this is set, event handlers should immediately exit without processing the event.
        /// If this is set by a mod, all internal behavior will be skipped.
        /// </remarks>
        public bool Cancel { get; set; }
        /// <summary>
        /// Gets or sets the type of button that triggers the event
        /// </summary>
        public ButtonType ButtonType { get; set; }
        /// <summary>
        /// Gets current values of all buttons and levers
        /// </summary>
        public RailDriverButtonValues ButtonValues { get; }

        public RailDriverEventArgs(RailDriverButtonValues values)
        {
            ButtonValues = values;
            ButtonType = ButtonType.None;
            Cancel = false;
            Handled = false;
        }
    }
}
