using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Bluetooth;
using Android.Util;
using Java.Util;

namespace ESP32FormGenerator.Services
{
    public static class JsonService
    {
        private static BluetoothDevice _device;
        private static BluetoothSocket _socket;

        public static ICollection<BluetoothDevice> GetBondedDevices()
        {
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter == null || !bluetoothAdapter.IsEnabled)
            {
                return null;
            }

            return bluetoothAdapter.BondedDevices; //Urzadzenia do Selecta
        }

        public static async Task Connect(BluetoothDevice device)
        {
            _device = device;
            BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
            if (bluetoothAdapter == null || !bluetoothAdapter.IsEnabled)
            {
                return;
            }

            try
            {
                await SocketOpen();
            }
            catch (Exception ex)
            {
                Log.Error("CONNECTION", ex.Message);
                await Console.Out.WriteLineAsync("ERROR " + ex.Message);
            }

            await Task.Delay(2000);
        }

        private static async Task SocketOpen()
        {
            _socket = _device.CreateRfcommSocketToServiceRecord(UUID.FromString("00001101-0000-1000-8000-00805f9b34fb"));
            await _socket.ConnectAsync();  
        }

        public static async Task<bool> BluetoothCommand(string message)
        {
            try
            {
                await SocketOpen();
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                if (_socket.IsConnected && _socket.OutputStream != null)
                {
                    await _socket.OutputStream.WriteAsync(bytes, 0, bytes.Length);
                    Disconnect();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync("Error " + ex.Message);
                return false;
            }
        }

        public static async Task<byte[]> BluetoothInput()
        {
            try
            {
                using (Stream inputStream = _socket.InputStream)
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead = await inputStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        byte[] dataReceived = new byte[bytesRead];
                        Array.Copy(buffer, dataReceived, bytesRead);

                        return dataReceived;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading data from Bluetooth: " + ex.Message);
            }

            return null;
        }

        public static void Disconnect()
        {
            _socket.Close();
        }
    }
}