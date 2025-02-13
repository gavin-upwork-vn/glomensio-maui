using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Telephony;
using AndroidX.Core.App;
using GlomensioApp.Commons;
using GlomensioApp.Model;
using Newtonsoft.Json;
using Plugin.LocalNotification;
using System.Text;
using System.Timers;

namespace GlomensioApp.Platforms.Android.BackgroundServices
{
    [Service(ForegroundServiceType = ForegroundService.TypePhoneCall, Exported = false)]
    public class DetectCallingService : Service
    {
        private string NOTIFICATION_CHANNEL_ID = "1000";
        private int NOTIFICATION_ID = 1;
        private string NOTIFICATION_CHANNEL_NAME = "notification";
        private TelephonyManager telephonyManager;
        private PhoneStateListener phoneStateListener;
        private System.Timers.Timer checkListenerTimer;
        private const int NotificationId = 1001;
        //public void startForeground()
        //{

        //}

        public override void OnCreate()
        {
            base.OnCreate();

            //StopForeground(true);

            StartForegroundService();

            InitializeTelephonyManager();

            StartCheckListenerTimer();
        }

        private void StartForegroundService()
        {
            try
            {
                var notifcationManager = GetSystemService(NotificationService) as NotificationManager;

                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    createNotificationChannel(notifcationManager);
                }

                var notification = new NotificationCompat.Builder(this, NOTIFICATION_CHANNEL_ID);
                notification.SetAutoCancel(false);
                notification.SetOngoing(true);
                notification.SetSmallIcon(Microsoft.Maui.Resource.Mipmap.appicon);
                notification.SetContentTitle("Glomensio");
                notification.SetContentText("Glomensio Service is running");

                if (Build.VERSION.SdkInt >= BuildVersionCodes.Q)
                {
#pragma warning disable CA1416
                    StartForeground(NOTIFICATION_ID, notification.Build(), ForegroundService.TypePhoneCall);
#pragma warning restore CA1416
                }
                else
                {
                    StartForeground(NOTIFICATION_ID, notification.Build());
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions (log them if necessary)
            }
        }

        private void createNotificationChannel(NotificationManager notificationMnaManager)
        {
            var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, NOTIFICATION_CHANNEL_NAME,
            NotificationImportance.Low);
            notificationMnaManager.CreateNotificationChannel(channel);
        }

        private void StartCheckListenerTimer()
        {
            checkListenerTimer = new System.Timers.Timer(10000); // 10 seconds interval
            checkListenerTimer.Elapsed += CheckListenerStatus;
            checkListenerTimer.AutoReset = true;
            checkListenerTimer.Enabled = true;
        }

        private void CheckListenerStatus(object sender, ElapsedEventArgs e)
        {
            try
            {
                try
                {
                    var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

                    using var httpClient = new HttpClient();
                    httpClient.BaseAddress = new Uri("https://cd.iotsystems-vn.com/api/");

                    using var request = new HttpRequestMessage(HttpMethod.Get, "device");

                    request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));

                    HttpResponseMessage response = httpClient.SendAsync(request).ConfigureAwait(false).GetAwaiter().GetResult();

                    var responseBody = response.Content.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                    if (response != null)
                    {
                        var responses = JsonConvert.DeserializeObject<List<DeviceIot>>(responseBody);
                    }
                }
                catch (Exception)
                {
                }

                // Check if the listener is active
                if (telephonyManager == null || phoneStateListener == null)
                {
                    InitializeTelephonyManager();
                }
                else
                {
                    telephonyManager.Listen(phoneStateListener, PhoneStateListenerFlags.CallState);
                }
            }
            catch (Exception ex)
            {
                // Log or handle exception
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private void InitializeTelephonyManager()
        {
            try
            {
                if (telephonyManager == null)
                {
                    telephonyManager = (TelephonyManager)GetSystemService(TelephonyService);
                }

                if (phoneStateListener == null)
                {
                    phoneStateListener = new MyPhoneStateListener();
                }

                telephonyManager.Listen(phoneStateListener, PhoneStateListenerFlags.CallState);
            }
            catch (Exception ex)
            {
                // Log or handle exception
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }


        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            return StartCommandResult.Sticky;
            // Your background task logic here


        }

        public override IBinder OnBind(Intent intent)
        {
            return null;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            // Stop any ongoing tasks, threads, or timers.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            {
                StopForeground(StopForegroundFlags.Remove);
            }
            else
            {
                StopForeground(true); // Stops the service from being foreground.
            }
        }

    }
    public class MyPhoneStateListener : PhoneStateListener
    {
        public async override void OnCallStateChanged(CallState state, string incomingNumber)
        {
            //base.OnCallStateChanged(state, incomingNumber);
            if (string.IsNullOrEmpty(incomingNumber))
            {
                return;
            }

            if (state == CallState.Offhook)
            {
                try
                {


                    var deviceEmg = StaticVariable.Devices.Where(x => x.PhoneActive.Equals(incomingNumber)).ToList();
                    if (deviceEmg.Count > 0)
                    {
                        await LocalNotificationCenter.Current.Show(new NotificationRequest()
                        {
                            NotificationId = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                            Title = "Glomensio SOS",
                            Subtitle = incomingNumber,
                            Description = $"The system has just detected an emergency call to {incomingNumber} from this device. Click here to view details.",
                            Schedule = new NotificationRequestSchedule()
                            {
                                NotifyTime = DateTime.Now.AddSeconds(1)
                            }
                        });
                    }

                    foreach (var item in deviceEmg)
                    {
                        item.EmgState = 1;
                        await UpdateDeviceAsync(item);
                    }
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                }
            }
        }

        private async Task UpdateDeviceAsync(DeviceIot device)
        {
            try
            {
                var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                if (!hasInternet)
                {
                    await LocalNotificationCenter.Current.Show(new NotificationRequest()
                    {
                        NotificationId = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                        Title = "Glomensio disconnected!",
                        Description = $"Internet disconnected. Please connect your device to the internet to use the Glomensio application.",
                        Schedule = new NotificationRequestSchedule()
                        {
                            NotifyTime = DateTime.Now.AddSeconds(1)
                        }
                    });
                    return;
                }
                using var httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri("https://cd.iotsystems-vn.com/api/");
                string jsonContent = JsonConvert.SerializeObject(device);

                using var request = new HttpRequestMessage(HttpMethod.Put, "device");
                request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
                request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

                request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
            }
            catch (Exception)
            {
            }

        }
    }
}
