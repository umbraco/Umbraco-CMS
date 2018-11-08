using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using umbraco.IO;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    [HealthCheck(
        "F4D2B02E-28C5-4999-8463-05759FA15C3A",
        "Cross-site scripting Protection (X-XSS-Protection header)",
        Description = "This header enables the Cross-site scripting (XSS) filter in your browser. It checks for the presence of the X-XSS-Protection-header.",
        Group = "Security")]
    public class XssProtectionCheck : BaseHttpHeaderCheck
    {
        private const string UpdateHeaderInConfigAction = "updateHeaderInConfig";

        // The check is mostly based on the instructions in the OWASP CheatSheet
        // (https://www.owasp.org/index.php/HTTP_Strict_Transport_Security_Cheat_Sheet)
        // and the blogpost of Troy Hunt (https://www.troyhunt.com/understanding-http-strict-transport/)
        // If you want do to it perfectly, you have to submit it https://hstspreload.appspot.com/,
        // but then you should include subdomains and I wouldn't suggest to do that for Umbraco-sites.
        public XssProtectionCheck(HealthCheckContext healthCheckContext)
            : base(healthCheckContext, "X-XSS-Protection", "1; mode=block", "xssProtection", true)
        {
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckForXSSHeader() };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            switch (action.Alias)
            {
                case UpdateHeaderInConfigAction:
                    return UpdateHeaderInConfig();
                default:
                    return base.ExecuteAction(action);
            }
        }

        protected HealthCheckStatus CheckForXSSHeader()
        {
            var message = string.Empty;
            var resultType = StatusResultType.Error;

            // Access the site home page and check for the click-jack protection header or meta tag
            var url = HealthCheckContext.SiteUrl;
            var request = WebRequest.Create(url);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                var key = response.Headers.AllKeys.FirstOrDefault(k => Header.InvariantEquals(k));

                if (string.IsNullOrWhiteSpace(key) == false)
                {
                    var headerValue = response.Headers[key];

                    if (key.Equals(headerValue, StringComparison.InvariantCulture))
                    {
                        resultType = StatusResultType.Success;
                        message = TextService.Localize($"healthcheck/{LocalizedTextPrefix}CheckHeaderFound");
                    }
                    else
                    {
                        resultType = StatusResultType.Warning;
                        message = TextService.Localize($"healthcheck/{LocalizedTextPrefix}CheckHeaderFoundWrongCase");
                    }
                }
                else
                {
                    message = TextService.Localize($"healthcheck/{LocalizedTextPrefix}CheckHeaderNotFound");
                }
            }
            catch (Exception ex)
            {
                message = TextService.Localize("healthcheck/healthCheckInvalidUrl", new[] { url, ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            switch (resultType)
            {
                case StatusResultType.Warning:
                    actions.Add(new HealthCheckAction(UpdateHeaderInConfigAction, Id)
                    {
                        Name = TextService.Localize("healthcheck/updateHeaderInConfig"),
                        Description = TextService.Localize($"healthcheck/{LocalizedTextPrefix}SetHeaderInConfigDescription")
                    });
                    break;

                case StatusResultType.Error:
                    actions.Add(new HealthCheckAction(SetHeaderInConfigAction, Id)
                    {
                        Name = TextService.Localize("healthcheck/setHeaderInConfig"),
                        Description = TextService.Localize($"healthcheck/{LocalizedTextPrefix}SetHeaderInConfigDescription")
                    });
                    break;

                default:
                    break;
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = resultType,
                    Actions = actions
                };
        }

        private HealthCheckStatus UpdateHeaderInConfig()
        {
            var errorMessage = string.Empty;
            var success = UpdateHeaderToConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(TextService.Localize(string.Format("healthcheck/{0}SetHeaderInConfigSuccess", LocalizedTextPrefix)))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(TextService.Localize("healthcheck/setHeaderInConfigError", new[] { errorMessage }))
                {
                    ResultType = StatusResultType.Error
                };
        }

        private bool UpdateHeaderToConfigFile(out string errorMessage)
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
                    .SingleOrDefault(x => Header.InvariantEquals(x.Attribute("name")?.Value));
                if (removeHeaderElement == null)
                {
                    removeHeaderElement = new XElement("remove");
                    removeHeaderElement.Add(new XAttribute("name", Header));
                    customHeadersElement.Add(removeHeaderElement);
                }
                else
                {
                    removeHeaderElement.Attribute("name").SetValue(Header);
                }

                var addHeaderElement1 = customHeadersElement.Elements("add");
                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => Header.InvariantEquals(x.Attribute("name")?.Value));
                if (addHeaderElement == null)
                {
                    addHeaderElement = new XElement("add");
                    addHeaderElement.Add(new XAttribute("name", Header));
                    addHeaderElement.Add(new XAttribute("value", Value));
                    customHeadersElement.Add(addHeaderElement);
                }
                else
                {
                    addHeaderElement.Attribute("name").SetValue(Header);
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
