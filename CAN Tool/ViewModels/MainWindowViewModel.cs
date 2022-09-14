﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAN_Tool.ViewModels.Base;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;
using CAN_Tool.Infrastructure.Commands;
using System.IO.Ports;
using Can_Adapter;
using AdversCan;
using System.Windows.Threading;
using System.Threading;
using System.Windows.Data;
using System.Globalization;

namespace CAN_Tool.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {

        int[] _Bitrates => new int[] { 20, 50, 125, 250, 500, 800, 1000 };

        public int[] Bitrates => _Bitrates;

        CanAdapter _canAdapter;
        public CanAdapter canAdapter { get => _canAdapter; }

        AC2P _AC2PInstance;
        public AC2P AC2PInstance => _AC2PInstance;

        ConnectedDevice _connectedDevice;
        public ConnectedDevice SelectedConnectedDevice
        {
            set =>
                Set(ref _connectedDevice, value);
            get => _connectedDevice;
        }

        public Dictionary<CommandId, AC2PCommand> Commands => AC2P.commands;
        #region SelectedMessage
        private AC2PMessage selectedMessage;

        public AC2PMessage SelectedMessage
        {
            get => selectedMessage;
            set => Set(ref selectedMessage, value);
        }
        #endregion

        #region  CustomMessage

        AC2PMessage customMessage = new AC2PMessage();

        public Dictionary<CommandId, AC2PCommand> CommandList { get; } = AC2P.commands;

        public AC2PParameter SelectedParameter { set; get; } = new();
        public AC2PMessage CustomMessage { get => customMessage; set => customMessage.Update(value); }

        public double[] CommandParametersArray;


        #endregion

        #region ErrorString
        private string error;

        public string Error
        {
            get { return error; }
            set { Set(ref error, value); }
        }
        #endregion

        #region PortName;
        private string portName = "";
        public string PortName
        {
            get => portName;
            set => Set(ref portName, value);
        }
        #endregion

        #region PortList
        private BindingList<string> _PortList = new BindingList<string>();
        public BindingList<string> PortList
        {
            get => _PortList;
            set => Set(ref _PortList, value);
        }
        #endregion

        #region Commands

        #region CanAdapterCommands

        #region SetAdapterNormalModeCommand

        public ICommand SetAdapterNormalModeCommand { get; }

        private void OnSetAdapterNormalModeCommandExecuted(object Parameter) => canAdapter.StartNormal();
        private bool CanSetAdapterNormalModeCommandExecute(object Parameter) => canAdapter.PortOpened;
        #endregion

        #region SetAdapterListedModeCommand

        public ICommand SetAdapterListedModeCommand { get; }

        private void OnSetAdapterListedModeCommandExecuted(object Parameter) => canAdapter.StartListen();
        private bool CanSetAdapterListedModeCommandExecute(object Parameter) => canAdapter.PortOpened;
        #endregion

        #region SetAdapterSelfReceptionModeCommand

        public ICommand SetAdapterSelfReceptionModeCommand { get; }

        private void OnSetAdapterSelfReceptionModeCommandExecuted(object Parameter) => canAdapter.StartSelfReception();
        private bool CanSetAdapterSelfReceptionModeCommandExecute(object Parameter) => canAdapter.PortOpened;
        #endregion

        #region StopCanAdapterCommand

        public ICommand StopCanAdapterCommand { get; }

        private void OnStopCanAdapterCommandExecuted(object Parameter) => canAdapter.Stop();
        private bool CanStopCanAdapterCommandExecute(object Parameter) => canAdapter.PortOpened;
        #endregion

        #region RefreshPortsCommand
        public ICommand RefreshPortListCommand { get; }
        private void OnRefreshPortsCommandExecuted(object Parameter)
        {
            PortList.Clear();
            foreach (var port in SerialPort.GetPortNames())
                PortList.Add(port);
            if (PortList.Count > 0)
                PortName = PortList[0];
        }

        #endregion

        #region OpenPortCommand
        public ICommand OpenPortCommand { get; }
        private void OnOpenPortCommandExecuted(object parameter)
        {
            canAdapter.PortName = PortName;
            canAdapter.PortOpen();
        }
        private bool CanOpenPortCommandExecute(object parameter) => (PortName.StartsWith("COM") && !canAdapter.PortOpened);
        #endregion

        #region ClosePortCommand
        public ICommand ClosePortCommand { get; }
        private void OnClosePortCommandExecuted(object parameter)
        {
            canAdapter.PortClose();
        }
        private bool CanClosePortCommandExecute(object parameter) => (canAdapter.PortOpened);
        #endregion
        #endregion

        #region CloseApplicationCommand
        public ICommand CloseApplicationCommand { get; }
        private void OnCloseApplicationCommandExecuted(object parameter)
        {
            App.Current.Shutdown();
        }
        private bool CanCloseApplicationCommandExecute(object parameter) => true;
        #endregion

