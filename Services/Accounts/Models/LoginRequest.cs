﻿namespace GlomensioApp.Services.Accounts.Models;

public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }

    public bool RememberLogin { get; set; }
    public string TwoFactorCode { get; set; }
    public string TwoFactorRecoveryCode { get; set; }

}
