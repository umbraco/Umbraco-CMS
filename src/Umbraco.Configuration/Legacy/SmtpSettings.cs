using System.Net.Mail;
using Umbraco.Core.Configuration;

namespace Umbraco.Configuration
{
    public class SmtpSettings : ISmtpSettings
    {
        public string From { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string PickupDirectoryLocation { get; set; }
        public SmtpDeliveryMethod DeliveryMethod { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
