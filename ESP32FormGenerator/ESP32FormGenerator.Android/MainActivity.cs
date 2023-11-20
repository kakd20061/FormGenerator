using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using AndroidX.Core.Content;
using Android;
using Android.Support.V4.App;
using Xamarin.Essentials;
using Android.Bluetooth;
using Android.Widget;
using System.Threading.Tasks;

namespace ESP32FormGenerator.Droid
{
    [Activity(Label = "ESP32FormGenerator", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private int REQUEST_BLUETOOTH = 123;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Platform.Init(this, savedInstanceState);
            Xamarin.Forms.Forms.Init(this, savedInstanceState);

            if(IsBluetoothEnabled())
            {
                RequestBluetoothPermission();
            }
            else
            {
                Toast.MakeText(this, "App cannot work without Bluetooth on", ToastLength.Short).Show();

                FinishAffinity();
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            if (requestCode == REQUEST_BLUETOOTH)
            {
                if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                {
                    LoadApplication(new App());
                }
                else
                {
                    AppInfo.ShowSettingsUI();
                    
                    Toast.MakeText(this, "App cannot work without Bluetooth permission", ToastLength.Long).Show();

                    FinishAffinity();
                }
            }

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        void RequestBluetoothPermission()
        {
            const string permission = Manifest.Permission.BluetoothConnect;
            if (ContextCompat.CheckSelfPermission(this, permission) == (int)Permission.Granted)
            {
                LoadApplication(new App());
            }
            else
            {
                var requiredPermissions = new string[] { Manifest.Permission.BluetoothConnect };
                ActivityCompat.RequestPermissions(this, requiredPermissions, REQUEST_BLUETOOTH);
            }
        }

        bool IsBluetoothEnabled()
        {
            var adapter = BluetoothAdapter.DefaultAdapter;
            return adapter.IsEnabled;
        }
    }
}
