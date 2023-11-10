using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using Plugin.BLE.Abstractions.Extensions;
using Xamarin.Forms;

namespace ESP32FormGenerator.Services
{
    public static class JsonService
    {

        private static ICharacteristic characteristic = null;

        public static List<IDevice> GetDevices()
        {
            var adapter = CrossBluetoothLE.Current.Adapter;
            var devices = adapter.GetSystemConnectedOrPairedDevices().ToList();
            return devices;
        }

        public static async Task Connect(IDevice device)
        {
            var adapter = CrossBluetoothLE.Current.Adapter;
            await adapter.DiscoverDeviceAsync(device.Id);
            try
            {
                await adapter.ConnectToDeviceAsync(device);

                
            }
            catch (DeviceConnectionException e)
            {
                await Console.Out.WriteLineAsync("Could not connect to device");
            }

            try
            {
                var services = await device.GetServicesAsync();
                var characteristics = await services[0].GetCharacteristicsAsync();

                characteristic = characteristics.FirstOrDefault(c => c.Id == device.Id);
            }catch (DeviceConnectionException e)
            {
                await Console.Out.WriteLineAsync("Could not get service");

            }
        }

        public static async void SendMessage(string message)
        {
            if (characteristic != null)
            {
                byte[] dataToSend = Encoding.UTF8.GetBytes(message);
                await characteristic.WriteAsync(dataToSend);
            }
        }
        public static async void ReadData()
        {
            if (characteristic != null)
            {
                var data = await characteristic.ReadAsync();
                if (data.resultCode > 0)
                {
                    await Console.Out.WriteLineAsync(data.data.ToString());
                }
                else
                {
                    await Console.Out.WriteLineAsync($"Something went wrong. Code: {data.resultCode}");
                }
            }
        }

        public static async void Disconnect(IDevice device)
        {
            var adapter = CrossBluetoothLE.Current.Adapter;
            await adapter.DisconnectDeviceAsync(device);
        }
    }
}