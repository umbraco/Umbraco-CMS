using System;
using System.Collections.Generic;
using System.Net;
using System.Web;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    /// <summary>
    /// Checks Umbraco backoffice users against the HaveIBeenPwned database to check if they've been breached
    /// </summary>
    [HealthCheck(
        "EB66BB3B-1BCD-4314-9531-9DA2C1D6D9A7",
        "HTTPS Configuration",
        Description = "Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.",
        Group = "Security")]
    public class HttpsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        public HttpsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckIfCurrentSchemeIsHttps(), CheckHttpsConfigurationSetting(), CheckForValidCertificate() };
        }

        /// <summary>
        /// Executes the action and returns it's status
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case "checkForValidCertificate":
                    return CheckForValidCertificate();
                case "checkHttpsConfigurationSetting":
                    return CheckHttpsConfigurationSetting();
                case "checkIfCurrentSchemeIsHttps":
                    return CheckIfCurrentSchemeIsHttps();
                case "fixHttpsSetting":
                    return FixHttpsSetting();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private HealthCheckStatus CheckForValidCertificate()
        {
            var message = string.Empty;
            var success = false;
            var url = HttpContext.Current.Request.Url;

            // Attempt to access the site over HTTPS to see if it HTTPS is supported 
            // and a valid certificate has been configured
            using (var webClient = new WebClient())
            {
                var address = string.Format("https://{0}:{1}", url.Host.ToLower(), url.Port);
                try
                {
                    webClient.DownloadString(address);
                    // if we get here, all is well!
                    success = true;
                }
                catch (Exception ex)
                {
                    var exception = ex as WebException;
                    if (exception != null)
                    {
                        message = exception.Status == WebExceptionStatus.TrustFailure
                            ? _textService.Localize("healthcheck/httpsCheckInvalidCertificate", new [] { exception.Message })
                            : _textService.Localize("healthcheck/httpsCheckInvalidUrl", new [] { address, exception.Message });
                    }
                    else
                    {
                        message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
                    }
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

        private HealthCheckStatus CheckIfCurrentSchemeIsHttps()
        {
            var uri = HttpContext.Current.Request.Url;
            var success = uri.Scheme == "https";

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/httpsCheckIsCurrentSchemeHttps", new[] { success ? string.Empty : "not" }))
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus CheckHttpsConfigurationSetting()
        {
            var httpsSettingEnabled = Core.Configuration.GlobalSettings.UseSSL;

            var actions = new List<HealthCheckAction>();
            if (httpsSettingEnabled == false)
                actions.Add(new HealthCheckAction("fixHttpsSetting", Id) {
                    Name = _textService.Localize("healthcheck/httpsCheckEnableHttpsButton"),
                    Description = _textService.Localize("healthcheck/httpsCheckEnableHttpsDescription")
                });

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/httpsCheckConfigurationCheckResult", new [] {
                        httpsSettingEnabled.ToString(), httpsSettingEnabled ? string.Empty : "not" }))
                {
                    ResultType = httpsSettingEnabled ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus FixHttpsSetting()
        {
            var configFile = HttpContext.Current.Server.MapPath("~/Web.config");
            const string xPath = "/configuration/appSettings/add[@key='umbracoUseSSL']/@value";
            var configurationService = new ConfigurationService(configFile, xPath);
            var updateConfigFile = configurationService.UpdateConfigFile("true");

            if (updateConfigFile.Success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck/httpsCheckEnableHttpsSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/httpsCheckEnableHttpsError", new [] { updateConfigFile.Result }))
                {
                    ResultType = StatusResultType.Error
                };
        }
    }
}