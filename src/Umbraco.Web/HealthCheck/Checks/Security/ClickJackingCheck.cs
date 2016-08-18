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
    [HealthCheck(
        "ED0D7E40-971E-4BE8-AB6D-8CC5D0A6A5B0",
        "Click-Jacking Protection",
        Description = "Checks if your site is allowed to be IFRAMed by another site and thus would be susceptible to click-jacking.",
        Group = "Security")]
    public class ClickJackingCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;

        private const string SetFrameOptionsHeaderInConfigActiobn = "setFrameOptionsHeaderInConfig";

        private const string XFrameOptionsHeader = "X-Frame-Options";
        private const string XFrameOptionsValue = "sameorigin"; // Note can't use "deny" as that would prevent Umbraco itself using IFRAMEs

        public ClickJackingCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
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
            return new[] { CheckForFrameOptionsHeader() };
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
                case SetFrameOptionsHeaderInConfigActiobn:
                    return SetFrameOptionsHeaderInConfig();
                default:
                    throw new InvalidOperationException("HttpsCheck action requested is either not executable or does not exist");
            }
        }

        private HealthCheckStatus CheckForFrameOptionsHeader()
        {
            var message = string.Empty;
            var success = false;
            var url = HealthCheckContext.HttpContext.Request.Url;

            // Access the site home page and check for the click-jack protection header or meta tag
            var address = string.Format("http://{0}:{1}", url.Host.ToLower(), url.Port);
            var request = WebRequest.Create(address);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = DoHeadersContainFrameOptions(response);

                // If not found, check for meta-tag
                if (success == false)
                {
                    success = DoMetaTagsContainFrameOptions(response);
                }

                message = success
                    ? _textService.Localize("healthcheck/clickJackingCheckHeaderFound")
                    : _textService.Localize("healthcheck/clickJackingCheckHeaderNotFound");
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck/httpsCheckInvalidUrl", new[] { address, ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetFrameOptionsHeaderInConfigActiobn, Id)
                {
                    Name = _textService.Localize("healthcheck/clickJackingSetHeaderInConfig"),
                    Description = _textService.Localize("healthcheck/clickJackingSetHeaderInConfigDescription")
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
            return response.Headers.AllKeys.Contains(XFrameOptionsHeader);
        }

        private static bool DoMetaTagsContainFrameOptions(WebResponse response)
        {
            using (var stream = response.GetResponseStream())
            {
                if (stream == null) return false;
                using (var reader = new StreamReader(stream))
                {
                    var html = reader.ReadToEnd();
                    var metaTags = ParseMetaTags(html);
                    return metaTags.ContainsKey(XFrameOptionsHeader);
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

        private HealthCheckStatus SetFrameOptionsHeaderInConfig()
        {
            var errorMessage = string.Empty;
            var success = SaveHeaderToConfigFile(out errorMessage);

            if (success)
            {
                return
                    new HealthCheckStatus(_textService.Localize("healthcheck/clickJackingSetHeaderInConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck/clickJackingSetHeaderInConfigError", new [] { errorMessage }))
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
                                          x.Attribute("name").Value == XFrameOptionsHeader);
                if (removeHeaderElement == null)
                {
                    removeHeaderElement = new XElement("remove");
                    removeHeaderElement.Add(new XAttribute("name", XFrameOptionsHeader));
                    customHeadersElement.Add(removeHeaderElement);
                }

                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => x.Attribute("name") != null &&
                                          x.Attribute("name").Value == XFrameOptionsHeader);
                if (addHeaderElement == null)
                {
                    addHeaderElement = new XElement("add");
                    addHeaderElement.Add(new XAttribute("name", XFrameOptionsHeader));
                    addHeaderElement.Add(new XAttribute("value", XFrameOptionsValue));
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