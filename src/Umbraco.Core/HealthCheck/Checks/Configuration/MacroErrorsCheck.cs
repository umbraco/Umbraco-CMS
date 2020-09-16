using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.HealthCheck.Checks.Configuration
{
    [HealthCheck("D0F7599E-9B2A-4D9E-9883-81C7EDC5616F", "Macro errors",
        Description =
            "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
        Group = "Configuration")]
    public class MacroErrorsCheck : AbstractSettingsCheck
    {
        private readonly ILocalizedTextService _textService;
        private readonly ILogger _logger;
        private readonly ContentSettings _contentSettings;

        /// <summary>
        /// Gets the current value of the config setting
        /// </summary>
        private string _currentValue = "MacroErrors";

        public override string ItemPath { get; }

        /// <summary>
        /// Gets the values to compare against.
        /// </summary>
        public override IEnumerable<AcceptableConfiguration> Values
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

        public MacroErrorsCheck(ILocalizedTextService textService, ILogger logger,
            IConfigurationService configurationService, IOptions<ContentSettings> contentSettings)
            : base(textService, logger, configurationService)
        {
            _textService = textService;
            _logger = logger;
            _contentSettings = contentSettings != null
                ? contentSettings.Value
                : throw new ArgumentNullException(nameof(contentSettings));
        }

        public override string CurrentValue => _contentSettings.MacroErrors.ToString();


        public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        {
            return string.IsNullOrEmpty(action.ProvidedValue)
                ? Rectify()
                : Rectify(action.ProvidedValue);
        }


        public override ValueComparisonType ValueComparisonType { get; }

        /// <summary>
        /// Gets the message for when the check has succeeded.
        /// </summary>
        public override string CheckSuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/macroErrorModeCheckSuccessMessage",
                    new[] {_currentValue, Values.First(v => v.IsRecommended).Value});
            }
        }

        /// <summary>
        /// Gets the message for when the check has failed.
        /// </summary>
        public override string CheckErrorMessage
        {
            get
            {
                return _textService.Localize("healthcheck/macroErrorModeCheckErrorMessage",
                    new[] {_currentValue, Values.First(v => v.IsRecommended).Value});
            }
        }

        /// <summary>
        /// Gets the rectify success message.
        /// </summary>
        public override string RectifySuccessMessage
        {
            get
            {
                var recommendedValue = Values.FirstOrDefault(v => v.IsRecommended);
                var rectifiedValue = recommendedValue != null ? recommendedValue.Value : ProvidedValue;
                return _textService.Localize("healthcheck/macroErrorModeCheckRectifySuccessMessage",
                    new[] {Values.First(v => v.IsRecommended).Value});
            }
        }
    }
}
