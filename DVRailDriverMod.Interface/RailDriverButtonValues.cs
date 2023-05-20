using DVRailDriverMod.Interface.Enums;

namespace DVRailDriverMod.Interface
{
    public class RailDriverButtonValues
    {
        public AuxButtons AuxButtons { get; set; }
        public RowButtons TopRowButtons { get; set; }
        public RowButtons BottomRowButtons { get; set; }
        public UpDownButtons UpDownButtons { get; set; }
        public CrossButtons DPad { get; set; }

        public AnalogInputValue Reverser { get; }
        public AnalogInputValue Throttle { get; }
        public AnalogInputValue AutoBrake { get; }
        public AnalogInputValue IndependentBrake { get; }
        public AnalogInputValue Wiper { get; }
        public AnalogInputValue Lights { get; }
    }
}
