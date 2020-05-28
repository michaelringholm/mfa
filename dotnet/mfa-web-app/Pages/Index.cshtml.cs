using System;
using Google.Authenticator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace mfa_web_app.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> logger;
        private readonly TwoFactorAuthenticator mfaAuthenticator;

        private string accountSecretKey;
        private readonly string accountTitleNoSpaces;
        private readonly bool secretIsBase32;
        private readonly int qrPixelsPerModule;
        private readonly string issuer;

        public string QrCodeSetupImageUrl { get; set; }
        public bool? IsClientMFACodeValid { get; set; }

        public IndexModel(ILogger<IndexModel> logger, TwoFactorAuthenticator mfaAuthenticator, IConfiguration configuration)
        {
            this.logger = logger;
            this.mfaAuthenticator = mfaAuthenticator;
            accountSecretKey = configuration.GetValue<string>("mfaAccountSecretKey"); // "MySecret##!!++$$aaa0001111";
            accountTitleNoSpaces = configuration.GetValue<string>("mfaAccountTitleNoSpaces"); // "OpusMagusMFAWebApp";            
            secretIsBase32 = configuration.GetValue<bool>("mfaSecretIsBase32"); // false;
            qrPixelsPerModule = configuration.GetValue<int>("mfaQrPixelsPerModule"); // 4;
            issuer = configuration.GetValue<string>("mfaIssuer"); // "OpusMagus";
        }

        public void OnGet(string twoFactorCodeFromClient)
        {
           drawQRCode();
        }

        [BindProperty]
        public ClientMFA ClientMFA { get; set; }
        public void OnPost()
        {
            drawQRCode();

            if(!String.IsNullOrEmpty(ClientMFA.MFACode)) {
                IsClientMFACodeValid = mfaAuthenticator.ValidateTwoFactorPIN(accountSecretKey, ClientMFA.MFACode);
                logger.LogInformation($"Client code validation = {IsClientMFACodeValid}");
            }
            else
                logger.LogInformation("No client code found.");
        }

        private void drawQRCode()
        {            
            var setupCode = mfaAuthenticator.GenerateSetupCode(issuer, accountTitleNoSpaces, accountSecretKey, secretIsBase32, qrPixelsPerModule);
            QrCodeSetupImageUrl = setupCode.QrCodeSetupImageUrl;
            logger.LogInformation("Setup code generated.");
        }                
    }
}
