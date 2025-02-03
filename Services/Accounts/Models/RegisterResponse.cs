namespace GlomensioApp.Services.Accounts.Models;

public class RegisterResponse
{
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}
