using Umbraco.Core.Configuration;

namespace Umbraco.Configuration
{
    public class SmtpSettings : ISmtpSettings
    {
        public string From { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string PickupDirectoryLocation { get; set; }
    }
}
