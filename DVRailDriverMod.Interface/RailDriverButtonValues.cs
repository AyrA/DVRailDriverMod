using DVRailDriverMod.Interface.Enums;
using System;

namespace DVRailDriverMod.Interface
{
    /// <summary>
    /// Contains all button states of an RD controller
    /// </summary>
    public class RailDriverButtonValues : ICloneable
    {
        /// <summary>
        /// Gets or sets the buttons in the top left corner
        /// </summary>
        public AuxButtons AuxButtons { get; set; }

        /// <summary>
        /// Gets or sets the top row of 14 buttons
        /// </summary>
        public RowButtons TopRowButtons { get; set; }

        /// <summary>
        /// Gets or sets the bottom row of 14 buttons
        /// </summary>
        public RowButtons BottomRowButtons { get; set; }

        /// <summary>
        /// Gets or sets the Up/Down buttons
        /// </summary>
        public UpDownButtons UpDownButtons { get; set; }

        /// <summary>
        /// Gets or sets the D-Pad buttons
        /// </summary>
        public CrossButtons DPad { get; set; }

        /// <summary>
        /// Gets the reverser position limited to directions applicable to modern locos
        /// </summary>
        public ReverserPosition TriStateReverser { get; set; }

        /// <summary>
        /// Gets the reverser value
        /// </summary>
        public AnalogInputValue Reverser { get; private set; }

        /// <summary>
        /// Gets the throttle value
        /// </summary>
        public AnalogInputValue Throttle { get; private set; }

        /// <summary>
        /// Gets the auto brake value
        /// </summary>
        public AnalogInputValue AutoBrake { get; private set; }

        /// <summary>
        /// Gets the independent brake value
        /// </summary>
        public AnalogInputValue IndependentBrake { get; private set; }

        /// <summary>
        /// Gets the wiper knob value
        /// </summary>
        public AnalogInputValue Wiper { get; private set; }

        /// <summary>
        /// Gets the lights knob value
        /// </summary>
        public AnalogInputValue Lights { get; private set; }

        public object Clone()
        {
            var copy = (RailDriverButtonValues)MemberwiseClone();
            copy.Reverser = (AnalogInputValue)copy.Reverser.Clone();
            copy.Throttle = (AnalogInputValue)copy.Throttle.Clone();
            copy.AutoBrake = (AnalogInputValue)copy.AutoBrake.Clone();
            copy.IndependentBrake = (AnalogInputValue)copy.IndependentBrake.Clone();
            copy.Wiper = (AnalogInputValue)copy.Wiper.Clone();
            copy.Lights = (AnalogInputValue)copy.Lights.Clone();
            return copy;
        }
    }
}
