
using GlomensioApp.Commons;
using GlomensioApp.Model;
using Newtonsoft.Json;
using Plugin.LocalNotification;
using System.Reflection;
using System.Text;
using Timer = System.Timers.Timer;
namespace GlomensioApp.Services;


public class DeviceIotService : IDeviceIotService
{
    private readonly Uri _baseUrl;
    private List<DeviceIot> _devices;
    Timer timer;
    public bool isNotified = true;
    public bool _isProcessingData = false;

    public DeviceIotService()
    {
        _devices = new List<DeviceIot>();
        _baseUrl = new Uri("https://cd.iotsystems-vn.com/api/");
        timer = new System.Timers.Timer(1000);
        timer.Elapsed += async (sender, e) => await LoadDevicesAsync();
        LoadDevicesAsync().GetAwaiter().GetResult();
        timer.Start();


        //#if DEBUG
        //          _baseUrl = new Uri("https://localhost:7029/api/");
        //#endif
    }
    public async Task<List<DeviceIot>> LoadDevicesAsync(bool isReload = false, bool isAuthorize = true)
    {
        try
        {
            if (!await CheckAccessInternet() && isNotified)
            {
                isNotified = false;
                return _devices;
            }

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = _baseUrl;

            using var request = new HttpRequestMessage(HttpMethod.Get, "app/byUser");

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
            if (isAuthorize)
            {
                var accessTokens = Preferences.Get(StorageKey.AccessToken, "");
                if (accessTokens is null || string.IsNullOrWhiteSpace(accessTokens))
                {
                    Console.WriteLine("AccessToken is null or empty.");
                    return new List<DeviceIot>();
                }
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessTokens}");
            }

            HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response != null)
            {
                var responses = JsonConvert.DeserializeObject<List<DeviceIot>>(responseBody) ?? new List<DeviceIot>();

                StaticVariable.Devices = responses.OrderBy(x => x.MacId).ToList();
                if (isReload)
                {
                    _devices.Clear();
                    foreach (var item in responses)
                    {
                        _devices.Add(item);
                    }
                }
                else
                {
                    //_devices = responses;
                    foreach (var device in responses)
                    {
                        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                        if (deviceUpdate == null)
                        {
                            _devices.Add(device);
                        }
                        else
                        {
                            CopyProperties(device, deviceUpdate);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return _devices.OrderBy(x => x.MacId).ToList();
    }

    public async Task<List<DeviceIot>> LoadDevicesForUserAsync(bool isReload = false, bool isAuthorize = true)
    {
        try
        {
            if (!await CheckAccessInternet() && isNotified)
            {
                isNotified = false;
                return _devices;
            }

            using var httpClient = new HttpClient();
            httpClient.BaseAddress = _baseUrl;

            using var request = new HttpRequestMessage(HttpMethod.Get, "byUser");

            request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
            if (isAuthorize)
            {
                var accessTokens = Preferences.Get(StorageKey.AccessToken, "");
                if (accessTokens is null || string.IsNullOrWhiteSpace(accessTokens))
                {
                    Console.WriteLine("AccessToken is null or empty.");
                    return new List<DeviceIot>();
                }
                request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessTokens}");
            }

            HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

            var responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response != null)
            {
                var responses = JsonConvert.DeserializeObject<List<DeviceIot>>(responseBody) ?? new List<DeviceIot>();

                StaticVariable.Devices = responses;
                if (isReload)
                {
                    _devices.Clear();
                    foreach (var item in responses)
                    {
                        _devices.Add(item);
                    }
                }
                else
                {
                    foreach (var device in responses)
                    {
                        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                        if (deviceUpdate == null)
                        {
                            _devices.Add(device);
                        }
                        else
                        {
                            CopyProperties(device, deviceUpdate);
                        }
                    }
                }
            }
        }
        catch (Exception)
        {
        }
        return _devices;
    }

    private void CopyProperties<T>(T source, T target)
    {
        PropertyInfo[] properties = typeof(T).GetProperties();
        foreach (PropertyInfo property in properties)
        {
            if (property.CanRead && property.CanWrite)
            {
                var value = property.GetValue(source);
                property.SetValue(target, value);
            }
        }
    }


    public async Task<DeviceIot> GetDeviceByMacId(string macId)
    {
        // Ensure the devices are loaded before attempting to get a specific device
        if (_devices == null || !_devices.Any())
        {
            await LoadDevicesAsync();
        }

        // Find and return the device with the specified MacId
        return _devices?.FirstOrDefault(d => d.MacId == macId) ?? new DeviceIot();
    }
    public async Task UpdateDeviceAsync(DeviceIot device)
    {
        try
        {
            if (!await CheckAccessInternet())
            {
                return;
            }
            _isProcessingData = true;

            var apiUrl = "device";
            var (isSuccess, response) = await PutAsync(apiUrl, device);
            if (isSuccess)
            {
                var responseSetting = JsonConvert.DeserializeObject<DeviceIot>(response);
                var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                if (deviceUpdate != null)
                {
                    CopyProperties(responseSetting, deviceUpdate);
                }
            }

            _isProcessingData = false;
        }
        catch (Exception)
        {
        }

    }

    public async Task RemoveDeviceAsync(DeviceIot device)
    {
        try
        {
            if (!await CheckAccessInternet())
            {
                return;
            }
            _isProcessingData = true;

            var apiUrl = $"app/{device.MacId}/removeDevice";
            var (isSuccess, response) = await PostAsync(apiUrl, device, false);
            if (isSuccess)
            {
                //var responseSetting = JsonConvert.DeserializeObject<DeviceIot>(response);
                var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                if (deviceUpdate != null)
                {
                    _devices.Remove(deviceUpdate);
                    //CopyProperties(responseSetting, deviceUpdate);
                }
            }
            Console.WriteLine($"API Response: {response}");


            _isProcessingData = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error removing device: {ex.Message}");

        }

    }

    public async Task AddDeviceAsync(DeviceIot device)
    {
        try
        {
            if (!await CheckAccessInternet())
            {
                return;
            }

            _isProcessingData = true;

            var apiUrl = "device";
            var (isSuccess, response) = await PostAsync(apiUrl, device);
            if (isSuccess)
            {
                var responseSetting = JsonConvert.DeserializeObject<DeviceIot>(response);
                var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                if (deviceUpdate != null)
                {
                    CopyProperties(responseSetting, deviceUpdate);
                }
            }
            
            _isProcessingData = true;
         
        }
        catch (Exception)
        {
        }

    }

    public async Task<bool> AddDeviceToUserAsync(DeviceIot device)
    {
        try
        {
            if (!await CheckAccessInternet())
            {
                return false;
            }

            _isProcessingData = true;

            var apiUrl = $"app/{device.MacId}/addDevice";
            var (isSuccess, response) = await PostAsync(apiUrl, device, false);
            if (isSuccess)
            {
                var responseSetting = JsonConvert.DeserializeObject<DeviceIot>(response);
                var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
                if (deviceUpdate != null)
                {
                    CopyProperties(responseSetting, deviceUpdate);
                }
                return true;
            }

            _isProcessingData = true;
            return false;
        }
        catch (Exception)
        {
            return true;
        }

    }

    private async Task<bool> CheckAccessInternet()
    {
        var hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
        if (!hasInternet && isNotified)
        {
            isNotified = false;
            await LocalNotificationCenter.Current.Show(new NotificationRequest()
            {
                NotificationId = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond),
                Title = "Hyperlight disconnected!",
                Description = $"Internet disconnected. Please connect your device to the internet to use the Hyperlight application.",
                Schedule = new NotificationRequestSchedule()
                {
                    NotifyTime = DateTime.Now.AddSeconds(1)
                }
            });
            return false;
        }
        isNotified = true;
        return true;
    }

