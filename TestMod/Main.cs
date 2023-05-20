using DVRailDriverMod.Interface;
using DVRailDriverMod.Interface.Enums;
using System;
using System.Diagnostics;
using UnityModManagerNet;

// This is an example mod that you can use as reference when writing your own mods,
// or you can outright copy this to your own project

namespace TestMod
{
#if DEBUG
    [EnableReloading]
#endif
    public static class Main
    {
        private static RailDriverBase adapter = null;

        /// <summary>
        /// Loads the mod
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <returns>true, if load succeeded</returns>
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            //For this simple mod, loading is the same as enabling
            Debug.Print("Called {0} on {1}", nameof(Load), typeof(Main).FullName);
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
            //For this simple mod, unloading is the same as disabling
            Debug.Print("Called {0} on {1}", nameof(OnUnload), typeof(Main).FullName);
            return OnToggle(modEntry, false);
        }

        /// <summary>
        /// Mod status toggle
        /// </summary>
        /// <param name="modEntry">Mod</param>
        /// <param name="value">status</param>
        /// <returns>true, if enabnled, false if disabled</returns>
        static bool OnToggle(UnityModManager.ModEntry modEntry, bool value)
        {
            Debug.Print("Called {1}.{0}({2})", nameof(OnToggle), typeof(Main).FullName, value);
            if (value)
            {
                if (adapter == null)
                {
                    adapter = RailDriverBase.Instance;
                    if (adapter != null)
                    {
                        adapter.RailDriverInputChange += OnInput;
                        Debug.Print("RD adapter event hooked");
                    }
                    else
                    {
                        //This is not supposed to happen
                        Debug.Print("Expected DVRailDriverMod to be loaded but it's not. Aborting load operation now.");
                        Debug.Print("Possible developer error: 'AyrA.DVRailDriverMod' might not be marked as dependency.");
                        Debug.Print("If you're the developer and properly registered the dependency in the info.json file, it might be a mod manager bug");
                        return false;
                    }
                }
                else
                {
                    Debug.Print("RD adapter already loaded. Resetting instance now.");
                    return OnToggle(modEntry, false) && OnToggle(modEntry, true);
                }
            }
            else
            {
                if (adapter != null)
                {
                    adapter.RailDriverInputChange -= OnInput;
                    adapter = null;
                    Debug.Print("RD adapter event unhooked");
                }
                else
                {
                    Debug.Print("RD adapter already unloaded. Doing nothing");
                }
            }
            return true;
        }

        private static void OnInput(object sender, RailDriverEventArgs e)
        {
            var instance = (RailDriverBase)sender;
            var b = e.ButtonValues;
            if (e.ButtonType == ButtonType.BottomRow)
            {
                //This shows how to attach new functionality to the left two buttons in the bottom button row
                if (b.BottomRowButtons.HasFlag(RowButtons.Button1))
                {
                    //Set display to a custom text on button 1 if it hasn't already
                    if (!instance.IsDisplayInCustomMode)
                    {
                        instance.SetDisplayContents("HELLO");
                    }
                    e.Handled = true;
                }
                else if (b.BottomRowButtons.HasFlag(RowButtons.Button2))
                {
                    //Reset the display to the default behavior on button 2
                    instance.ResetDisplayBehavior();
                    e.Handled = true;
                }
            }
            else if (e.ButtonType == ButtonType.TopRow)
            {
                //This shows how to suppress existing functionality without replacing it.
                //It disables the start/stop functionality unless the "P" button is held down.
                //When disabling the behavior it still leaves the start button in the pressed state,
                //giving other mods the ability to use it for other purposes
                if (b.TopRowButtons.HasFlag(RowButtons.Button1))
                {
                    e.Handled = b.AuxButtons.HasFlag(AuxButtons.P);
                }
            }

            var hasSand = b.AuxButtons.HasFlag(AuxButtons.Sand);
            var hasAlert = b.AuxButtons.HasFlag(AuxButtons.Alert);
            //This shows how to re-map buttons by swapping the sand and alert button
            //If both buttons are pressed we do not need to swap them
            if (hasSand != hasAlert)
            {
                if (hasSand)
                {
                    //Add alert and remove sand
                    b.AuxButtons |= AuxButtons.Alert;
                    b.AuxButtons &= ~AuxButtons.Sand;
                }
                if (hasAlert)
                {
                    //remove alert and add sand
                    b.AuxButtons |= AuxButtons.Sand;
                    b.AuxButtons &= ~AuxButtons.Alert;
                }
            }


            //This shows how to change built-in behavior.
            //- It limits the independent brake to half power
            //- It disables all emergency brake behavior via e-stop button and auto brake lever 'EMG' position
            //- It maps the bell button to the horn button because there's no functioning bell in the game yet
            //Note: It's not necessary to change the raw lever values. They're for informational purposes only.
            b.IndependentBrake.ProcessedValue = Math.Min(0.5f, b.IndependentBrake.ProcessedValue);

            //emergency setting is negative
            if (b.AutoBrake.ProcessedValue < 0.0)
            {
                b.AutoBrake.ProcessedValue = 1.0;
            }
            //When bell is held, add horn
            if (b.AuxButtons.HasFlag(AuxButtons.Bell))
            {
                b.AuxButtons |= AuxButtons.HornUp;
            }
            //Completely disable E-Stop button
            b.AuxButtons &= ~(AuxButtons.EUp | AuxButtons.EDown);
            //Note: We do not set the e.Handled or e.Cancel property here.
            //This causes the internal RD mod mechanism to continue working, but with our changed values
        }
    }
}
