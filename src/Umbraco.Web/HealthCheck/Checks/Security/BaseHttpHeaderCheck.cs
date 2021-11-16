using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Security
{
    public abstract class BaseHttpHeaderCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly IRuntimeState _runtime;

        private const string SetHeaderInConfigAction = "setHeaderInConfig";

        private readonly string _header;
        private readonly string _value;
        private readonly string _localizedTextPrefix;
        private readonly bool _metaTagOptionAvailable;


        protected BaseHttpHeaderCheck(
            IRuntimeState runtime,
            ILocalizedTextService textService,
            string header, string value, string localizedTextPrefix, bool metaTagOptionAvailable)
        {
            _runtime = runtime;
            _textService = textService ?? throw new ArgumentNullException(nameof(textService));
            _header = header;
            _value = value;
            _localizedTextPrefix = localizedTextPrefix;
            _metaTagOptionAvailable = metaTagOptionAvailable;
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
            var url = _runtime.ApplicationUrl.GetLeftPart(UriPartial.Authority);

            var request = WebRequest.Create(url);
            request.Method = "GET";
            try
            {
                var response = request.GetResponse();

                // Check first for header
                success = HasMatchingHeader(response.Headers.AllKeys);

                // If not found, and available, check for meta-tag
                if (success == false && _metaTagOptionAvailable)
                {
                    success = DoMetaTagsContainKeyForHeader(response);

                }

                message = success
                    ? _textService.Localize($"healthcheck", $"{_localizedTextPrefix}CheckHeaderFound")
                    : _textService.Localize($"healthcheck", $"{_localizedTextPrefix}CheckHeaderNotFound");
            }
            catch (Exception ex)
            {
                message = _textService.Localize("healthcheck", "healthCheckInvalidUrl", new[] { url.ToString(), ex.Message });
            }

            var actions = new List<HealthCheckAction>();
            if (success == false)
            {
                actions.Add(new HealthCheckAction(SetHeaderInConfigAction, Id)
                {
                    Name = _textService.Localize("healthcheck", "setHeaderInConfig"),
                    Description = _textService.Localize($"healthcheck", $"{_localizedTextPrefix}SetHeaderInConfigDescription")
                });
            }

            return
                new HealthCheckStatus(message)
                {
                    ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                    Actions = actions
                };
        }

        private bool HasMatchingHeader(IEnumerable<string> headerKeys)
        {
            return headerKeys.Contains(_header, StringComparer.InvariantCultureIgnoreCase);
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
                    return HasMatchingHeader(metaTags.Keys);
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
                    new HealthCheckStatus(_textService.Localize("healthcheck", _localizedTextPrefix + "SetHeaderInConfigSuccess"))
                    {
                        ResultType = StatusResultType.Success
                    };
            }

            return
                new HealthCheckStatus(_textService.Localize("healthcheck", "setHeaderInConfigError", new [] { errorMessage }))
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
                    .SingleOrDefault(x => x.Attribute("name")?.Value.Equals(_value, StringComparison.InvariantCultureIgnoreCase) == true);
                if (removeHeaderElement == null)
                {
                    customHeadersElement.Add(
                        new XElement("remove",
                            new XAttribute("name", _header)));
                }

                var addHeaderElement = customHeadersElement.Elements("add")
                    .SingleOrDefault(x => x.Attribute("name")?.Value.Equals(_header, StringComparison.InvariantCultureIgnoreCase) == true);
                if (addHeaderElement == null)
                {
                    customHeadersElement.Add(
                        new XElement("add",
                            new XAttribute("name", _header),
                            new XAttribute("value", _value)));
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
