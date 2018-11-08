using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    public abstract class BaseHttpHeaderCheck : HealthCheck
    {
        protected readonly ILocalizedTextService TextService;

        protected const string SetHeaderInConfigAction = "setHeaderInConfig";

        protected readonly string Header;
        protected readonly string Value;
        protected readonly string LocalizedTextPrefix;
        protected readonly bool MetaTagOptionAvailable;

        public BaseHttpHeaderCheck(HealthCheckContext healthCheckContext, 
            string header, string value, string localizedTextPrefix, bool metaTagOptionAvailable) : base(healthCheckContext)
        {
            TextService = healthCheckContext.ApplicationContext.Services.TextService;

            Header = header;
            Value = value;
            LocalizedTextPrefix = localizedTextPrefix;
            MetaTagOptionAvailable = metaTagOptionAvailable;
        }

        /// <summary>
        /// Get the status for this health check
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            //return the statuses
            return new[] { CheckForHeader() };
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
                case SetHeaderInConfigAction:
                    return SetHeaderInConfig();
                default:
                    throw new InvalidOperationException("HTTP Header action requested is either not executable or does not exist");
            }
        }

        protected HealthCheckStatus CheckForHeader()
        {
            var message = string.Empty;
            var success = false;

            // Access the site home page and check for the click-jack protection header or meta tag
            var url = HealthCheckContext.SiteUrl;
            var request = WebRequest.Create(url);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = DoHttpHeadersContainHeader(response);

                // If not found, and available, check for meta-tag
                if (success == false && MetaTagOptionAvailable)
                {
                    success = DoMetaTagsContainKeyForHeader(response);
                }

                message = success
                    ? TextService.Localize(string.Format("healthcheck/{0}CheckHeaderFound", LocalizedTextPrefix))
                    : TextService.Localize(string.Format("healthcheck/{0}CheckHeaderNotFound", LocalizedTextPrefix));
            }
            catch (Exception ex)
            {
                message = TextService.Localize("healthcheck/healthCheckInvalidUrl", new[] { url, ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetHeaderInConfigAction, Id)
                {
                    Name = TextService.Localize("healthcheck/setHeaderInConfig"),
                    Description = TextService.Localize(string.Format("healthcheck/{0}SetHeaderInConfigDescription", LocalizedTextPrefix))
                });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private bool DoHttpHeadersContainHeader(WebResponse response)
        {
            return response.Headers.AllKeys.Contains(Header);
        }

        private bool DoMetaTagsContainKeyForHeader(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return false;
                using (var reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    var metaTags = ParseMetaTags(html);
                    return metaTags.ContainsKey(Header);
                }
            }
        }

        private static Dictionary<string, string> ParseMetaTags(string html)
        {
            var regex = new Regex("<meta http-equiv=\"(.+?)\" content=\"(.+?)\"", RegexOptions.IgnoreCase);

            return regex.Matches(html)
                .Cast<Match>()
                .ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value);
        }

        private HealthCheckStatus SetHeaderInConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveHeaderToConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(TextService.Localize(string.Format("healthcheck/{0}SetHeaderInConfigSuccess", LocalizedTextPrefix)))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(TextService.Localize("healthcheck/setHeaderInConfigError", new [] { errorMessage }))
                {
                    ResultType = StatusResultType.Error
                };
        }

        private bool SaveHeaderToConfigFile(out string errorMessage)
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
                                          x.Attribute("name")?.Value == Value);
                if (removeHeaderElement == null)
                {
                    removeHeaderElement = new XElement("remove");
                    removeHeaderElement.Add(new XAttribute("name", Header));
                    customHeadersElement.Add(removeHeaderElement);
                }

                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => x.Attribute("name") != null &&
                                          x.Attribute("name").Value == Header);
                if (addHeaderElement == null)
                {
                    addHeaderElement = new XElement("add");
                    addHeaderElement.Add(new XAttribute("name", Header));
                    addHeaderElement.Add(new XAttribute("value", Value));
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