        #region HeaterCommands

        #region StartHeaterCommand
        public ICommand StartHeaterCommand { get; }
        private void OnStartHeaterCommandExecuted(object parameter)
        {
            executeCommand(1, 0xff, 0xff);
        }
        private bool CanStartHeaterCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #region StopHeaterCommand
        public ICommand StopHeaterCommand { get; }
        private void OnStopHeaterCommandExecuted(object parameter)
        {
            executeCommand(3);
        }
        private bool CanStopHeaterCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #region StartPumpCommand
        public ICommand StartPumpCommand { get; }
        private void OnStartPumpCommandExecuted(object parameter)
        {
            executeCommand(4, 0, 0);
        }
        private bool CanStartPumpCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #region ClearErrorsCommand
        public ICommand ClearErrorsCommand { get; }
        private void OnClearErrorsCommandExecuted(object parameter)
        {
            executeCommand(5);
        }
        private bool CanClearErrorsCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #region StartVentCommand
        public ICommand StartVentCommand { get; }
        private void OnStartVentCommandExecuted(object parameter)
        {
            executeCommand(10);
        }
        private bool CanStartVentCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #region CalibrateTermocouplesCommand

        public ICommand CalibrateTermocouplesCommand { get; }
        private void OnCalibrateTermocouplesCommandExecuted(object parameter)
        {
            executeCommand(20);
        }


        private bool CanCalibrateTermocouplesCommandExecute(object parameter) => canAdapter.PortOpened && SelectedConnectedDevice != null;
        #endregion

        #endregion

        #region ChartCommands

        #region ChartStartCommand
        public ICommand ChartStartCommand { get; }
        private void OnChartStartCommandExecuted(object parameter)
        {
            
        }
        private bool CanChartStartCommandExecute(object parameter) => (AC2PInstance.CurrentTask.Occupied);
        #endregion


        #endregion

        #region CancelOperationCommand
        public ICommand CancelOperationCommand { get; }
        private void OnCancelOperationCommandExecuted(object parameter)
        {
            AC2PInstance.CurrentTask.onCancel();
        }
        private bool CanCancelOperationCommandExecute(object parameter) => (AC2PInstance.CurrentTask.Occupied);
        #endregion

        #region ConfigCommands
        #region ReadConfigCommand
        public ICommand ReadConfigCommand { get; }
        private void OnReadConfigCommandExecuted(object parameter)
        {
            AC2PInstance.ReadAllParameters(_connectedDevice.ID);
        }
        private bool CanReadConfigCommandExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion

