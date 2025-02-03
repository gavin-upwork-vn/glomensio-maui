using GlomensioApp.Model;

namespace GlomensioApp.Commons;

public class StaticVariable
{
    public static List<DeviceIot> Devices = new List<DeviceIot>();

    public static string Email { get; set; }
}
