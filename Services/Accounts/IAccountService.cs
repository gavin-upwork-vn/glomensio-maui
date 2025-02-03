using GlomensioApp.Services.Accounts.Models;

namespace GlomensioApp.Services.Accounts;

public interface IAccountService
{
    Task<LoginResponse?> LoginAsync(LoginRequest loginRequest);
    Task<RegisterResponse?> RegisterAsync(RegisterRequest registerRequest);

    Task<ForgotPasswordResponse> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest);

    Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
}
