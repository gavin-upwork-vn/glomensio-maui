using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using GlomensioApp.Platforms.Android.BackgroundServices;

namespace GlomensioApp
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        const int RequestReadPhoneStateId = 0;
        const int RequestIgnoreBatteryOptimizationsId = 1;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.ReadPhoneState) != (int)Permission.Granted)
            {
                ActivityCompat.RequestPermissions(this, new String[]
                {
                    Manifest.Permission.Internet,
                    Manifest.Permission.AccessNetworkState,
                    Manifest.Permission.ReadPhoneState,
                    Manifest.Permission.ReadCallLog,
                    Manifest.Permission.ForegroundService,
                    Manifest.Permission.ForegroundServicePhoneCall,
                    Manifest.Permission.PostNotifications
                }, RequestReadPhoneStateId);
            }
            else
            {
                StartDetectCallingService();
            }

            if (!IsIgnoringBatteryOptimizations())
            {
                ActivityCompat.RequestPermissions(this, new String[] { Manifest.Permission.RequestIgnoreBatteryOptimizations }, RequestIgnoreBatteryOptimizationsId);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            switch (requestCode)
            {
                case RequestReadPhoneStateId:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        StartDetectCallingService();
                    }
                    break;
                case RequestIgnoreBatteryOptimizationsId:
                    if (grantResults.Length > 0 && grantResults[0] == Permission.Granted)
                    {
                        RequestIgnoreBatteryOptimization();
                    }
                    break;
            }
        }

        private void StartDetectCallingService()
        {
            if (!IsServiceRunning(typeof(DetectCallingService)))
            {
                var intent = new Intent(this, typeof(DetectCallingService));
                StartForegroundService(intent);
            }
        }

        private bool IsIgnoringBatteryOptimizations()
        {
            var powerManager = (PowerManager)GetSystemService(Context.PowerService);
            return powerManager.IsIgnoringBatteryOptimizations(PackageName);
        }

        private void RequestIgnoreBatteryOptimization()
        {
            var intent = new Intent();
            intent.SetAction(Android.Provider.Settings.ActionRequestIgnoreBatteryOptimizations);
            intent.SetData(Android.Net.Uri.Parse("package:" + PackageName));
            StartActivity(intent);
        }

        private bool IsServiceRunning(Type serviceType)
        {
            ActivityManager manager = (ActivityManager)GetSystemService(Context.ActivityService);
            foreach (var service in manager.GetRunningServices(int.MaxValue))
            {
                if (service.Service.ClassName.Equals(Java.Lang.Class.FromType(serviceType).CanonicalName))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
