﻿using CAN_Tool.ViewModels.Base;
using RVC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CAN_Tool.ViewModels
{
    public enum zoneState_t { Off, Heat, Fan }

    public class ZoneHandler : ViewModel
    {
        public ZoneHandler(Action<ZoneHandler> NotifierAction)
        {
            selectedZoneChanged = NotifierAction;
        }
        private Action<ZoneHandler> selectedZoneChanged;

        private int tempSetpointDay = 22;
        public int TempSetpointDay { set => Set(ref tempSetpointDay, value); get => tempSetpointDay; }

        private int tempSetpointNight = 20;
        public int TempSetpointNight { set => Set(ref tempSetpointNight, value); get => tempSetpointNight; }

        private int tempSetpointCurrent = 20;
        public int TempSetpointCurrent { set => Set(ref tempSetpointCurrent, value); get => tempSetpointCurrent; }

        private int currentTemperature = 0;
        public int CurrentTemperature { set => Set(ref currentTemperature, value); get => currentTemperature; }

        private bool connected = false;
        public bool Connected { set => Set(ref connected, value); get => connected; }

        private zoneState_t state = zoneState_t.Off;
        public zoneState_t State { set => Set(ref state, value); get => state; }

        private bool selected = false;
        public bool Selected { set { Set(ref selected, value); if (value) selectedZoneChanged(this); } get => selected; }

        private bool manualMode = false;
        public bool ManualMode { set => Set(ref manualMode, value); get => manualMode; }

        private int manualPercent = 40;
        public int ManualPercent { set => Set(ref manualPercent, value); get => manualPercent; }

        private int currentPwm = 50;
        public int CurrentPwm { set => Set(ref currentPwm, value); get => currentPwm; }
    }


    public class Timberline20Handler : ViewModel
    {
        private DispatcherTimer timer;

        public event EventHandler NeedToTransmit;

        public void ProcessMesage(RvcMessage msg)
        {
            byte[] D = msg.Data;
            switch (msg.Dgn)
            {
                case 0x1FFF7://Water heater status
                    if (D[0] != 1) return;
                    if (D[1] != 255)
                    {
                        HeaterEnabled = (D[1] & 1) != 0;
                        ElementEnabled = (D[1] & 2) != 0;
                        if (D[4] != 0xFF && D[5] != 0xFF)
                            TankTemperature = (D[4] + D[5] * 256) / 32 - 273;
                    }

                    break;

                /*
            case 0x1FE97://Pump status
                if (D[0] != 1) return;
                if ((D[1]&0xF) != 0xF)
                    switch (D[1]&0xf)
                    {
                        case 0: WaterPumpStatus=pumpStatus_t.off; WaterPumpOverride = false; break;
                        case 1: WaterPumpStatus = pumpStatus_t.on; WaterPumpOverride = false; break;
                        case 5: WaterPumpStatus = pumpStatus_t.overriden; WaterPumpOverride = true; break;
                        default: throw new ArgumentException("Wrong pump status");
                    }

                break;
                */
                case 0x1FFE4://Furnace status
                    if (D[0] != 1) return;
                    if ((D[1] & 3) != 3)
                        ZoneManualFanMode = (D[1] & 3) != 0;
                    if (D[2] != 0xFF)
                        ZoneFanMeasuredSpeed = D[2] / 2;
                    break;
                case 0x1FFE2://Thermostat status 1
                    if (D[0] > 5) return;
                    if ((D[1] & 0xF) != 0xF)
                    {
                        if ((D[1] & 0xF) == 0) Zones[D[0] - 1].State = zoneState_t.Off;
                        if ((D[1] & 0xF) == 2 || (D[1] & 0xF) == 3) Zones[D[0] - 1].State = zoneState_t.Heat;
                        if ((D[1] & 0xF) == 4) Zones[D[0] - 1].State = zoneState_t.Fan;
                    }

                    if (D[3] != 0xFF || D[4] != 0xFF) CurrentSetpoint = (D[3] + D[4] * 256) / 32 - 273;
                    break;

                case 0x1FEF7://Thermostat schedule 1
                    if (D[0] > 5) return;
                    if (D[1] == 0)
                    {
                        if (D[2] < 24 && D[3] < 60)
                            NightStart = new DateTime(1, 1, 1, D[2], D[3], 1);
                        if (D[4] != 0xFF || D[5] != 0xFF) Zones[D[0] - 1].TempSetpointNight = (D[4] + D[5] * 256) / 32 - 273;
                    }
                    if (D[1] == 1)
                    {
                        if (D[2] < 24 && D[3] < 60)
                            DayStart = new DateTime(1, 1, 1, D[2], D[3], 1);
                        if (D[4] != 0xFF || D[5] != 0xFF) Zones[D[0] - 1].TempSetpointDay = (D[4] + D[5] * 256) / 32 - 273;
                    }
                    break;

                case 0x1FF9C://Ambient temperature
                    if (D[0] > 6) return;
                    if (D[1] != 0xFF || D[2] != 0xFF)
                        if (D[0] < 6)
                            Zones[D[0] - 1].CurrentTemperature = (D[1] + D[2] * 256) / 32 - 273;
                        else
                            OutsideTemperature = (D[1] + D[2] * 256) / 32 - 273;
                    break;

                case 0x1EF65: //Proprietary dgn
                    switch (D[0])
                    {
                        case 0x84:
                            if (D[2] != 0xFF || D[3] != 0xFF) TankTemperature = (D[2] + D[3] * 256) / 32 - 273;
                            if (D[4] != 0xFF || D[5] != 0xFF) HeaterTemperature = (D[4] + D[5] * 256) / 32 - 273;
                            if (D[6] != 0xFF) ZoneManualFanSpeed = (byte)(D[6] / 2);
                            break;
                        case 0x85: //Estimated time
                            if (D[1] != 0xFF || D[2] != 0xFF || D[3] != 0xFF) SystemEstimatedTime = D[1] + D[2] * 256 + D[3] * 65536;
                            if (D[4] != 0xFF || D[5] != 0xFF) WaterEstimatedTime = D[4] + D[5] * 256;
                            if (D[6] != 0xFF || D[7] != 0xFF) PumpEstimatedTime = D[6] + D[7] * 256;
                            break;
                        case 0x86: // Heater info
                            if (D[1] != 0xFF || D[2] != 0xFF || D[3] != 0xFF) HeaterTotalMinutes = D[1] + D[2] * 256 + D[3] * 65536;
                            HeaterVersion = new byte[] { D[4], D[5], D[6], D[7] };
                            break;
                        case 0x87: //Panel Info
                            if (D[1] != 0xFF || D[2] != 0xFF || D[3] != 0xFF) PanelMinutesSinceStart = D[1] + D[2] * 256 + D[3] * 65536;
                            PanelVersion = new byte[] { D[4], D[5], D[6], D[7] };
                            break;
                        case 0x88: //HCU info
                            HcuVersion = new byte[] { D[4], D[5], D[6], D[7] };
                            break;
                        case 0x8A: //Timers config status
                            if (D[1] != 0xFF || D[2] != 0xFF) SystemDuration = D[1] + D[2] * 256;
                            if (D[3] != 0xFF) WaterDuration = D[3];
                            break;
                    }

                    break;

            }
        }

        public void ToggleHeater()
        {
            RvcMessage msg = new() { Dgn = 0x1FFF6 };
            msg.Data[0] = 1;
            msg.Data[1] = (byte)(0b11110000 + (!HeaterEnabled ? 1 : 0) + ((ElementEnabled ? 1 : 0) << 1));

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });

        }

        public void ToggleElement()
        {
            RvcMessage msg = new() { Dgn = 0x1FFF6 };
            msg.Data[0] = 1;
            msg.Data[1] = (byte)(0b11110000 + (HeaterEnabled ? 1 : 0) + ((!ElementEnabled ? 1 : 0) << 1));

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        /*
        public void TogglePump()
        {
            
                RvcMessage msg = new() { Dgn = 0x1FE96 };
                msg.Data[0] = 1;
                if (WaterPumpStatus!=pumpStatus_t.overriden)
                    msg.Data[1] = 0b11110101;
                else
                    msg.Data[1] = 0b11110000;

                NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });

        }
        */
        public void ToggleZone()
        {
            RvcMessage msg = new() { Dgn = 0x1FEF9 };
            msg.Data[0] = 1;
            msg.Data[1] = (byte)(0xF0 + (!ZoneEnabled ? 2 : 0));

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void ToggleFanManualMode()
        {
            RvcMessage msg = new() { Dgn = 0x1FFE3 };
            msg.Data[0] = 1;
            msg.Data[1] = (byte)(0b11111100 + (!ZoneManualFanMode ? 1 : 0));

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void ToggleScheduleMode()
        {
            RvcMessage msg = new() { Dgn = 0x1FEF9 };
            msg.Data[0] = 1;
            msg.Data[1] = (byte)(0b00111111 + ((!ScheduleMode ? 1 : 0) << 6));

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetFanManualSpeed(byte speed)
        {
            RvcMessage msg = new() { Dgn = 0x1FFE3 };
            msg.Data[0] = 1;
            msg.Data[2] = (byte)(speed * 2);

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetDaySetpoint(int temp)
        {
            RvcMessage msg = new() { Dgn = 0x1FEF5 };
            msg.Data[0] = 1;
            msg.Data[1] = 1;
            ushort tmp = (ushort)((temp + 273) * 32);
            msg.Data[4] = (byte)(tmp & 0xFF);
            msg.Data[5] = (byte)(tmp >> 8 & 0xFF);

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });

        }

        public void SetNightSetpoint(int temp)
        {
            RvcMessage msg = new() { Dgn = 0x1FEF5 };
            msg.Data[0] = 1;
            msg.Data[1] = 0;
            ushort tmp = (ushort)((temp + 273) * 32);
            msg.Data[4] = (byte)(tmp & 0xFF);
            msg.Data[5] = (byte)(tmp >> 8 & 0xFF);

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetTime(DateTime dateTime)
        {
            RvcMessage msg = new();
            msg.Dgn = 0x1FFFF;
            msg.Priority = 6;
            msg.Data[0] = (byte)(dateTime.Year - 2000);
            msg.Data[1] = (byte)dateTime.Month;
            msg.Data[2] = (byte)dateTime.Day;
            msg.Data[3] = (byte)dateTime.DayOfWeek;
            msg.Data[4] = (byte)dateTime.Hour;
            msg.Data[5] = (byte)dateTime.Minute;
            msg.Data[6] = (byte)dateTime.Second;

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void ClearErrors()
        {
            RvcMessage msg = new();
            msg.Dgn = 0x1EF65;
            msg.Priority = 6;
            msg.Data[0] = 0x81;

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetSystemDuration(int minutes)
        {
            if (minutes < 60) minutes = 60;
            if (minutes > 7200) minutes = 7200;

            RvcMessage msg = new();
            msg.Dgn = 0x1EF65;
            msg.Priority = 6;
            msg.Data[0] = 0x89;
            msg.Data[1] = (byte)(minutes & 0xff);
            msg.Data[2] = (byte)(minutes >> 8 & 0xff);

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetWaterDuration(int minutes)
        {
            if (minutes < 30) minutes = 30;
            if (minutes > 60) minutes = 60;

            RvcMessage msg = new();
            msg.Dgn = 0x1EF65;
            msg.Priority = 6;
            msg.Data[0] = 0x89;
            msg.Data[3] = (byte)(minutes & 0xff);


            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetDayStart(int hours, int minutes)
        {
            if (minutes > 59) minutes = 59;
            if (hours > 23) minutes = 23;

            RvcMessage msg = new();
            msg.Dgn = 0x1FEF5;
            msg.Priority = 6;
            msg.Data[0] = 1;
            msg.Data[1] = 1;
            msg.Data[2] = (byte)hours;
            msg.Data[3] = (byte)minutes;

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void SetNightStart(int hours, int minutes)
        {
            if (minutes > 59) minutes = 59;
            if (hours > 23) minutes = 23;

            RvcMessage msg = new();
            msg.Dgn = 0x1FEF5;
            msg.Priority = 6;
            msg.Data[0] = 1;
            msg.Data[1] = 0;
            msg.Data[2] = (byte)hours;
            msg.Data[3] = (byte)minutes;

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }

        public void OverrideTempSensor(int temperature)
        {
            RvcMessage msg = new();
            msg.Dgn = 0x1FF9C;
            msg.Priority = 6;
            msg.Data[0] = 1;
            ushort tmp = (ushort)((temperature + 273) * 32);
            msg.Data[1] = (byte)tmp;
            msg.Data[2] = (byte)(tmp >> 8);

            NeedToTransmit.Invoke(this, new NeedToTransmitEventArgs() { msgToTransmit = msg });
        }


        public Timberline20Handler()
        {
            hcuVersion = new byte[4];
            heaterVersion = new byte[4];
            panelVersion = new byte[4];

            timer = new();
            timer.Interval = new TimeSpan(0, 0, 5);
            timer.Start();
            timer.Tick += TimerCallback;
        }

        void TimerCallback(object sender, EventArgs e)
        {
            if (BroadcastTemperature)
                OverrideTempSensor(RvcTemperature);
        }

        private int tankTemperature;
        public int TankTemperature { set => Set(ref tankTemperature, value); get => tankTemperature; }

        private int heatExchangerTemperature;
        public int HeatExchangerTemperature { set => Set(ref heatExchangerTemperature, value); get => heatExchangerTemperature; }

        private int heaterTemperature;
        public int HeaterTemperature { set => Set(ref heaterTemperature, value); get => heaterTemperature; }

        private int outsideTemperature;
        public int OutsideTemperature { set => Set(ref outsideTemperature, value); get => outsideTemperature; }

        private pumpStatus_t heaterPumpStatus;
        public pumpStatus_t HeaterPumpStatus { set => Set(ref heaterPumpStatus, value); get => heaterPumpStatus; }

        private pumpStatus_t waterPump1Status;
        public pumpStatus_t WaterPump1Status { set => Set(ref waterPump1Status, value); get => waterPump1Status; }

        private pumpStatus_t waterPump2Status;
        public pumpStatus_t WaterPump2Status { set => Set(ref waterPump2Status, value); get => waterPump2Status; }

        private pumpStatus_t waterPumpAux1Status;
        public pumpStatus_t WaterPumpAux1Status { set => Set(ref waterPumpAux1Status, value); get => waterPumpAux1Status; }

        private pumpStatus_t waterPumpAux2Status;
        public pumpStatus_t WaterPumpAux2Status { set => Set(ref waterPumpAux2Status, value); get => waterPumpAux2Status; }

        private pumpStatus_t waterPumpAux3Status;
        public pumpStatus_t WaterPumpAux3Status { set => Set(ref waterPumpAux3Status, value); get => waterPumpAux3Status; }

        private bool heaterPumpOverride;
        public bool HeaterPumpOverride { set => Set(ref heaterPumpOverride, value); get => heaterPumpOverride; }

        private bool waterPump1Override;
        public bool WaterPump1Override { set => Set(ref waterPump1Override, value); get => waterPump1Override; }

        private bool waterPump2Override;
        public bool WaterPump2Override { set => Set(ref waterPump2Override, value); get => waterPump2Override; }

        private bool waterPumpAux1Override;
        public bool WaterPumpAux1Override { set => Set(ref waterPumpAux1Override, value); get => waterPumpAux1Override; }

        private bool waterPumpAux2Override;
        public bool WaterPumpAux2Override { set => Set(ref waterPumpAux2Override, value); get => waterPumpAux2Override; }

        private bool waterPumpAux3Override;
        public bool WaterPumpAux3Override { set => Set(ref waterPumpAux3Override, value); get => waterPumpAux3Override; }

        private bool heaterEnabled;
        public bool HeaterEnabled { set => Set(ref heaterEnabled, value); get => heaterEnabled; }

        private bool elementEnabled;
        public bool ElementEnabled { set => Set(ref elementEnabled, value); get => elementEnabled; }

        private bool underfloorHeatingEnabled;
        public bool UnderfloorHeatingEnabled { set => Set(ref underfloorHeatingEnabled, value); get => underfloorHeatingEnabled; }

        private bool enginePreheatEnabled;
        public bool EnginePreheatEnabled { set => Set(ref enginePreheatEnabled, value); get => enginePreheatEnabled; }

        private bool domesticWater;
        public bool DomesticWater { set => Set(ref domesticWater, value); get => domesticWater; }

        private BindingList<ZoneHandler> zones;
        public BindingList<ZoneHandler> Zones => zones;

        private int setpointDay;
        public int SetpointDay { set => Set(ref setpointDay, value); get => setpointDay; }

        private int setpointNight;
        public int SetpointNight { set => Set(ref setpointNight, value); get => setpointNight; }

        private int currentSetpoint;
        public int CurrentSetpoint { set => Set(ref currentSetpoint, value); get => currentSetpoint; }

        private byte zoneManualFanSpeed;
        public byte ZoneManualFanSpeed { set => Set(ref zoneManualFanSpeed, value); get => zoneManualFanSpeed; }

        private bool zoneManualFanMode;
        public bool ZoneManualFanMode { set => Set(ref zoneManualFanMode, value); get => zoneManualFanMode; }

        private int zoneFanMeasuredSpeed;
        public int ZoneFanMeasuredSpeed { set => Set(ref zoneFanMeasuredSpeed, value); get => zoneFanMeasuredSpeed; }

        private int systemLimitationTime;
        public int SystemLimitationTime { set => Set(ref systemLimitationTime, value); get => systemLimitationTime; }

        private int waterLimitationTime;
        public int WaterLimitationTime { set => Set(ref waterLimitationTime, value); get => waterLimitationTime; }

        private int systemEstimatedTime;
        public int SystemEstimatedTime { set => Set(ref systemEstimatedTime, value); get => systemEstimatedTime; }

        private int pumpEstimatedTime;
        public int PumpEstimatedTime { set => Set(ref pumpEstimatedTime, value); get => pumpEstimatedTime; }

        private int waterEstimatedTime;
        public int WaterEstimatedTime { set => Set(ref waterEstimatedTime, value); get => waterEstimatedTime; }

        private int panelTimeSinceStart;
        public int PanelMinutesSinceStart { set => Set(ref panelTimeSinceStart, value); get => panelTimeSinceStart; }

        private bool panelSensorOn;
        public bool PanelSensorOn { set => Set(ref panelSensorOn, value); get => panelSensorOn; }

        private int zoneTemperature;
        public int ZoneTemperature { set => Set(ref zoneTemperature, value); get => zoneTemperature; }

        private int heaterTotalMinutes;
        public int HeaterTotalMinutes { set => Set(ref heaterTotalMinutes, value); get => heaterTotalMinutes; }

        private int waterDuration;
        public int WaterDuration { set => Set(ref waterDuration, value); get => waterDuration; }

        private int systemDuration;
        public int SystemDuration { set => Set(ref systemDuration, value); get => systemDuration; }

        private bool broadcastTemperature;
        public bool BroadcastTemperature { set => Set(ref broadcastTemperature, value); get => broadcastTemperature; }

        private bool scheduleMode;
        public bool ScheduleMode { set => Set(ref scheduleMode, value); get => scheduleMode; }

        private int rvcTemperature = 30;
        public int RvcTemperature { set => Set(ref rvcTemperature, value); get => rvcTemperature; }

        private DateTime? dayStart;
        public DateTime? DayStart { set => Set(ref dayStart, value); get => dayStart; }

        private DateTime? nightStart;
        public DateTime? NightStart { set => Set(ref nightStart, value); get => nightStart; }


        private byte[] heaterVersion;
        [AffectsTo(nameof(HeaterVersionString))]
        public byte[] HeaterVersion { set => Set(ref heaterVersion, value); get => heaterVersion; }

        public string HeaterVersionString { get => $"{heaterVersion[0]:D03}.{heaterVersion[1]:D03}.{heaterVersion[2]:D03}.{heaterVersion[3]:D03}"; }

        private byte[] hcuVersion;
        [AffectsTo(nameof(HcuVersionString))]
        public byte[] HcuVersion { set => Set(ref hcuVersion, value); get => hcuVersion; }

        public string HcuVersionString { get => $"{hcuVersion[0]:D03}.{hcuVersion[1]:D03}.{hcuVersion[2]:D03}.{hcuVersion[3]:D03}"; }

        private byte[] panelVersion;
        [AffectsTo(nameof(PanelVersionString))]
        public byte[] PanelVersion { set => Set(ref panelVersion, value); get => panelVersion; }

        public string PanelVersionString { get => $"{panelVersion[0]:D03}.{panelVersion[1]:D03}.{panelVersion[2]:D03}.{panelVersion[3]:D03}"; }

    }

}

