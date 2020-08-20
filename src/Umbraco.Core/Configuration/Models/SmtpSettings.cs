using System.Net.Mail;

namespace Umbraco.Core.Configuration.Models
{
    public class SmtpSettings
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
