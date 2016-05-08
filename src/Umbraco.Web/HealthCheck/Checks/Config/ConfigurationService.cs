using System;
using System.IO;
using System.Xml;
using Umbraco.Core.Logging;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    //TODO: Add config transform for when config with specified XPath is not found

    public class ConfigurationService
    {
        private readonly string _configFilePath;
        private readonly string _xPath;

        /// <param name="configFilePath">The absolute file location of the configuration file</param>
        /// <param name="xPath">The XPath to select the value</param>
        /// <returns></returns>
        public ConfigurationService(string configFilePath, string xPath)
        {
            _configFilePath = configFilePath;
            _xPath = xPath;
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
                        Result = string.Format("File does not exist: {0}", _configFilePath)
                    };

                var xmlDocument = new XmlDocument();
                xmlDocument.Load(_configFilePath);

                var xmlNode = xmlDocument.SelectSingleNode(_xPath);
                if (xmlNode == null)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = string.Format("Unable to find <strong>{0}</strong> in config file <strong>{1}</strong>", _xPath, _configFilePath)
                    };

                return new ConfigurationServiceResult
                {
                    Success = true,
                    Result = string.Format(xmlNode.Value ?? xmlNode.InnerText)
                };
            }
            catch (Exception exception)
            {
                LogHelper.Error<ConfigurationService>("Error trying to get configuration value", exception);
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = string.Format("Error trying to get configuration value, check log for full error: {0}", exception.Message)
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
                        Result = string.Format("File does not exist: {0}", _configFilePath)
                    };

                var xmlDocument = new XmlDocument { PreserveWhitespace = true };
                xmlDocument.Load(_configFilePath);

                var node = xmlDocument.SelectSingleNode(_xPath);
                if (node == null)
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = string.Format("Unable to find <strong>{0}</strong> in config file <strong>{1}</strong>", _xPath, _configFilePath)
                    };

                if (node.NodeType == XmlNodeType.Element)
                    node.InnerText = value;
                else
                    node.Value = value;

                xmlDocument.Save(_configFilePath);
                return new ConfigurationServiceResult { Success = true };
            }
            catch (Exception exception)
            {
                LogHelper.Error<ConfigurationService>("Error trying to update configuration", exception);
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = string.Format("Error trying to update configuration, check log for full error: {0}", exception.Message)
                };
            }
        }
    }
}