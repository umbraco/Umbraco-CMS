using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks
{
    [HealthCheck("D0F7599E-9B2A-4D9E-9883-81C7EDC5616F", "Macro errors",
        Description = "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
        Group = "Configuration")]
    public class MacroErrorsCheck : HealthCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly ILogger _logger;
        private readonly IConfigurationService _configurationService;
        private readonly ContentSettings _contentSettings;

        /// <summary>
        /// Gets the current value of the config setting
        /// </summary>
        private string _currentValue = "MacroErrors";

        /// <summary>
        /// Gets the values to compare against.
        /// </summary>
        public IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                var values = new List<AcceptableConfiguration>
                {
                    new AcceptableConfiguration
                    {
                        IsRecommended = true,
                        Value = "inline"
                    },
                    new AcceptableConfiguration
                    {
                        IsRecommended = false,
                        Value = "silent"
                    }
                };

                return values;
            }
        }

        public string CurrentValue { get; set; }

        /// <summary>
        /// Gets the provided value
        /// </summary>
        public string ProvidedValue { get; set; }

        /// <summary>
        /// Gets the comparison type for checking the value.
        /// </summary>
        public ValueComparisonType ValueComparisonType { get; }

        public MacroErrorsCheck(ILocalizedTextService textService, ILogger logger, IConfigurationService configurationService, IOptions<ContentSettings> contentSettings)
        {
            _textService = textService;
            _logger = logger;
            _configurationService = configurationService;
            _contentSettings = contentSettings.Value;
        }

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var status = new List<HealthCheckStatus>();
            var actions = new List<HealthCheckAction>();

            if (_contentSettings.MacroErrors != MacroErrorBehaviour.Throw)
            {
                status.Add(new HealthCheckStatus("Success")
                {
                    ResultType = StatusResultType.Success,
                    Actions = actions
                });
            }
            else
            {
                status.Add(new HealthCheckStatus("Error")
                {
                    ResultType = StatusResultType.Error,
                    Actions = actions
                });
            }
            return status;


            //if (ValidIfConfigMissing)
            //{
            //    return new[]
            //    {
            //        new HealthCheckStatus(successMessage) { ResultType = StatusResultType.Success }
            //    };
            //}

            //string errorMessage;
            //return new[]
            //{
            //    new HealthCheckStatus(errorMessage) { ResultType = StatusResultType.Error }
            //};


            //remove configurationServiceNodeNotFound from dictionary
            //remove configurationServiceError from dictionary

            // update the successMessage with the CurrentValue
            var successMessage = string.Format(CheckSuccessMessage, Values, _currentValue);
            bool valueFound = Values.Any(value => string.Equals(_currentValue, value.Value, StringComparison.InvariantCultureIgnoreCase));

            if (ValueComparisonType == ValueComparisonType.ShouldEqual && valueFound || ValueComparisonType == ValueComparisonType.ShouldNotEqual && valueFound == false)
            {
                return new[]
                {
                    new HealthCheckStatus(successMessage)
                    {
                        ResultType = StatusResultType.Success
                    }
                };
            }

            // Declare the action for rectifying the config value
            var rectifyAction = new HealthCheckAction("rectify", Id)
            {
                Name = _textService.Localize("healthcheck/rectifyButton"),
                ValueRequired = CanRectifyWithValue
            };

            string resultMessage = string.Format(CheckErrorMessage, Values, _currentValue);
            return new[]
            {
                new HealthCheckStatus(resultMessage)
                {
                    ResultType = StatusResultType.Error,
                    Actions = CanRectify || CanRectifyWithValue ? new[] { rectifyAction } : new HealthCheckAction[0]
                }
            };
        }

        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return string.IsNullOrEmpty(action.ProvidedValue)
                ? Rectify()
                : Rectify(action.ProvidedValue);
        }


        /// <summary>
        /// Gets the message for when the check has succeeded.
        /// </summary>
        public virtual string CheckSuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/checkSuccessMessage", new[] { _currentValue, Values.First(v => v.IsRecommended).Value });

                return _textService.Localize("healthcheck/macroErrorModeCheckSuccessMessage",
                    new[] { _currentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        /// <summary>
        /// Gets the message for when the check has failed.
        /// </summary>
        public virtual string CheckErrorMessage
        {
            get
            {
                return ValueComparisonType == ValueComparisonType.ShouldEqual
                    ? _textService.Localize("healthcheck/checkErrorMessageDifferentExpectedValue",
                        new[] { _currentValue, Values.First(v => v.IsRecommended).Value })
                    : _textService.Localize("healthcheck/checkErrorMessageUnexpectedValue",
                        new[] { _currentValue, Values.First(v => v.IsRecommended).Value });

                return _textService.Localize("healthcheck/macroErrorModeCheckErrorMessage",
                    new[] { _currentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        /// <summary>
        /// Gets the rectify success message.
        /// </summary>
        public virtual string RectifySuccessMessage
        {
            get
            {
                AcceptableConfiguration recommendedValue = Values.FirstOrDefault(v => v.IsRecommended);
                string rectifiedValue = recommendedValue != null ? recommendedValue.Value : ProvidedValue;
                return _textService.Localize("healthcheck/rectifySuccessMessage", new[] { _currentValue, rectifiedValue });

                return _textService.Localize("healthcheck/macroErrorModeCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value });
            }
        }

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically.
        /// </summary>
        public virtual bool CanRectify => ValueComparisonType == ValueComparisonType.ShouldEqual;

        /// <summary>
        /// Gets a value indicating whether this check can be rectified automatically if a value is provided.
        /// </summary>
        public virtual bool CanRectifyWithValue => ValueComparisonType == ValueComparisonType.ShouldNotEqual;

   

        /// <summary>
        /// Rectifies this check.
        /// </summary>
        /// <returns></returns>
        public virtual HealthCheckStatus Rectify()
        {
            if (ValueComparisonType == ValueComparisonType.ShouldNotEqual)
            {
                throw new InvalidOperationException(_textService.Localize("healthcheck/cannotRectifyShouldNotEqual"));
            }

            var recommendedValue = Values.First(v => v.IsRecommended).Value;
            return UpdateConfigurationValue(recommendedValue);
        }

        /// <summary>
        /// Rectifies this check with a provided value.
        /// </summary>
        /// <param name="value">Value provided</param>
        /// <returns></returns>
        public virtual HealthCheckStatus Rectify(string value)
        {
            if (ValueComparisonType == ValueComparisonType.ShouldEqual)
            {
                throw new InvalidOperationException(
                    _textService.Localize("healthcheck/cannotRectifyShouldEqualWithValue"));
            }

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new InvalidOperationException(_textService.Localize("healthcheck/valueToRectifyNotProvided"));
            }

            // Need to track provided value in order to correctly put together the rectify message
            ProvidedValue = value;

            return UpdateConfigurationValue(value);
        }

        //TODO: share code amongst health checks
        private HealthCheckStatus UpdateConfigurationValue(string value)
        {
            ConfigurationServiceResult updateConfigFile = _configurationService.UpdateConfigFile(value);

            if (updateConfigFile.Success == false)
            {
                var message = updateConfigFile.Result;
                return new HealthCheckStatus(message) { ResultType = StatusResultType.Error };
            }

            string resultMessage = string.Format(RectifySuccessMessage, Values);
            return new HealthCheckStatus(resultMessage) { ResultType = StatusResultType.Success };
        }
    }
}
