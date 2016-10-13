using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "ED0D7E40-971E-4BE8-3108-8CC5D0A6A5B0",
        "Cross-site scripting Protection (X-XSS-Protection header)",
        Description = "This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.",
        Group = "Security")]
    public class XSSProtectionCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        private const string SetXSSProtectionHeaderInConfigAction = "setXXSProtectionHeaderInConfig";

        private const string XSSProtectionHeader = "X-XSS-Protection";
        private const string XXSProtectionValue = "1; mode=block"; 

        public XSSProtectionCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
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
            return new[] { CheckForXSSProtectionHeader() };
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
                case SetXSSProtectionHeaderInConfigAction:
                    return SetFrameOptionsHeaderInConfig();
                default:
                    throw new InvalidOperationException("XSS-Protection-action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckForXSSProtectionHeader()
        {
            var message = string.Empty;
            var success = false;
            var url = HealthCheckContext.HttpContext.Request.Url;

            // Access the site home page and check for the click-jack protection header or meta tag
            var useSsl = GlobalSettings.UseSSL || HealthCheckContext.HttpContext.Request.ServerVariables["SERVER_PORT"] == "443";
            var address = string.Format("http{0}://{1}:{2}", useSsl ? "s" : "", url.Host.ToLower(), url.Port);
            var request = WebRequest.Create(address);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = DoHeadersContainFrameOptions(response);

                message = success
                    ? _textService.Localize("healthcheck/xssProtectionCheckHeaderFound")
                    : _textService.Localize("healthcheck/xssProtectionCheckHeaderNotFound");
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetXSSProtectionHeaderInConfigAction, Id)
                {
                    Name = _textService.Localize("healthcheck/xssProtectionSetHeaderInConfig"),
                    Description = _textService.Localize("healthcheck/xssProtectionSetHeaderInConfigDescription")
                });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private static bool DoHeadersContainFrameOptions(WebResponse response)
        {
            return response.Headers.AllKeys.Contains(XSSProtectionHeader);
        }

        private HealthCheckStatus SetFrameOptionsHeaderInConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveHeaderToConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck/xssProtectionSetHeaderInConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/xssProtectionSetHeaderInConfigError", new [] { errorMessage }))
                {
                    ResultType = StatusResultType.Error
                };
        }

        private static bool SaveHeaderToConfigFile(out string errorMessage)
        {
            try
            {
                // There don't look to be any useful classes defined in https://msdn.microsoft.com/en-us/library/system.web.configuration(v=vs.110).aspx
                // for working with the customHeaders section, so working with the XML directly.
                var configFile = IOHelper.MapPath("~/Web.config");
                var doc = XDocument.Load(configFile);
                var systemWebServerElement = doc.XPathSelectElement("/configuration/system.webServer");
                var httpProtocolElement = systemWebServerElement.Element("httpProtocol");
                if (httpProtocolElement == null)
                {
                    httpProtocolElement = new XElement("httpProtocol");
                    systemWebServerElement.Add(httpProtocolElement);
                }

                var customHeadersElement = httpProtocolElement.Element("customHeaders");
                if (customHeadersElement == null)
                {
                    customHeadersElement = new XElement("customHeaders");
                    httpProtocolElement.Add(customHeadersElement);
                }

                var removeHeaderElement = customHeadersElement.Elements("remove")
                    .SingleOrDefault(x => x.Attribute("name") != null &&
                                          x.Attribute("name").Value == XSSProtectionHeader);
                if (removeHeaderElement == null)
                {
                    removeHeaderElement = new XElement("remove");
                    removeHeaderElement.Add(new XAttribute("name", XSSProtectionHeader));
                    customHeadersElement.Add(removeHeaderElement);
                }

                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => x.Attribute("name") != null &&
                                          x.Attribute("name").Value == XSSProtectionHeader);
                if (addHeaderElement == null)
                {
                    addHeaderElement = new XElement("add");
                    addHeaderElement.Add(new XAttribute("name", XSSProtectionHeader));
                    addHeaderElement.Add(new XAttribute("value", XXSProtectionValue));
                    customHeadersElement.Add(addHeaderElement);
                }

                doc.Save(configFile);

                errorMessage = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                return false;
            }
        }
    }
}
