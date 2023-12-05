using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using ESP32FormGenerator.Services;
using Xamarin.Forms;

namespace ESP32FormGenerator
{
    public partial class HomePage : ContentPage
    {
        private ICollection<BluetoothDevice> devices;

        public HomePage()
        {
            InitializeComponent();
            SetPicker(JsonService.GetBondedDevices());
        }

        public void SetPicker(ICollection<BluetoothDevice> devices)
        {
            var resultList = new List<string>();
            this.devices = devices;
            foreach (var item in devices)
            {
                resultList.Add(item.Name);
            }
            picker.ItemsSource = resultList;
        }

        async void Connect(object sender, EventArgs e)
        {
            BluetoothDevice item = null;
            try
            {
                string selectedDeviceName = picker.SelectedItem.ToString();

                item = devices.FirstOrDefault(n => n.Name == selectedDeviceName);
                
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", "Cannot connect selected device", "OK");
            }
            
            await JsonService.Connect(item);
            await Navigation.PushAsync(new MainPage(item));
        }
    }
}