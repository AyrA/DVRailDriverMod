using DVRailDriverMod.Interface;

namespace DVRailDriverMod
{
    public class RailDriverFinder : SingletonBehaviour<RailDriverFinder>
    {
        public static new bool AllowAutoCreate() { return true; }
        public RailDriverBase RailDriver { get; internal set; }

        internal RailDriverFinder()
        {
            if (!enabled)
            {
                Awake();
            }
        }
    }
}