        #region SaveConfigCommand
        public ICommand SaveConfigCommand { get; }
        private void OnSaveConfigCommandExecuted(object parameter)
        {
            AC2PInstance.SaveParameters(_connectedDevice.ID);
        }
        private bool CanSaveConfigCommandExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied && SelectedConnectedDevice.readedParameters.Count > 0);
        #endregion

        #region ResetConfigCommand
        public ICommand ResetConfigCommand { get; }
        private void OnResetConfigCommandExecuted(object parameter)
        {
            AC2PInstance.ResetParameters(_connectedDevice.ID);
        }
        private bool CanResetConfigCommandExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion
        #endregion

        #region BlackBoxCommands
        #region ReadBlackBoxDataCommand
        public ICommand ReadBlackBoxDataCommand { get; }
        private void OnReadBlackBoxDataCommandExecuted(object parameter)
        {
            AC2PInstance.ReadBlackBoxData(_connectedDevice.ID);
        }
        private bool CanReadBlackBoxDataExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion

        #region ReadBlackBoxErrorsCommand
        public ICommand ReadBlackBoxErrorsCommand { get; }
        private void OnReadBlackBoxErrorsCommandExecuted(object parameter)
        {
            Task.Run(() => AC2PInstance.ReadErrorsBlackBox(_connectedDevice.ID));
        }
        private bool CanReadBlackBoxErrorsExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion

        #region EraseBlackBoxErrorsCommand
        public ICommand EraseBlackBoxErrorsCommand { get; }
        private void OnEraseBlackBoxErrorsCommandExecuted(object parameter)
        {
            Task.Run(() => AC2PInstance.EraseErrorsBlackBox(_connectedDevice.ID));
        }
        private bool CanEraseBlackBoxErrorsExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion

        #region EraseBlackBoxDataCommand
        public ICommand EraseBlackBoxDataCommand { get; }
        private void OnEraseBlackBoxDataCommandExecuted(object parameter)
        {
            Task.Run(() => AC2PInstance.EraseErrorsBlackBox(_connectedDevice.ID));
        }
        private bool CanEraseBlackBoxDataExecute(object parameter) =>
            (canAdapter.PortOpened && SelectedConnectedDevice != null && !AC2PInstance.CurrentTask.Occupied);
        #endregion
        #endregion

        #region SendCustomMessageCommand
        public ICommand SendCustomMessageCommand { get; }
        private void OnSendCustomMessageCommandExecuted(object parameter)
        {
            CustomMessage.TransmitterId = new DeviceId(126, 6);
            CustomMessage.ReceiverId = SelectedConnectedDevice.ID;
            AC2PInstance.SendMessage(CustomMessage);
        }
        private bool CanSendCustomMessageCommandExecute(object parameter)
        {
            if (!canAdapter.PortOpened || SelectedConnectedDevice == null) return false;
            return true;
        }

        #endregion
        private void executeCommand(byte num, params byte[] data)
        {
            CustomMessage.TransmitterType = 126;
            CustomMessage.TransmitterAddress = 6;
            CustomMessage.ReceiverId = SelectedConnectedDevice.ID;
            CustomMessage.PGN = 1;
            CustomMessage.Data = new byte[8];
            for (int i = 0; i < data.Length; i++)
                customMessage.Data[i + 2] = data[i];
            CustomMessage.Data[1] = num;
            canAdapter.Transmit(customMessage);
        }

        #region Chart

        DispatcherTimer dispatcherTimer = new DispatcherTimer();

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            foreach (var d in AC2PInstance.ConnectedDevices)
            {
                if (d.ChartLogging)
                    d.ChartTick();
            }
        }


        #endregion
        #endregion
        public MainWindowViewModel()
        {
            AC2P.ParseParamsname();
            _canAdapter = new CanAdapter();
            _AC2PInstance = new AC2P(canAdapter);

            

            OpenPortCommand = new LambdaCommand(OnOpenPortCommandExecuted, CanOpenPortCommandExecute);
            ClosePortCommand = new LambdaCommand(OnClosePortCommandExecuted, CanClosePortCommandExecute);
            RefreshPortListCommand = new LambdaCommand(OnRefreshPortsCommandExecuted);
            ReadConfigCommand = new LambdaCommand(OnReadConfigCommandExecuted, CanReadConfigCommandExecute);
            ReadBlackBoxDataCommand = new LambdaCommand(OnReadBlackBoxDataCommandExecuted, CanReadBlackBoxDataExecute);
            ReadBlackBoxErrorsCommand = new LambdaCommand(OnReadBlackBoxErrorsCommandExecuted, CanReadBlackBoxErrorsExecute);
            EraseBlackBoxErrorsCommand = new LambdaCommand(OnEraseBlackBoxErrorsCommandExecuted, CanEraseBlackBoxErrorsExecute);
            EraseBlackBoxDataCommand = new LambdaCommand(OnEraseBlackBoxDataCommandExecuted, CanEraseBlackBoxDataExecute);
            SendCustomMessageCommand = new LambdaCommand(OnSendCustomMessageCommandExecuted, CanSendCustomMessageCommandExecute);
            CancelOperationCommand = new LambdaCommand(OnCancelOperationCommandExecuted, CanCancelOperationCommandExecute);
            SaveConfigCommand = new LambdaCommand(OnSaveConfigCommandExecuted, CanSaveConfigCommandExecute);
            ResetConfigCommand = new LambdaCommand(OnResetConfigCommandExecuted, CanResetConfigCommandExecute);
            SetAdapterNormalModeCommand = new LambdaCommand(OnSetAdapterNormalModeCommandExecuted, CanSetAdapterNormalModeCommandExecute);
            SetAdapterListedModeCommand = new LambdaCommand(OnSetAdapterListedModeCommandExecuted, CanSetAdapterListedModeCommandExecute);
            SetAdapterSelfReceptionModeCommand = new LambdaCommand(OnSetAdapterSelfReceptionModeCommandExecuted, CanSetAdapterSelfReceptionModeCommandExecute);
            StopCanAdapterCommand = new LambdaCommand(OnStopCanAdapterCommandExecuted, CanStopCanAdapterCommandExecute);
            CloseApplicationCommand = new LambdaCommand(OnCloseApplicationCommandExecuted, CanCloseApplicationCommandExecute);
            StartHeaterCommand = new LambdaCommand(OnStartHeaterCommandExecuted,CanStartHeaterCommandExecute);
            StopHeaterCommand = new LambdaCommand(OnStopHeaterCommandExecuted,CanStopHeaterCommandExecute);
            StartPumpCommand = new LambdaCommand(OnStartPumpCommandExecuted, CanStartPumpCommandExecute);
            StartVentCommand = new LambdaCommand(OnStartVentCommandExecuted, CanStartVentCommandExecute);
            ClearErrorsCommand = new LambdaCommand(OnClearErrorsCommandExecuted, CanClearErrorsCommandExecute);
            CalibrateTermocouplesCommand = new LambdaCommand(OnCalibrateTermocouplesCommandExecuted, CanCalibrateTermocouplesCommandExecute);
            CustomMessage.TransmitterAddress = 6;
            CustomMessage.TransmitterType = 126;

            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
            dispatcherTimer.Start();
        }
    }
}