    //public async Task OnReloadData(DeviceIot device)
    //{
    //    isNotified = true;
    //    LoadDevices(true).GetAwaiter().GetResult();
    //}

    private async Task OnOpenWifiSettings(DeviceIot device)
    {
        await Launcher.Default.OpenAsync(new Uri("http://10.10.0.1/"));
    }

    public async Task OnChangeEmg(DeviceIot device)
    {
        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
        if (deviceUpdate != null)
        {
            isNotified = true;
            deviceUpdate.EmgState = device.EmgState == 0 ? 1 : 0;
            await UpdateDeviceAsync(deviceUpdate);
            await LoadDevicesAsync();
        }
    }
    public async Task OnChangeK(DeviceIot device)
    {
        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
        if (deviceUpdate != null)
        {
            isNotified = true;
            deviceUpdate.K = device.K;
            await UpdateDeviceAsync(deviceUpdate);
            await LoadDevicesAsync();
        }
    }


    public async Task OnChangeEnableOTA(DeviceIot device)
    {
        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
        if (deviceUpdate != null)
        {
            isNotified = true;
            deviceUpdate.EnableOTA = device.EnableOTA = !deviceUpdate.EnableOTA;
            await UpdateDeviceAsync(deviceUpdate);
            await LoadDevicesAsync();
        }
    }

    public async Task OnBlinkDevice(DeviceIot device)
    {
        var deviceUpdate = _devices.FirstOrDefault(x => x.MacId == device.MacId);
        if (deviceUpdate != null)
        {
            isNotified = true;
            deviceUpdate.Blink = device.Blink = deviceUpdate.Blink == 1 ? 0 : 1;
            await UpdateDeviceAsync(deviceUpdate);
            await LoadDevicesAsync();
        }
    }
    private async Task<(bool, string)> PutAsync<TRequest>(string apiUrl, TRequest requestModel)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = _baseUrl;
        string jsonContent = JsonConvert.SerializeObject(requestModel);

        using var request = new HttpRequestMessage(HttpMethod.Put, apiUrl);
        var accessTokens = Preferences.Get(StorageKey.AccessToken, "");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessTokens}");

        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return (true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        return (false, string.Empty);
    }

    private async Task<(bool, string)> PostAsync<TRequest>(string apiUrl, TRequest requestModel, bool haveContent = true)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = _baseUrl;
        string jsonContent = JsonConvert.SerializeObject(requestModel);

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        var accessTokens = Preferences.Get(StorageKey.AccessToken, "");
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessTokens}");

        if (haveContent)
        {
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return (true, await response.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
        else       
          return (false, string.Empty);
        
    
    }
}
