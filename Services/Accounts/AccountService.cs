using GlomensioApp.Commons;
using GlomensioApp.Services.Accounts.Models;
using Newtonsoft.Json;
using System.Text;

namespace GlomensioApp.Services.Accounts;

public class AccountService : IAccountService
{
    private readonly Uri _baseUrl;
    public AccountService()
    {
        _baseUrl = new Uri("https://cd.iotsystems-vn.com/");
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest loginRequest)
    {
        try
        {
            var apiUrl = "login";

            var (isSuccess, response) = await PostAsync(apiUrl, loginRequest, false);

            if (isSuccess)
            {
                var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response);
                if (loginResponse == null || string.IsNullOrEmpty(loginResponse.AccessToken))
                {
                    return null;
                }

                if (loginRequest.RememberLogin)
                {
                    Preferences.Set(StorageKey.Email, loginRequest.Email);
                    Preferences.Set(StorageKey.Password, loginRequest.Password);
                }
                else
                {
                    Preferences.Set(StorageKey.Email, "");
                    Preferences.Set(StorageKey.Password, "");
                }
                Preferences.Set(StorageKey.RememberLogin, loginRequest.RememberLogin.ToString().ToLower());
                Preferences.Set(StorageKey.AccessToken, "");
                Preferences.Set(StorageKey.AccessToken, loginResponse.AccessToken);

                return loginResponse;
            }

        }
        catch (Exception ex)
        {

            throw;
        }
        return null;
    }

    public async Task<RegisterResponse?> RegisterAsync(RegisterRequest registerRequest)
    {
        var apiUrl = "register";
        var (isSuccess, response) = await PostRegisterAsync(apiUrl, registerRequest);

        if (isSuccess)
        {
            return new RegisterResponse { Success = true };
        }
        else
        {
            // Nếu phản hồi có nội dung lỗi, xử lý như trước
            var errorResponse = JsonConvert.DeserializeObject<ValidationErrorResponse>(response);
            if (errorResponse != null && errorResponse.Errors != null)
            {
                var errors = new List<string>();

                // Xử lý các lỗi cụ thể từ phản hồi
                if (errorResponse.Errors.ContainsKey("DuplicateUserName"))
                {
                    errors.AddRange(errorResponse.Errors["DuplicateUserName"]);
                }
                if (errorResponse.Errors.ContainsKey("PasswordTooShort"))
                {
                    errors.AddRange(errorResponse.Errors["PasswordTooShort"]);
                }
                if (errorResponse.Errors.ContainsKey("PasswordRequiresNonAlphanumeric"))
                {
                    errors.AddRange(errorResponse.Errors["PasswordRequiresNonAlphanumeric"]);
                }
                if (errorResponse.Errors.ContainsKey("PasswordRequiresDigit"))
                {
                    errors.AddRange(errorResponse.Errors["PasswordRequiresDigit"]);
                }
                if (errorResponse.Errors.ContainsKey("PasswordRequiresUpper"))
                {
                    errors.AddRange(errorResponse.Errors["PasswordRequiresUpper"]);
                }

                return new RegisterResponse
                {
                    Success = false,
                    Errors = errors
                };
            }

            return new RegisterResponse { Success = false };
        }
    }

    public async Task<ForgotPasswordResponse?> ForgotPasswordAsync(ForgotPasswordRequest forgotPasswordRequest)
    {
        try
        {
            var apiUrl = "api/account/forgot-password";

            var (isSuccess, response) = await PostAsync(apiUrl, forgotPasswordRequest);

            if (isSuccess)
            {
                return new ForgotPasswordResponse
                {
                    Success = true,
                    Errors = null // Không có lỗi khi thành công
                };
            }
            else
            {
                // Nếu API trả về danh sách lỗi
                var errorResponse = JsonConvert.DeserializeObject<ValidationErrorResponse>(response);

                return new ForgotPasswordResponse
                {
                    Success = false,
                    Errors = errorResponse?.Errors?.SelectMany(e => e.Value).ToList() // Lấy tất cả lỗi từ ErrorResponse
                };
            }
        }
        catch (Exception ex)
        {
            // Trong trường hợp xảy ra lỗi ngoại lệ
            return new ForgotPasswordResponse
            {
                Success = false,
                Errors = new List<string> { $"An error occurred: {ex.Message}" }
            };
        }
    }

    public async Task<ResetPasswordResponse?> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
    {
        try
        {
            var apiUrl = "api/account/reset-password"; // Endpoint cho đặt lại mật khẩu

            var (isSuccess, response) = await PostAsync(apiUrl, resetPasswordRequest);

            if (isSuccess)
            {
                return new ResetPasswordResponse
                {
                    Success = true,
                    Errors = null // Không có lỗi nếu thành công
                };
            }
            else
            {
                // Nếu API trả về danh sách lỗi
                var errorResponse = JsonConvert.DeserializeObject<ValidationErrorResponse>(response);

                return new ResetPasswordResponse
                {
                    Success = false,
                    Errors = errorResponse?.Errors?.SelectMany(e => e.Value).ToList() // Kết hợp tất cả các lỗi
                };
            }
        }
        catch (Exception ex)
        {
            // Trong trường hợp xảy ra ngoại lệ
            return new ResetPasswordResponse
            {
                Success = false,
                Errors = new List<string> { $"An error occurred: {ex.Message}" }
            };
        }
    }



    private async Task<(bool, string)> PostAsync<TRequest>(string apiUrl, TRequest requestModel, bool isAuthorize = false)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = _baseUrl;
        string jsonContent = JsonConvert.SerializeObject(requestModel);

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/octet-stream"));
        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");

        if (isAuthorize)
        {
            var accessToken = Preferences.Get(StorageKey.AccessToken, "");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }

        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);

        if (response.IsSuccessStatusCode)
        {
            string responseBody = await response.Content.ReadAsStringAsync();
            return (true, responseBody);
        }

        return (false, string.Empty);
    }



    private async Task<(bool, string)> PostRegisterAsync<TRequest>(string apiUrl, TRequest requestModel)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = _baseUrl;
        string jsonContent = JsonConvert.SerializeObject(requestModel);

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        var accessToken = await SecureStorage.Default.GetAsync(StorageKey.AccessToken);
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.TryAddWithoutValidation("Content-Type", "application/json");
        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {accessToken}");
        }
        request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await httpClient.SendAsync(request).ConfigureAwait(false);
        if (response.IsSuccessStatusCode)
        {
            return (true, string.Empty);
        }
        var errorContent = await response.Content.ReadAsStringAsync();
        return (false, errorContent);
    }
}
