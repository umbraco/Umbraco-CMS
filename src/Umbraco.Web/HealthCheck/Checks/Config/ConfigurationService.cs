using System;
using System.IO;
using System.Xml;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    // TODO: Add config transform for when config with specified XPath is not found

    public class ConfigurationService
    {
        private readonly string _configFilePath;
        private readonly string _xPath;
        private readonly ILocalizedTextService _textService;

        /// <param name="configFilePath">The absolute file location of the configuration file</param>
        /// <param name="xPath">The XPath to select the value</param>
        /// <returns></returns>
        public ConfigurationService(string configFilePath, string xPath, ILocalizedTextService textService)
        {
            _configFilePath = configFilePath;
            _xPath = xPath;
            _textService = textService;
        }

        /// <summary>
        /// Gets a value from a given configuration file with the given XPath
        /// </summary>
        public ConfigurationServiceResult GetConfigurationValue()
        {
            try
            {
                if (File.Exists(_configFilePath) == false)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck", "configurationServiceFileNotFound", new[] { _configFilePath })
                    };

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_configFilePath);

                var xmlNode = xmlDocument.SelectSingleNode(_xPath);
                if (xmlNode == null)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck", "configurationServiceNodeNotFound", new[] { _xPath, _configFilePath })
                    };

                return new ConfigurationServiceResult
                {
                    Success = true,
                    Result = string.Format(xmlNode.Value ?? xmlNode.InnerText)
                };
            }
            catch (Exception ex)
            {
                Current.Logger.Error<ConfigurationService>(ex, "Error trying to get configuration value");
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = _textService.Localize("healthcheck", "configurationServiceError", new[] { ex.Message })
                };
            }
        }

        /// <summary>
        /// Updates a value in a given configuration file with the given XPath
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ConfigurationServiceResult UpdateConfigFile(string value)
        {
            try
            {
                if (File.Exists(_configFilePath) == false)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck", "configurationServiceFileNotFound", new[] { _configFilePath })
                    };

                var xmlDocument = new XmlDocument { PreserveWhitespace = true };
                xmlDocument.Load(_configFilePath);

                var node = xmlDocument.SelectSingleNode(_xPath);
                if (node == null)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck", "configurationServiceNodeNotFound", new[] { _xPath, _configFilePath })
                    };

                if (node.NodeType == XmlNodeType.Element)
                    node.InnerText = value;
                else
                    node.Value = value;

                xmlDocument.Save(_configFilePath);
                return new ConfigurationServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                Current.Logger.Error<ConfigurationService>(ex, "Error trying to update configuration");
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = _textService.Localize("healthcheck", "configurationServiceError", new[] { ex.Message })
                };
            }
        }
    }
}
