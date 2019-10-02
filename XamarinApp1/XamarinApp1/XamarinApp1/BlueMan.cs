using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.EventArgs;
using Plugin.BLE.Abstractions.Extensions;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace XamarinApp1
{
    /// <summary>
    /// Bluetooth Management
    /// </summary>
    class BlueMan
    {
        IBluetoothLE _ble;
        IAdapter _adapter;

        static BlueMan _instance = null;

        static BlueMan()
        {
        }

        public static BlueMan Instance()
        {
            if (_instance == null)
                _instance = new BlueMan();

            return _instance;
        }

        private BlueMan()
        {
            _ble = CrossBluetoothLE.Current;
            _adapter = CrossBluetoothLE.Current.Adapter;

            _adapter.ScanMode = ScanMode.LowLatency;
            _adapter.DeviceDiscovered += DeviceDiscovered;
        }

        public bool HasBluetooth()
        {
            return _ble.State == BluetoothState.On;
        }

        public string GetStateText()
        {
            switch (_ble.State)
            {
                case BluetoothState.Unknown:
                    return "Unknown BLE state.";
                case BluetoothState.Unavailable:
                    return "BLE is not available on this device.";
                case BluetoothState.Unauthorized:
                    return "You are not allowed to use BLE.";
                case BluetoothState.TurningOn:
                    return "BLE is warming up, please wait.";
                case BluetoothState.On:
                    return "BLE is on.";
                case BluetoothState.TurningOff:
                    return "BLE is turning off. That's sad!";
                case BluetoothState.Off:
                    return "BLE is off. Turn it on!";
                default:
                    return "Unknown BLE state.";
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private List<BleDevice> _devices = new List<BleDevice>();

        public async void TryStartScanningAsync(bool refresh = false)
        {
            if (Xamarin.Forms.Device.RuntimePlatform == Device.Android)
            { 
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Location);
                if (status != PermissionStatus.Granted)
                {
                    var permissionResult = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Location);

                    if (permissionResult.First().Value != PermissionStatus.Granted)
                    {
                        Debug.WriteLine("Permission denied. Not scanning.");
                        CrossPermissions.Current.OpenAppSettings();
                        return;
                    }
                } 
            }

            if (refresh) // this.HasBluetooth() && (refresh || !_devices.Any()) && !_IsScanning)
            {
                ScanForDevices();
            }
        }

        private void DeviceDiscovered(object sender, DeviceEventArgs d)
        {
            if (d.Device.Name != null)
                AddOrUpdateDevice(d.Device);
        }

        private async void ScanForDevices()
        {
            //_devices.Clear();

            foreach (var connectedDevice in _adapter.ConnectedDevices)
            {
                //update rssi for already connected evices (so tha 0 is not shown in the list)
                try
                {
                    await connectedDevice.UpdateRssiAsync();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                    //                    await _userDialogs.AlertAsync($"Failed to update RSSI for {connectedDevice.Name}");
                }

                AddOrUpdateDevice(connectedDevice);
            }

            _cancellationTokenSource = new CancellationTokenSource();

            _IsScanning = true;

            await _adapter.StartScanningForDevicesAsync(_cancellationTokenSource.Token);
        }

        private void AddOrUpdateDevice(IDevice device)
        {
            //InvokeOnMainThread(() =>
            {
                var vm = _devices.FirstOrDefault(d => d.DeviceId == device.Id);
                if (vm != null)
                {
                    vm.Name = device.Name;
                    vm.RSSI = device.Rssi;
                }
                else
                {
                    _devices.Add(new BleDevice(device));
                }
            } //);
        }

        //private int dum = 0;

        public IList<BleDevice> Devices()
        {
            /*0
            switch (dum % 4)
            {
                case 0:
                    _devices.Add(new BleDevice() { Name = "Allan", DeviceId = Guid.NewGuid(), RSSI = 2 });
                    break;
                case 1:
                    _devices.Add(new BleDevice() { Name = "tar", DeviceId = Guid.NewGuid(), RSSI = 12 });
                    break;
                case 2:
                    _devices.Add(new BleDevice() { Name = "Kakan", DeviceId = Guid.NewGuid(), RSSI = 21 });
                    break;
                case 3:
                    _devices.Add(new BleDevice() { Name = "2400", DeviceId = Guid.NewGuid(), RSSI = 7 });
                    break;
            }
            dum += 1;
            */
            return _devices;
        }
    }

    public class BleDevice
    {
        private Guid deviceId;
        private string name;
        private int rSSI;

        public Guid DeviceId { get => deviceId; set => deviceId = value; }
        public string Name { get => name; set => name = value; }
        public int RSSI { get => rSSI; set => rSSI = value; }

        public BleDevice()
        { }

        public BleDevice(IDevice device)
        {
            DeviceId = device.Id;
            Name = device.Name;
            RSSI = device.Rssi;
        }

        public string Info
        {
            get { 
                return name + "  " + rSSI; 
            }
        }
    }
}
