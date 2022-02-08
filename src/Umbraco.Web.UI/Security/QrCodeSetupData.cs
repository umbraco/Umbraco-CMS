using Google.Authenticator;

namespace Umbraco.Cms.Web.UI.Security
{
    public class QrCodeSetupData
    {
        public string Secret { get; init; }
        public SetupCode SetupCode { get; init; }
    }
}
