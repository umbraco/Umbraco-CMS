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
using Umbraco.Core.Sync;
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

            switch (_appContext.GetCurrentServerRole())
            {
                case ServerRole.Slave:
                    LogHelper.Debug<ScheduledPublishing>("Does not run on slave servers.");
                    return true; // DO repeat, server role can change
                case ServerRole.Unknown:
                    LogHelper.Debug<ScheduledPublishing>("Does not run on servers with unknown role.");
                    return true; // DO repeat, server role can change
            }

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
                var emailNotificationSettings = healthCheckConfig.NotificationSettings.EmailSettings;
                if (emailNotificationSettings != null && string.IsNullOrEmpty(emailNotificationSettings.RecipientEmail) == false)
                {
                    using (var client = new SmtpClient())
                    using (var mailMessage = new MailMessage())
                    {
                        mailMessage.To.Add(emailNotificationSettings.RecipientEmail);
                        mailMessage.Body = string.Format("<html><body><p>Results of the scheduled Umbraco Health Checks run on {0} at {1} are as follows:</p>{2}</body></html>",
                            DateTime.Now.ToShortDateString(), DateTime.Now.ToShortTimeString(), results.ResultsAsHtml(emailNotificationSettings.Verbosity));
                        mailMessage.Subject = emailNotificationSettings.Subject;
                        mailMessage.IsBodyHtml = true;

                        await client.SendMailAsync(mailMessage);
                    }
                }

                // Send Slack incoming webhook if configured
                var slackNotificationSettings = healthCheckConfig.NotificationSettings.SlackSettings;
                if (slackNotificationSettings != null && string.IsNullOrEmpty(slackNotificationSettings.WebHookUrl) == false)
                {
                    var slackClient = new SlackClient(slackNotificationSettings.WebHookUrl);

                    var icon = Emoji.Warning;
                    if (results.AllChecksSuccessful)
                    {
                        icon = Emoji.WhiteCheckMark;
                    }

                    var slackMessage = new SlackMessage
                    {
                        Channel = slackNotificationSettings.Channel,
                        Text = results.ResultsAsMarkDown(slackNotificationSettings.Verbosity, true),
                        IconEmoji = icon,
                        Username = slackNotificationSettings.UserName
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