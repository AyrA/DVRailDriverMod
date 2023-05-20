using DVRailDriverMod.Interface.Enums;

namespace DVRailDriverMod.Interface
{
    public class RailDriverEventArgs
    {
        public object Tag { get; set; }
        public bool Handled { get; set; }
        public bool Cancel { get; set; }
        public ButtonType ButtonType { get; set; }
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
