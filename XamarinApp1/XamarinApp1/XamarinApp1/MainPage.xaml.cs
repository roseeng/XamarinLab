
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace XamarinApp1
{
    public partial class MainPage : ContentPage
    {
        BlueMan _ble;

        public MainPage()
        {
            InitializeComponent();
            _ble = BlueMan.Instance();
        }
        
        bool _timerRunning = false;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _timerRunning = true;
            Device.StartTimer(TimeSpan.FromSeconds(5), WaitForBleActive);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            _timerRunning = false;
        }

        private bool WaitForBleActive()
        {
            if (!_timerRunning) return false;

            status.Text = _ble.GetStateText();

            if (_ble.HasBluetooth())
            {
                myButton.IsEnabled = true;
                UpdateButtonImage();
                return false;
            }
            else
            {
                return true;
            }
        }

        private int _ImageButtonStatus = 0;
        private void ImageButton_Clicked(object sender, EventArgs e)
        {
            _ImageButtonStatus = (_ImageButtonStatus + 1) % 3;

            UpdateButtonImage();

        }

        private void UpdateButtonImage()
        {
            var image = "grey.png";

            switch (_ImageButtonStatus)
            {
                case 0:
                    image = "green.png";
                    break;
                case 1:
                    image = "yellow.png";
                    break;
                case 2:
                    image = "red.png";
                    break;
            }

            myButton.Source = ImageSource.FromResource("XamarinApp1." + image, typeof(MainPage).GetTypeInfo().Assembly);
        }

        async private void Button_Clicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DeviceList());
        }
    }
}
