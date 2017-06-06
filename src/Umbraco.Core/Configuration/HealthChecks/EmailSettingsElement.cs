using System.Configuration;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class EmailSettingsElement : BaseNotificationMethodElement
    {
        private const string RECIPIENT_EMAIL_KEY = "recipientEmail";
        private const string SUBJECT_KEY = "subject";

        [ConfigurationProperty(RECIPIENT_EMAIL_KEY, IsRequired = true)]
        public string RecipientEmail
        {
            get
            {
                return ((string)(base[RECIPIENT_EMAIL_KEY]));
            }
        }

        [ConfigurationProperty(SUBJECT_KEY, IsRequired = true)]
        public string Subject
        {
            get
            {
                return ((string)(base[SUBJECT_KEY]));
            }
        }
    }
}
