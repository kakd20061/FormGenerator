using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESP32FormGenerator.Models;
using ESP32FormGenerator.Services;
using Newtonsoft.Json;
using Plugin.BLE.Abstractions.Contracts;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ESP32FormGenerator
{
    public partial class HomePage : ContentPage
    {
        private List<IDevice> devices;
        public HomePage()
        {
            InitializeComponent();
            SetPicker(JsonService.GetDevices());
        }
        public void SetPicker(List<IDevice> devices)
        {
            var resultList = new List<string>();
            this.devices = devices;
            foreach (var item in devices)
            {
                resultList.Add(item.Name);
            }
            picker.ItemsSource = resultList;
        }
        async void Connect(System.Object sender, System.EventArgs e)
        {
            string selectedDeviceName = picker.SelectedItem.ToString();

            var item = devices.FirstOrDefault(n => n.Name == selectedDeviceName);

            await JsonService.Connect(item);
            await Navigation.PushAsync(new MainPage(item));
        }
    }
}