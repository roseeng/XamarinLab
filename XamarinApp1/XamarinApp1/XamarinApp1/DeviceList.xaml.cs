using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinApp1
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceList : ContentPage
    {
        BlueMan _ble;

        public DeviceList()
        {
            InitializeComponent();
            _ble = BlueMan.Instance();
            listDevices.ItemsSource = Devices;
            lblScanning.SetBinding(IsVisibleProperty, nameof(this.IsScanning));
        }

        ObservableCollection<BleDevice> _devs;

        public ObservableCollection<BleDevice> Devices
        {
            get
            {
                _devs = new ObservableCollection<BleDevice>(_ble.Devices());
                return _devs;
            }
        }

        bool _timerRunning = false;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _timerRunning = true;
            Device.StartTimer(TimeSpan.FromSeconds(10), UpdateDevices);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _timerRunning = false;
        }

        private bool _IsScanning = false;

        private bool UpdateDevices()
        {
            if (!_timerRunning) return false;

            IsScanning = true;
            lblScanning.IsVisible = true;

            _ble.TryStartScanningAsync(refresh: true);
            listDevices.ItemsSource = null;
            _ = this.Devices;
            listDevices.ItemsSource = _devs;

            IsScanning = false;
            lblScanning.IsVisible = false;
            return true;
        }

        public bool IsScanning
        {
            get
            {
                return _IsScanning;
            }
            set
            {
                _IsScanning = value;
                OnPropertyChanged();
            }
        }
        private void listDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            Debug.WriteLine(((BleDevice)e.SelectedItem).Name);
        }

        async private void listDevices_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            Debug.WriteLine(((BleDevice)e.Item).Name);
            var p = Navigation.NavigationStack.First();
            if (p is MainPage)
            {
                var mp = p as MainPage;
                mp.SelectedDevice = (BleDevice)e.Item;
            }
            await Navigation.PopAsync();
//            await Navigation.PushAsync(new MainPage((BleDevice)e.Item));
        }
    }
}