using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.IO;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "EB66BB3B-1BCD-4314-9531-9DA2C1D6D9A7",
        "HTTPS Configuration",
        Description = "Checks if your site is configured to work over HTTPS and if the Umbraco related configuration for that is correct.",
        Group = "Security")]
    public class HttpsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtime;
        private readonly IGlobalSettings _globalSettings;
        private readonly IContentSection _contentSection;

        private const string FixHttpsSettingAction = "fixHttpsSetting";

        public HttpsCheck(ILocalizedTextService textService, IRuntimeState runtime, IGlobalSettings globalSettings, IContentSection contentSection)
        {
            _textService = textService;
            _runtime = runtime;
            _globalSettings = globalSettings;
            _contentSection = contentSection;
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
                case FixHttpsSettingAction:
                    return FixHttpsSetting();
                default:
                    throw new InvalidOperationException("HttpsCheck action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckForValidCertificate()
        {
            var message = string.Empty;
            StatusResultType result;

            // Attempt to access the site over HTTPS to see if it HTTPS is supported
            // and a valid certificate has been configured
            var url = _runtime.ApplicationUrl.ToString().Replace("http:", "https:");

            var request = (HttpWebRequest) WebRequest.Create(url);
            request.AllowAutoRedirect = false;

            try
            {

                var response = (HttpWebResponse)request.GetResponse();

                // Check for 301/302 as a external login provider such as UmbracoID might be in use
                if (response.StatusCode == HttpStatusCode.Moved || response.StatusCode == HttpStatusCode.Redirect)
                {
                    // Reset request to use the static login background image
                    var absoluteLoginBackgroundImage = $"{url}/{_contentSection.LoginBackgroundImage}";
                    
                    request = (HttpWebRequest)WebRequest.Create(absoluteLoginBackgroundImage);
                    response = (HttpWebResponse)request.GetResponse();
                }

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    // Got a valid response, check now for if certificate expiring within 14 days
                    // Hat-tip: https://stackoverflow.com/a/15343898/489433
                    const int NumberOfDaysForExpiryWarning = 14;
                    var cert = request.ServicePoint.Certificate;
                    using (var cert2 = new X509Certificate2(cert))
                    {
                        var expirationDate = cert2.NotAfter;

                        var daysToExpiry = (int)Math.Floor((cert2.NotAfter - DateTime.Now).TotalDays);
                        if (daysToExpiry <= 0)
                        {
                            result = StatusResultType.Error;
                            message = _textService.Localize("healthcheck", "httpsCheckExpiredCertificate");
                        }
                        else if (daysToExpiry < NumberOfDaysForExpiryWarning)
                        {
                            result = StatusResultType.Warning;
                            message = _textService.Localize("healthcheck", "httpsCheckExpiringCertificate", new[] { daysToExpiry.ToString() });
                        }
                        else
                        {
                            result = StatusResultType.Success;
                            message = _textService.Localize("healthcheck", "httpsCheckValidCertificate");
                        }
                    }
                }
                else
                {
                    result = StatusResultType.Error;
                    message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url, response.StatusDescription });
                }
            }
            catch (Exception ex)
            {
                var exception = ex as WebException;
                if (exception != null)
                {
                    message = exception.Status == WebExceptionStatus.TrustFailure
                        ? _textService.Localize("healthcheck", "httpsCheckInvalidCertificate", new [] { exception.Message })
                        : _textService.Localize("healthcheck", "healthCheckInvalidUrl", new [] { url, exception.Message });
                }
                else
                {
                    message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url, ex.Message });
                }

                result = StatusResultType.Error;
            }

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(message)
                {
                    ResultType = result,
                    Actions = actions
                };
        }

        private HealthCheckStatus CheckIfCurrentSchemeIsHttps()
        {
            var uri = _runtime.ApplicationUrl;
            var success = uri.Scheme == "https";

            var actions = new List<HealthCheckAction>();

            return
                new HealthCheckStatus(_textService.Localize("healthcheck", "httpsCheckIsCurrentSchemeHttps", new[] { success ? string.Empty : "not" }))
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private HealthCheckStatus CheckHttpsConfigurationSetting()
        {
            var httpsSettingEnabled = _globalSettings.UseHttps;
            var uri = _runtime.ApplicationUrl;
            var actions = new List<HealthCheckAction>();

            string resultMessage;
            StatusResultType resultType;
            if (uri.Scheme != "https")
            {
                resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationRectifyNotPossible");
                resultType = StatusResultType.Info;
            }
            else
            {
                if (httpsSettingEnabled == false)
                    actions.Add(new HealthCheckAction(FixHttpsSettingAction, Id)
                    {
                        Name = _textService.Localize("healthcheck", "httpsCheckEnableHttpsButton"),
                        Description = _textService.Localize("healthcheck", "httpsCheckEnableHttpsDescription")
                    });

                resultMessage = _textService.Localize("healthcheck", "httpsCheckConfigurationCheckResult",
                    new[] {httpsSettingEnabled.ToString(), httpsSettingEnabled ? string.Empty : "not"});
                resultType = httpsSettingEnabled ? StatusResultType.Success: StatusResultType.Error;
            }

            return
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        private HealthCheckStatus FixHttpsSetting()
        {
            var configFile = IOHelper.MapPath("~/Web.config");
            const string xPath = "/configuration/appSettings/add[@key='Umbraco.Core.UseHttps']/@value";
            var configurationService = new ConfigurationService(configFile, xPath, _textService);
            var updateConfigFile = configurationService.UpdateConfigFile("true");

            if (updateConfigFile.Success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck", "httpsCheckEnableHttpsSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck", "httpsCheckEnableHttpsError", new [] { updateConfigFile.Result }))
                {
                    ResultType = StatusResultType.Error
                };
        }
    }
}
