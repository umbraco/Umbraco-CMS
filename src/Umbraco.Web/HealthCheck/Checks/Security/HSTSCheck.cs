using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.Configuration;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "ED0D7E40-971E-4BE8-AB6D-8CC5D0A63108",
        "Cookie hijacking and protocol downgrade attacks Protection (Strict-Transport-Security Header (HSTS))",
        Description = "Checks if your site, when running with HTTPS, contains the Strict-Transport-Security Header (HSTS). If not, it adds with a default of 100 days.",
        Group = "Security")]
    public class HSTSCheck : HealthCheck
    {
        // The check is mostly based on the instructions in the OWASP CheatSheet (https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet) and the blogpost of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
        // If you want do to it perfect, you have to submit it https://hstspreload.appspot.com/, but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
        // It almost completely based on the ClickJackingCheck-code, and only removed the meta-tag-stuff because that's not possible with the HSTS
        private readonly ILocalizedTextService _textService;

        private const string SetHSTSHeaderInConfigAction = "setHSTSHeaderInConfig";

        private const string HSTSHeader = "Strict-Transport-Security";


        // The minimal max-age is 18 weeks
        // includeSubdomains is not specified because it can break all your subdomains if they aren't running https. I'll leave up to the sec-savvy people to modify the header if they want
        // Preload must be specified
        private const string HSTSValue = "max-age=10886400; preload";

        public HSTSCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
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
            return new[] { CheckForHSTSHeader() };
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
                case SetHSTSHeaderInConfigAction:
                    return SetHSTSHeaderInConfig();
                default:
                    throw new InvalidOperationException("HSTS action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckForHSTSHeader()
        {
            var message = string.Empty;
            var success = false;
            var url = HealthCheckContext.HttpContext.Request.Url;

            // Access the site home page and check for the HSTS header
            var address = string.Format("https://{0}:{1}", url.Host.ToLower(), url.Port);
            var request = WebRequest.Create(address);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = DoHeadersContainHSTS(response);

                message = success
                    ? _textService.Localize("healthcheck/hSTSCheckHeaderFound")                 
                    : _textService.Localize("healthcheck/hSTSCheckHeaderNotFound");             
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/hSTSCheckInvalidUrl", new[] { address, ex.Message });
            }

            StatusResultType resultType = success ? StatusResultType.Success : StatusResultType.Error;
            string resultMessage;

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                // Check whether HTTPS is solved, if not => Solve that first
                if (url.Scheme != "https")
                {
                    resultType = StatusResultType.Info;
                    resultMessage = _textService.Localize("healthcheck/httpsCheckConfigurationRectifyNotPossible");     // Use the same message, seems okay
                }
                else
                {
                    actions.Add(new HealthCheckAction(SetHSTSHeaderInConfigAction, Id)
                    {
                        Name = _textService.Localize("healthcheck/hSTSSetHeaderInConfig"),                              
                        Description = _textService.Localize("healthcheck/hSTSSetHeaderInConfigDescription")             
                    });
                }
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        private static bool DoHeadersContainHSTS(WebResponse response)
        {
            return response.Headers.AllKeys.Contains(HSTSHeader);
        }

        private HealthCheckStatus SetHSTSHeaderInConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveHeaderToConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck/hSTSSetHeaderInConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/hSTSSetHeaderInConfigError", new[] { errorMessage }))         
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
                                          x.Attribute("name").Value == HSTSHeader);
                if (removeHeaderElement == null)
                {
                    removeHeaderElement = new XElement("remove");
                    removeHeaderElement.Add(new XAttribute("name", HSTSHeader));
                    customHeadersElement.Add(removeHeaderElement);
                }

                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => x.Attribute("name") != null &&
                                          x.Attribute("name").Value == HSTSHeader);
                if (addHeaderElement == null)
                {
                    addHeaderElement = new XElement("add");
                    addHeaderElement.Add(new XAttribute("name", HSTSHeader));
                    addHeaderElement.Add(new XAttribute("value", HSTSValue));
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
