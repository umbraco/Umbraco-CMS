using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Web;
using System.Web.Configuration;

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
        public HttpsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
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
                            ? string.Format("Certificate validation error: '{0}'", exception.Message)
                            : string.Format("Error pinging the URL {0} - '{1}'", address, exception.Message);
                    }
                    else
                    {
                        message = string.Format("Error pinging the URL {0} - '{1}'", address, ex.Message);
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
                new HealthCheckStatus(string.Format("You are currently {0} viewing the site using the HTTPS scheme.", success ? string.Empty : "not"))
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus CheckHttpsConfigurationSetting()
        {
            var httpsSettingEnabled = false;
            var httpsAppSetting = ConfigurationManager.AppSettings["umbracoUseSSL"];
            if (httpsAppSetting != null)
                bool.TryParse(httpsAppSetting, out httpsSettingEnabled);

            var actions = new List<HealthCheckAction>();
            if (httpsSettingEnabled == false)
                actions.Add(new HealthCheckAction("fixHttpsSetting", Id) { Name = "Enable HTTPS", Description = "Sets umbracoSSL setting to true in the appSettings of the web.config file." });

            return
                new HealthCheckStatus(
                    string.Format(
                        "The appSetting 'umbracoUseSSL' is set to '{1}' in your web.config file, your cookies are {0} marked as secure.",
                        httpsSettingEnabled ? string.Empty : "not", httpsAppSetting))
                {
                    ResultType = httpsSettingEnabled ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus FixHttpsSetting()
        {
            var httpsAppSetting = ConfigurationManager.AppSettings["umbracoUseSSL"];
            var webConfig = WebConfigurationManager.OpenWebConfiguration(HttpContext.Current.Request.ApplicationPath);

            if (httpsAppSetting != null)
                webConfig.AppSettings.Settings.Remove("umbracoUseSSL");
            webConfig.AppSettings.Settings.Add("umbracoUseSSL", "true");
            webConfig.Save(ConfigurationSaveMode.Minimal);

            return new HealthCheckStatus("The appSetting 'umbracoUseSSL' is now set to 'true' in your web.config file, your cookies will be marked as secure.")
            {
                ResultType = StatusResultType.Success
            };
        }
    }
}