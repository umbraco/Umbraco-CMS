using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Slack.Webhooks;
using Umbraco.Core;
using Umbraco.Core.Configuration.HealthChecks;
using Umbraco.Core.Logging;
using Umbraco.Core.Security;
using Umbraco.Web.HealthCheck;

namespace Umbraco.Web.Scheduling
{
    internal class HealthCheckNotifier : RecurringTaskBase
    {
        private readonly ApplicationContext _appContext;
        private readonly IHealthCheckResolver _healthCheckResolver;

        public HealthCheckNotifier(IBackgroundTaskRunner<RecurringTaskBase> runner, int delayMilliseconds, int periodMilliseconds, 
            ApplicationContext appContext)
            : base(runner, delayMilliseconds, periodMilliseconds)
        {
            _appContext = appContext;
            _healthCheckResolver = HealthCheckResolver.Current;
        }

        public override async Task<bool> PerformRunAsync(CancellationToken token)
        {
            if (_appContext == null) return true; // repeat...

            // ensure we do not run if not main domain, but do NOT lock it
            if (_appContext.MainDom.IsMainDom == false)
            {
                LogHelper.Debug<HealthCheckNotifier>("Does not run if not MainDom.");
                return false; // do NOT repeat, going down
            }

            using (DisposableTimer.DebugDuration<KeepAlive>(() => "Health checks executing", () => "Health checks complete"))
            {
                var healthCheckConfig = (HealthChecksSection)ConfigurationManager.GetSection("umbracoConfiguration/HealthChecks");

                // Don't notify for any checks that are disabled, nor for any disabled
                // just for notifications
                var disabledCheckIds = healthCheckConfig.NotificationSettings.DisabledChecks
                        .Select(x => x.Id)
                    .Union(healthCheckConfig.DisabledChecks
                        .Select(x => x.Id))
                    .Distinct()
                    .ToArray();

                var checks = _healthCheckResolver.HealthChecks
                    .Where(x => disabledCheckIds.Contains(x.Id) == false);

                var results = new HealthCheckResults(checks);
                results.LogResults();

                // Send to email address if configured
                if (!string.IsNullOrEmpty(healthCheckConfig.NotificationSettings.RecipientEmail))
                {
                    using (var client = new SmtpClient())
                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.Body = "Results"; // TODO - get from results
                        mailMessage.To.Add(healthCheckConfig.NotificationSettings.RecipientEmail);
                        mailMessage.Subject = "Umbraco Scheduled HeathChecks Results";
                        mailMessage.IsBodyHtml = true;

                        await client.SendMailAsync(mailMessage);
                    }
                }

                // TODO: get web hook and post
                if (!string.IsNullOrEmpty(healthCheckConfig.NotificationSettings.WebhookUrl))
                {
                    var slackClient = new SlackClient(healthCheckConfig.NotificationSettings.WebhookUrl);
                    var slackMessage = new SlackMessage
                    {
                        Channel = "#test",
                        Text = results.ResultsAsMarkDown(true),
                        IconEmoji = Emoji.Ghost,
                        Username = "Umbraco Health Check Notifier"
                    };
                    slackClient.Post(slackMessage);
                }
            }

            return true; // repeat
        }

        public override bool IsAsync
        {
            get { return true; }
        }
    }
}