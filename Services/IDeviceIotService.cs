using GlomensioApp.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GlomensioApp.Services
{
    public interface IDeviceIotService
    {
        Task<List<DeviceIot>> LoadDevicesAsync(bool isReload = false, bool isAuthorize = true);
        //    Task<List<DeviceIot>> LoadDevices(bool isReload = false);
        Task UpdateDeviceAsync(DeviceIot device);

        Task AddDeviceAsync(DeviceIot device);
        Task<DeviceIot> GetDeviceByMacId(string macId);
        Task OnChangeEmg(DeviceIot device);
        Task OnChangeEnableOTA(DeviceIot device);
        Task OnBlinkDevice(DeviceIot device);
        //Task AddDeviceToUserAsync(DeviceIot device);
        Task<bool> AddDeviceToUserAsync(DeviceIot device);
        Task RemoveDeviceAsync(DeviceIot device);
    }
}
