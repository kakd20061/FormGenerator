using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ESP32FormGenerator.Models;
using ESP32FormGenerator.Services;
using Newtonsoft.Json;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace ESP32FormGenerator
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        async void Connect(System.Object sender, System.EventArgs e)
        {
            await Navigation.PushAsync(new MainPage());
        }
    }
}