using System;
using Microsoft.Extensions.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.HealthCheck.Checks.Config;

namespace Umbraco.Core.Configuration.HealthChecks
{
    public class ConfigurationService
    {
        private readonly string _key;
        private readonly ILocalizedTextService _textService;
        private readonly ILogger _logger;
        private readonly IConfiguration _config;
        private string appsettings => "appsettings.json";

        /// <param name="configuration">The configuration (a</param>
        /// <param name="key">The path to select the value from the JSON</param>
        /// <param name="textService"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public ConfigurationService(IConfiguration configuration, string key, ILocalizedTextService textService, ILogger logger)
        {
            _config = configuration;
            _textService = textService;
            _logger = logger;
            _key = key;
        }

        /// <summary>
        /// Gets a value from a given configuration file with the given XPath
        /// </summary>
        public ConfigurationServiceResult GetConfigurationValue()
        {
            try
            {
                if (_config == null)
                {
                    return missingConfigResult();
                }

                string key = _config[_key];
                if (key == null)
                {
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck/configurationServiceNodeNotFound",
                            new[] { _key, appsettings })
                    };
                }

                return new ConfigurationServiceResult
                {
                    Success = true,
                    Result = string.Format(key.IsNullOrWhiteSpace() ? _key : key)
                };
            }
            catch (Exception ex)
            {
                _logger.Error<ConfigurationService>(ex, "Error trying to get configuration value");
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = _textService.Localize("healthcheck/configurationServiceError", new[] { ex.Message })
                };
            }
        }

        /// <summary>
        /// This should never happen since if the apsettings.json is missing, everything would fail
        /// </summary>
        /// <returns>File not found result</returns>
        private ConfigurationServiceResult missingConfigResult()
        {

            return new ConfigurationServiceResult
            {
                Success = false,
                Result = _textService.Localize("healthcheck/configurationServiceFileNotFound",
                            new[] { appsettings })
            };
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
                if (_config == null)
                {
                    return missingConfigResult();
                }

                //var node = xmlDocument.SelectSingleNode(_xPath);
                //if (node == null)
                //    return new ConfigurationServiceResult
                //    {
                //        Success = false,
                //        Result = _textService.Localize("healthcheck/configurationServiceNodeNotFound", new[] { _xPath, _configFilePath })
                //    };

                //if (node.NodeType == XmlNodeType.Element)
                //    node.InnerText = value;
                //else
                //    node.Value = value;

                //xmlDocument.Save(_configFilePath);
                return new ConfigurationServiceResult { Success = true };
            }
            catch (Exception ex)
            {
                _logger.Error<ConfigurationService>(ex, "Error trying to update configuration");
                return new ConfigurationServiceResult
                {
                    Success = false,
                    Result = _textService.Localize("healthcheck/configurationServiceError", new[] { ex.Message })
                };
            }
        }
    }
}
