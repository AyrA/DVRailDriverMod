using UnityModManagerNet;

namespace DVRailDriverMod
{
#if DEBUG
    [EnableReloading]
#endif
    public static class Main
    {
        public static readonly RailDriverFinder finder;

        private static RailDriverAdapter adapter = null;

        static Main()
        {
            finder = new RailDriverFinder();
        }

        /// <summary>
        /// Loads the mod
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <returns>true, if load succeeded</returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            modEntry.OnUnload = OnUnload;
            return true;
        }

        /// <summary>
        /// Mod unload
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <returns>true, if unloaded</returns>
        static bool OnUnload(UnityModManager.ModEntry modEntry)
        {
            return OnToggle(modEntry, false);
        }

        /// <summary>
        /// Mod status toggle
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <param name="value">status</param>
        /// <returns>true, if toggled</returns>
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            if (value)
            {
                if (adapter == null)
                {
                    adapter = new RailDriverAdapter();
                    adapter.Start();
                    finder.RailDriver = adapter;
                }
            }
            else
            {
                if (adapter != null)
                {
                    finder.RailDriver = null;
                    adapter.Stop();
                    adapter.Dispose();
                    adapter = null;
                }
            }
            return true;
        }
    }
}
