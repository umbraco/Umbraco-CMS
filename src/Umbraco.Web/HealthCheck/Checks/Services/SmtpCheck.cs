using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Configuration;
using System.Net.Sockets;
using System.Web.Configuration;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Services
{
    [HealthCheck(
        "1B5D221B-CE99-4193-97CB-5F3261EC73DF",
        "SMTP Settings",
        Description = "Checks that valid settings for sending emails are in place.",
        Group = "Services")]
    public class SmtpCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtime;

        public SmtpCheck(ILocalizedTextService textService, IRuntimeState runtime)
        {
            _textService = textService;
            _runtime = runtime;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckSmtpSettings() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            throw new InvalidOperationException("SmtpCheck has no executable actions");
        }

        private HealthCheckStatus CheckSmtpSettings()
        {
            const int DefaultSmtpPort = 25;
            var message = string.Empty;
            var success = false;

            // appPath is the virtual application root path on the server
            var config = WebConfigurationManager.OpenWebConfiguration(_runtime.ApplicationVirtualPath);
            var settings = (MailSettingsSectionGroup)config.GetSectionGroup("system.net/mailSettings");
            if (settings == null)
            {
                message = _textService.Localize("healthcheck", "smtpMailSettingsNotFound");
            }
            else
            {
                var host = settings.Smtp.Network.Host;
                var port = settings.Smtp.Network.Port == 0 ? DefaultSmtpPort : settings.Smtp.Network.Port;
                if (string.IsNullOrEmpty(host))
                {
                    message = _textService.Localize("healthcheck", "smtpMailSettingsHostNotConfigured");
                }
                else
                {
                    success = CanMakeSmtpConnection(host, port);
                    message = success
                        ? _textService.Localize("healthcheck", "smtpMailSettingsConnectionSuccess")
                        : _textService.Localize("healthcheck", "smtpMailSettingsConnectionFail", new [] { host, port.ToString() });
                }
            }

            var actions = new List<HealthCheckAction>();
            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private bool CanMakeSmtpConnection(string host, int port)
        {
            try
            {
                using (var client = new TcpClient())
                {
                    client.Connect(host, port);
                    using (var stream = client.GetStream())
                    {
                        using (var writer = new StreamWriter(stream))
                        using (var reader = new StreamReader(stream))
                        {
                            writer.WriteLine("EHLO " + host);
                            writer.Flush();
                            reader.ReadLine();
                            return true;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
