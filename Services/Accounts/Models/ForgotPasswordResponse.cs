﻿namespace GlomensioApp.Services.Accounts.Models
{
    public class ForgotPasswordResponse
    {
        public bool Success { get; set; }
        public List<string>? Errors { get; set; }
    }
}
