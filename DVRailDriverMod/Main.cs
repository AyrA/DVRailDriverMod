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
            Logging.LogInfo("Called {0} on {1}", nameof(Load), typeof(Main).FullName);
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
            Logging.LogInfo("Called {0} on {1}", nameof(OnUnload), typeof(Main).FullName);
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
            Logging.LogInfo("Called {0}({2}) on {1}", nameof(OnToggle), typeof(Main).FullName, value);
            if (value)
            {
                if (adapter == null)
                {
                    adapter = new RailDriverAdapter();
                    adapter.Start();
                }
                else
                {
                    Logging.LogWarning("RD adapter already loaded");
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
                else
                {
                    Logging.LogWarning("RD adapter already unloaded");
                }
            }
            return true;
        }
    }
}
