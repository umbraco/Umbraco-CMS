using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly ILocalizedTextService _textService;
        private readonly ILogger _logger;
        private readonly IConfigManipulator _configManipulator;

        /// <param name="textService"></param>
        /// <param name="logger"></param>
        /// <param name="configManipulator"></param>
        /// <returns></returns>
        public ConfigurationService(ILocalizedTextService textService, ILogger logger, IConfigManipulator configManipulator)
        {
            if (textService == null) HandleNullParameter(nameof(textService));
            if (configManipulator == null) HandleNullParameter(nameof(configManipulator));
            if (logger == null) HandleNullParameter(nameof(logger));

            _configManipulator = configManipulator;
            _textService = textService;
            _logger = logger;
        }

        private void HandleNullParameter(string parameter)
        {
            _logger.Error<ConfigurationService>("A required ConfigurationService parameter was null", parameter);
            throw new ArgumentNullException(parameter);
        }

        /// <summary>
        /// Updates a value in a given configuration file with the given path
        /// </summary>
        /// <param name="value"></param>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        public ConfigurationServiceResult UpdateConfigFile(string value, string itemPath)
        {
            try
            {
                if (itemPath == null)
                {
                    return new ConfigurationServiceResult
                    {
                        Success = false,
                        Result = _textService.Localize("healthcheck/configurationServiceNodeNotFound", new[] { itemPath, value })
                    };
                }

                _configManipulator.SaveConfigValue(itemPath, value);
                return new ConfigurationServiceResult
                {
                    Success = true
                };
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
