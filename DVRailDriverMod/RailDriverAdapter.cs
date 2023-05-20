﻿using DVRailDriverMod.HID;
using DVRailDriverMod.Interface;
using DVRailDriverMod.Interface.Enums;
using DVRailDriverMod.RailDriverDevice;
using System;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace DVRailDriverMod
{
    public class RailDriverAdapter : RailDriverBase, IDisposable
    {
        private LocoControllerBase currentLoco = null;
        private TrainCar currentCar = null;

        private Device dev = null;
        private Thread tUpdateLoop = null;
        private bool isEmergencyBrakeActive = false;

        internal void Start()
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
                RailDriverInputChange += RailDriverValueChange;
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
            ApplyRailDriverValues(currentLoco, lastValues);
        }

        private void ApplyRailDriverValues(LocoControllerBase loco, RailDriverButtonValues values)
        {
            if (values.DPad != CrossButtons.None)
            {
                //Camera
                float offset = 0.0f;
                if (values.DPad.HasFlag(CrossButtons.Right))
                {
                    offset = 45f;
                }
                else if (values.DPad.HasFlag(CrossButtons.Left))
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
                if (values.DPad.HasFlag(CrossButtons.Up))
                {
                    move = 1f;
                }
                else if (values.DPad.HasFlag(CrossButtons.Down))
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
            var steamer = loco as LocoControllerSteam;

            //Stuff below is for the locomotive, so we skip it if unnecessary
            if (loco == null)
            {
                return;
            }
            //Switch
            if (values.TopRowButtons.HasFlag(RowButtons.Button14))
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
                    System.Diagnostics.Debug.Print("SWITCH: No track or bogie found. Loco derailed?");
                }

            }
            //Sand
            if (values.AuxButtons.HasFlag(AuxButtons.Sand))
            {
                loco.SetSanders(loco.IsSandOn() ? 0.0f : 1.0f);
            }
            //Reverser
            if (loco.analogReverser)
            {
                loco.SetReverser((float)values.Reverser.ProcessedValue);
            }
            else
            {
                switch (values.TriStateReverser)
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
            if (values.AuxButtons.HasFlag(AuxButtons.HornDown) || values.AuxButtons.HasFlag(AuxButtons.HornUp))
            {
                loco.UpdateHorn(1.0f);
            }
            else
            {
                loco.UpdateHorn(0.0f);
            }
            //Handle E-Brake
            if (isEmergencyBrakeActive || values.AutoBrake.ProcessedValue < 0.0 || values.AuxButtons.HasFlag(AuxButtons.EUp) || values.AuxButtons.HasFlag(AuxButtons.EDown))
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
                    if (values.TopRowButtons.HasFlag(RowButtons.Button1))
                    {
                        var running = diesel?.GetEngineRunning() ?? shunter?.GetEngineRunning() ?? false;
                        diesel?.SetEngineRunning(!running);
                        shunter?.SetEngineRunning(!running);
                    }
                }
                if (values.Throttle.ProcessedValue >= 0.0)
                {
                    loco.SetThrottle((float)values.Throttle.ProcessedValue);
                    //Future version: Disable dynamic brake
                }
                else
                {
                    loco.SetThrottle(0.0f);
                    //Future version: Set dynamic brake value
                }
                loco.SetIndependentBrake((float)values.IndependentBrake.ProcessedValue);
                loco.SetBrake((float)values.AutoBrake.ProcessedValue);
            }
        }

        private void Dev_Input(Device sender, ButtonType buttonType)
        {
            var newValues = new RailDriverButtonValues()
            {
                AuxButtons = sender.AuxButtons,
                BottomRowButtons = sender.ButtonsBottom,
                DPad = sender.DPad,
                TopRowButtons = sender.ButtonsTop,
                UpDownButtons = sender.UpDown
            };
            newValues.AutoBrake.ProcessedValue = sender.ParsedTrainBrake;
            newValues.AutoBrake.RawValue = sender.RawTrainBrake;

            newValues.IndependentBrake.ProcessedValue = sender.ParsedIndBrake;
            newValues.IndependentBrake.RawValue = sender.RawIndBrake;

            newValues.Throttle.ProcessedValue = sender.ParsedThrottle;
            newValues.Throttle.RawValue = sender.RawThrottle;

            newValues.Reverser.ProcessedValue = sender.ParsedReverser;
            newValues.Reverser.RawValue = sender.RawReverser;

            newValues.Wiper.ProcessedValue = sender.ParsedWipers;
            newValues.Wiper.RawValue = sender.RawWipers;

            newValues.Lights.ProcessedValue = sender.ParsedLights;
            newValues.Lights.RawValue = sender.RawLights;

            lastValues = newValues;

            var args = new RailDriverEventArgs(newValues)
            {
                ButtonType = buttonType
            };
            RailDriverInputChange(this, args);
        }

        internal void Stop()
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
                RailDriverInputChange -= RailDriverValueChange;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        private void RailDriverValueChange(object sender, RailDriverEventArgs e)
        {
            if (e.Cancel || e.Handled)
            {
                return;
            }
            ApplyRailDriverValues(currentLoco, e.ButtonValues);
        }

        #region RailDriverBase

        private RailDriverButtonValues lastValues = null;

        public override event RailDriverInputChangeEventHandler RailDriverInputChange;

        public override RailDriverButtonValues GetButtonStates()
        {
            return (RailDriverButtonValues)lastValues.Clone();
        }

        public override void SetDisplayContents(string text)
        {
            dev.LED.ClearMarquee();
            if (string.IsNullOrEmpty(text))
            {
                IsDisplayInCustomMode = false;
            }
            else
            {
                IsDisplayInCustomMode = true;
                dev.LED.SetText(text);
            }
        }

        public override void SetDisplayContents(double number)
        {
            dev.LED.ClearMarquee();
            IsDisplayInCustomMode = true;
            dev.LED.SetNumber(number);
        }

        public override void SetDisplayContents(Segment left, Segment middle, Segment right)
        {
            IsDisplayInCustomMode = true;
            dev.LED.SetLED(left, middle, right);
        }

        public override void SetDisplayScrollDelay(int ms)
        {
            dev.LED.MarqueeDelay = Math.Max(100, ms / 100 * 100);
        }

        public override void ResetDisplayBehavior()
        {
            IsDisplayInCustomMode = false;
        }

        #endregion
    }
}
