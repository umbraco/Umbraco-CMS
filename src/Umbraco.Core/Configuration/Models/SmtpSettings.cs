using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using Umbraco.Core.Configuration.Models.Validation;

namespace Umbraco.Core.Configuration.Models
{
    public class SmtpSettings : ValidatableEntryBase
    {
        [Required]
        [EmailAddress]
        public string From { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string PickupDirectoryLocation { get; set; }

        // See notes on ContentSettings.MacroErrors
        internal string DeliveryMethod { get; set; } = SmtpDeliveryMethod.Network.ToString();

        public SmtpDeliveryMethod DeliveryMethodValue
        {
            get
            {
                if (Enum.TryParse<SmtpDeliveryMethod>(DeliveryMethod, true, out var value))
                {
                    return value;
                }

                // We need to return somethhing valid here as this property is evalulated during start-up, and if there's an error
                // in the configured value it won't be parsed to the enum.
                // At run-time though this default won't be used, as an invalid value will be picked up by GlobalSettingsValidator.
                return SmtpDeliveryMethod.Network;
            }
        }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}
