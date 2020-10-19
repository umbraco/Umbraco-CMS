using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using Umbraco.Core.IO;

namespace Umbraco.Core.Configuration
{
    public class XmlConfigManipulator : IConfigManipulator
    {
        private readonly IIOHelper _ioHelper;
        private readonly ILogger<XmlConfigManipulator> _logger;

        public XmlConfigManipulator(IIOHelper ioHelper, ILogger<XmlConfigManipulator> logger)
        {
            _ioHelper = ioHelper;
            _logger = logger;
        }

        public void RemoveConnectionString()
        {
            var key = Constants.System.UmbracoConnectionName;
            var fileName = _ioHelper.MapPath("~/web.config");
            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);

            var appSettings = xml.Root.DescendantsAndSelf("appSettings").Single();
            var setting = appSettings.Descendants("add").FirstOrDefault(s => s.Attribute("key").Value == key);

            if (setting != null)
            {
                setting.Remove();
                xml.Save(fileName, SaveOptions.DisableFormatting);
                ConfigurationManager.RefreshSection("appSettings");
            }

            var settings = ConfigurationManager.ConnectionStrings[key];
        }

        /// <summary>
        ///     Saves the connection string as a proper .net connection string in web.config.
        /// </summary>
        /// <remarks>Saves the ConnectionString in the very nasty 'medium trust'-supportive way.</remarks>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="providerName">The provider name.</param>
        public void SaveConnectionString(string connectionString, string providerName)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.",
                    nameof(connectionString));
            if (providerName == null) throw new ArgumentNullException(nameof(providerName));
            if (string.IsNullOrWhiteSpace(providerName))
                throw new ArgumentException("Value can't be empty or consist only of white-space characters.",
                    nameof(providerName));


            var fileSource = "web.config";
            var fileName = _ioHelper.MapPath("~/" + fileSource);

            var xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
            if (xml.Root == null) throw new Exception($"Invalid {fileSource} file (no root).");

            var connectionStrings = xml.Root.DescendantsAndSelf("connectionStrings").FirstOrDefault();
            if (connectionStrings == null) throw new Exception($"Invalid {fileSource} file (no connection strings).");

            // handle configSource
            var configSourceAttribute = connectionStrings.Attribute("configSource");
            if (configSourceAttribute != null)
            {
                fileSource = configSourceAttribute.Value;
                fileName = _ioHelper.MapPath("~/" + fileSource);

                if (!File.Exists(fileName))
                    throw new Exception($"Invalid configSource \"{fileSource}\" (no such file).");

                xml = XDocument.Load(fileName, LoadOptions.PreserveWhitespace);
                if (xml.Root == null) throw new Exception($"Invalid {fileSource} file (no root).");

                connectionStrings = xml.Root.DescendantsAndSelf("connectionStrings").FirstOrDefault();
                if (connectionStrings == null)
                    throw new Exception($"Invalid {fileSource} file (no connection strings).");
            }

            // create or update connection string
            var setting = connectionStrings.Descendants("add").FirstOrDefault(s =>
                s.Attribute("name")?.Value == Constants.System.UmbracoConnectionName);
            if (setting == null)
            {
                connectionStrings.Add(new XElement("add",
                    new XAttribute("name", Constants.System.UmbracoConnectionName),
                    new XAttribute("connectionString", connectionString),
                    new XAttribute("providerName", providerName)));
            }
            else
            {
                AddOrUpdateAttribute(setting, "connectionString", connectionString);
                AddOrUpdateAttribute(setting, "providerName", providerName);
            }

            // save
            _logger.LogInformation("Saving connection string to {ConfigFile}.", fileSource);
            xml.Save(fileName, SaveOptions.DisableFormatting);
            _logger.LogInformation("Saved connection string to {ConfigFile}.", fileSource);
        }

        public void SaveConfigValue(string itemPath, object value)
        {
            throw new NotImplementedException();
        }

        public void SaveDisableRedirectUrlTracking(bool disable)
        {
            var fileName = _ioHelper.MapPath("~/config/umbracoSettings.config");

            var umbracoConfig = new XmlDocument { PreserveWhitespace = true };
            umbracoConfig.Load(fileName);

            var action = disable ? "disable" : "enable";

            if (File.Exists(fileName) == false)
                throw new Exception($"Couldn't {action} URL Tracker, the umbracoSettings.config file does not exist.");

            if (!(umbracoConfig.SelectSingleNode("//web.routing") is XmlElement webRoutingElement))
                throw new Exception($"Couldn't {action} URL Tracker, the web.routing element was not found in umbracoSettings.config.");

            // note: this adds the attribute if it does not exist
            webRoutingElement.SetAttribute("disableRedirectUrlTracking", disable.ToString().ToLowerInvariant());
            umbracoConfig.Save(fileName);
        }

        private static void AddOrUpdateAttribute(XElement element, string name, string value)
        {
            var attribute = element.Attribute(name);
            if (attribute == null)
                element.Add(new XAttribute(name, value));
            else
                attribute.Value = value;
        }
    }
}
