using DVRailDriverMod.HID;
using DVRailDriverMod.Interface.Enums;
using DVRailDriverMod.RailDriverDevice;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DVRailDriverMod
{
    public class RailDriverAdapter : IDisposable
    {
        private LocoControllerBase currentLoco = null;
        private TrainCar currentCar = null;

        private Device dev = null;
        private Thread tUpdateLoop = null;
        private bool isEmergencyBrakeActive = false;

        public void Start()
        {
            if (dev != null)
            {
                return;
            }
            Debug.LogWarning($"{nameof(RailDriverAdapter)}: Started");
            var devices = HidPieDeviceFinder.FindPieDevices();
            Debug.LogWarning(string.Join("\n", devices.Select(m => m.Path)));
            if (devices.Length > 0)
            {
                dev = new Device(DeviceCalibration.GetCalibrationData());
                dev.Input += Dev_Input;
                try
                {
                    dev.Start();
                    Debug.LogWarning("Device is open");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("Unable to open the device: " + ex.Message);
                }
                if (dev.IsOpen)
                {
                    dev.LED.SetText("RDY");
                }
                else
                {
                    Debug.LogError("Device could not be opened");
                }
                PlayerManager.CarChanged += CarChanged;
                tUpdateLoop = new Thread(UpdateLocoStates)
                {
                    IsBackground = true
                };
                tUpdateLoop.Start();
            }
        }

        private void UpdateLocoStates()
        {
            while (tUpdateLoop != null)
            {
                var d = dev;
                var l = currentLoco;
                var diesel = l as LocoControllerDiesel;
                var shunter = l as LocoControllerShunter;
                if (d != null && l != null)
                {
                    if (isEmergencyBrakeActive)
                    {
                        if (l.GetSpeedKmH() < 1.0)
                        {
                            isEmergencyBrakeActive = false;
                            l.SetSanders(0.0f);
                            diesel?.SetEngineRunning(false);
                            shunter?.SetEngineRunning(false);
                            d.LED.ClearMarquee();
                        }
                        else
                        {
                            d.LED.SetMarquee("E-STOP", true);
                        }
                    }
                    else if (l.IsDerailed())
                    {
                        d.LED.SetMarquee("DERAILED", true);
                    }
                    else if (l.IsWheelslipping())
                    {
                        d.LED.SetText("SLP");
                    }
                    else
                    {
                        d.LED.SetNumber(l.GetSpeedKmH());
                    }
                }
                else
                {
                    d.LED.SetText("OFF");
                }
                Thread.Sleep(200);
            }
        }

        private void CarChanged(TrainCar obj)
        {
            if (obj == null)
            {
                //Keep RailDriver connected if we're hooked up with the remote control
                if (currentLoco == null || !currentLoco.IsRemoteControlled())
                {
                    currentLoco = null;
                    currentCar = null;
                }
                return;
            }
            currentCar = obj;
            if (obj.TryGetComponent(out LocoControllerShunter shunter))
            {
                currentLoco = shunter;
            }
            else if (obj.TryGetComponent(out LocoControllerDiesel diesel))
            {
                currentLoco = diesel;
            }
            else if (obj.TryGetComponent(out LocoControllerSteam steam))
            {
                currentLoco = steam;
            }
            else
            {
                currentLoco = null;
            }
            SetInitialValues(currentLoco);
        }

        private void SetInitialValues(LocoControllerBase loco)
        {
            if (dev.DPad != CrossButtons.None)
            {
                //Camera
                float offset = 0.0f;
                if (dev.DPad.HasFlag(CrossButtons.Right))
                {
                    offset = 45f;
                }
                else if (dev.DPad.HasFlag(CrossButtons.Left))
                {
                    offset = 315f;
                }
                if (offset != 0f)
                {
                    var instance = SingletonBehaviour<APlayerTeleport>.Instance;
                    if (instance is PlayerTeleportNonVR)
                    {
                        var c = (instance as PlayerTeleportNonVR).charController;
                        var y = c.m_Camera.transform.rotation.eulerAngles.y;
                        var rotation = Quaternion.Euler(0, y + offset, 0);
                        c.ForceLookRotation(rotation);
                    }
                }
                //Movement
                float move = 0f;
                if (dev.DPad.HasFlag(CrossButtons.Up))
                {
                    move = 1f;
                }
                else if (dev.DPad.HasFlag(CrossButtons.Down))
                {
                    move = -1f;
                }
                if (move != 0f)
                {
                    /* TODO: Let player walk forwards or backwards
                    var instance = SingletonBehaviour<APlayerTeleport>.Instance;
                    if (instance is PlayerTeleportNonVR)
                    {
                        var c = (instance as PlayerTeleportNonVR).charController;
                        var angles = c.m_Camera.transform.rotation.eulerAngles;
                        angles.x = angles.x / 360f * move;
                        angles.y = angles.y / 360f * move;
                        angles.z = angles.z / 360f * move;
                        c.MoveBy(angles);
                    }
                    //*/
                }

            }

            var diesel = loco as LocoControllerDiesel;
            var shunter = loco as LocoControllerShunter;

            if (loco == null)
            {
                return;
            }
            //Switch
            if (dev.ButtonsTop.HasFlag(RowButtons.Button13) || dev.ButtonsTop.HasFlag(RowButtons.Button14))
            {
                System.Diagnostics.Debug.Print("SWITCH: Attempting track switch");
                var bogie = loco.train?.Bogies?.FirstOrDefault(m => !m.HasDerailed);
                if (bogie?.track != null)
                {
                    System.Diagnostics.Debug.Print("SWITCH: Trying to find closest junction");
                    var junction = bogie.track.FindClosestJunction(bogie.gameObject.transform.position, 100f);
                    if (junction != null)
                    {
                        if (bogie.track != junction.inBranch?.track)
                        {
                            System.Diagnostics.Debug.Print("SWITCH: Wrong side of junction {0:X8}", junction.GetInstanceID());
                        }
                        else
                        {
                            try
                            {
                                junction.Switch(Junction.SwitchMode.REGULAR);
                                System.Diagnostics.Debug.Print("SWITCH: Sent switch command to junction {0:X8}", junction.GetInstanceID());
                            }
                            catch (Exception ex)
                            {
                                //Cannot switch this for some reason
                                System.Diagnostics.Debug.Print("SWITCH: Cannot switch junction {0:X8}", junction.GetInstanceID());
                                System.Diagnostics.Debug.Print("SWITCH: {0}", ex.Message);
                            }
                        }
                    }
                    else
                    {
                        System.Diagnostics.Debug.Print("SWITCH: No junction found");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.Print("SWITCH: No track or bogie found");
                }

            }
            //Sand
            if (dev.AuxButtons.HasFlag(AuxButtons.Sand))
            {
                loco.SetSanders(loco.IsSandOn() ? 0.0f : 1.0f);
            }
            //Reverser
            if (loco.analogReverser)
            {
                loco.SetReverser((float)dev.ParsedReverser);
            }
            else
            {
                switch (dev.TriStateReverser)
                {
                    case ReverserPosition.Forward:
                        loco.SetReverser(1.0f);
                        break;
                    case ReverserPosition.Neutral:
                        loco.SetReverser(0.0f);
                        break;
                    case ReverserPosition.Reverse:
                        loco.SetReverser(-1.0f);
                        break;
                }
            }
            //Horn
            if (dev.AuxButtons.HasFlag(AuxButtons.HornDown) || dev.AuxButtons.HasFlag(AuxButtons.HornUp))
            {
                loco.UpdateHorn(1.0f);
            }
            else
            {
                loco.UpdateHorn(0.0f);
            }
            //Handle E-Brake
            if (isEmergencyBrakeActive || dev.ParsedTrainBrake < 0.0 || dev.AuxButtons.HasFlag(AuxButtons.EUp) || dev.AuxButtons.HasFlag(AuxButtons.EDown))
            {
                isEmergencyBrakeActive = true;
                loco.SetThrottle(0.0f);
                loco.SetBrake(1.0f);
                loco.SetIndependentBrake(1.0f);
            }
            else
            {
                if (diesel != null || shunter != null)
                {
                    if (dev.ButtonsTop.HasFlag(RowButtons.Button1))
                    {
                        var running = diesel?.GetEngineRunning() ?? shunter?.GetEngineRunning() ?? false;
                        diesel?.SetEngineRunning(!running);
                        shunter?.SetEngineRunning(!running);
                    }
                }
                if (dev.ParsedThrottle >= 0.0)
                {
                    loco.SetThrottle((float)dev.ParsedThrottle);
                    //Future version: Disable dynamic brake
                }
                else
                {
                    loco.SetThrottle(0.0f);
                    //Future version: Set dynamic brake value
                }
                loco.SetIndependentBrake((float)dev.ParsedIndBrake);
                loco.SetBrake((float)dev.ParsedTrainBrake);
            }
        }

        private void Dev_Input(Device sender, ButtonType buttonType)
        {
            SetInitialValues(currentLoco);
        }

        public void Stop()
        {
            if (dev != null)
            {
                Debug.LogWarning($"{nameof(RailDriverAdapter)}: Stopped");
                var tSpeedOld = tUpdateLoop;
                tUpdateLoop = null;
                tSpeedOld.Join();
                dev.Dispose();
                dev = null;
                PlayerManager.CarChanged -= CarChanged;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
