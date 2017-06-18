using System;
using System.Net.Mail;
using System.Threading.Tasks;
using Umbraco.Core.Configuration.HealthChecks;

namespace Umbraco.Web.HealthCheck.NotificationMethods
{
    [HealthCheckNotificationMethod("email")]
    public class EmailNotificationMethod : NotificationMethodBase, IHealthCheckNotificatationMethod
    {
        public EmailNotificationMethod(bool enabled, bool failureOnly, HealthCheckNotificationVerbosity verbosity,
                string recipientEmail, string subject)
            : base(enabled, failureOnly, verbosity)
        {
            RecipientEmail = recipientEmail;
            Subject = subject;
            Verbosity = verbosity;
        }

        public string RecipientEmail { get; set; }

        public string Subject { get; set; }

        public async Task SendAsync(HealthCheckResults results)
        {
            if (ShouldSend(results) == false)
            {
                return;
            }

            if (string.IsNullOrEmpty(RecipientEmail))
            {
                return;
            }

            using (var client = new SmtpClient())
            using (var mailMessage = new MailMessage())
            {
                mailMessage.To.Add(RecipientEmail);
                mailMessage.Body =
                    string.Format(
                        "<html><body><p>Results of the scheduled Umbraco Health Checks run on {0} at {1} are as follows:</p>{2}</body></html>",
                        DateTime.Now.ToShortDateString(), 
                        DateTime.Now.ToShortTimeString(),
                        results.ResultsAsHtml(Verbosity));
                mailMessage.Subject = string.IsNullOrEmpty(Subject) ? "Umbraco Health Check Status" : Subject;
                mailMessage.IsBodyHtml = true;

                await client.SendMailAsync(mailMessage);
            }
        }
    }
}
