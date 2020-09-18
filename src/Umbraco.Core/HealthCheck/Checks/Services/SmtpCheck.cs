using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
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
        private readonly GlobalSettings _globalSettings;

        public SmtpCheck(ILocalizedTextService textService, IOptions<GlobalSettings> globalSettings)
        {
            _textService = textService;
            _globalSettings = globalSettings.Value;
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
            var success = false;

            var smtpSettings = _globalSettings.Smtp;

            string message;
            if (smtpSettings == null)
            {
                message = _textService.Localize("healthcheck/smtpMailSettingsNotFound");
            }
            else
            {
                if (string.IsNullOrEmpty(smtpSettings.Host))
                {
                    message = _textService.Localize("healthcheck/smtpMailSettingsHostNotConfigured");
                }
                else
                {
                    success = CanMakeSmtpConnection(smtpSettings.Host, smtpSettings.Port);
                    message = success
                        ? _textService.Localize("healthcheck/smtpMailSettingsConnectionSuccess")
                        : _textService.Localize("healthcheck/smtpMailSettingsConnectionFail", new [] { smtpSettings.Host, smtpSettings.Port.ToString() });
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
