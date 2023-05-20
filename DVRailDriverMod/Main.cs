using UnityModManagerNet;

namespace DVRailDriverMod
{
#if DEBUG
    [EnableReloading]
#endif
    public static class Main
    {
        private static RailDriverAdapter adapter = null;

        /// <summary>
        /// Loads the mod
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <returns>true, if load succeeded</returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            modEntry.OnToggle = OnToggle;
            modEntry.OnUnload = OnUnload;
            //modEntry.OnGUI = OnGUI;
            //modEntry.OnSaveGUI = OnSaveGUI;
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
                }
            }
            else
            {
                if (adapter != null)
                {
                    adapter.Stop();
                    adapter.Dispose();
                    adapter = null;
                }
            }
            return true;
        }
    }
}
