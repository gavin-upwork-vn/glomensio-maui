namespace GlomensioApp.Services.Accounts.Models
{
    public class ResetPasswordResponse
    {
        public bool Success { get; set; }
        public List<string>? Errors { get; set; }
    }
}
