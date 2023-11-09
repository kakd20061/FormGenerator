using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;

namespace ESP32FormGenerator.Services
{
    public static class JsonService
    {
        public static List<IDevice> GetDevices()
        {
            var adapter = CrossBluetoothLE.Current.Adapter;
            var devices = adapter.GetSystemConnectedOrPairedDevices().ToList();
            return devices;
        }

        public static async Task Connect(IDevice device)
        {
            var adapter = CrossBluetoothLE.Current.Adapter;

            try
            {
                await adapter.ConnectToDeviceAsync(device);
            }
            catch (DeviceConnectionException e)
            {
                // ... could not connect to device
            }
        }


    }
}