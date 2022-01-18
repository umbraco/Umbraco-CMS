namespace Umbraco.Cms.Core.Models
{
    public class TwoFactorLoginSetupInfo
    {
        public TwoFactorLoginSetupInfo(string secret, string qrCodeUrl)
        {
            Secret = secret;
            QrCodeUrl = qrCodeUrl;
        }

        public string QrCodeUrl { get; }
        public string Secret { get; }
    }
}
