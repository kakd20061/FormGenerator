using System;
using System.Collections.Generic;
using System.Linq;
using Android.Bluetooth;
using Android.Content;
using ESP32FormGenerator.Models;
using ESP32FormGenerator.Services;
using Xamarin.Forms;

namespace FormGenerator
{
    public partial class HomePage : ContentPage
    {
        private ICollection<BluetoothDevice> devices;
        public HomePage()
        {
            InitializeComponent();
            SetPicker(JsonService.GetBondedDevices());
            BindingContext = new LoadingModel(false);
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
            try
            {
                BindingContext = new LoadingModel(true);
                string selectedDeviceName = picker.SelectedItem.ToString();
                
                var item = devices.FirstOrDefault(n => n.Name == selectedDeviceName);
                var connectionResult = await JsonService.Connect(item);
                BindingContext = new LoadingModel(false);

                if(!JsonService._bluetoothAdapter.IsEnabled)
                {
                    var alert = await DisplayAlert("Error", "Bluetooth is disabled", "Enable bluetooth", "Cancel");
                    if(alert) JsonService.OpenBluetoothSettings();
                    return;
                }
                if (connectionResult)
                {
                    await Navigation.PushAsync(new MainPage(item));
                }
                else
                {
                    await DisplayAlert("Error", "Cannot connect selected device", "Ok");
                }
            }
            catch (Exception ex)
            {
                BindingContext = new LoadingModel(false);
                await DisplayAlert("Error", "Cannot connect selected device", "OK");
            }
        }
    }
}