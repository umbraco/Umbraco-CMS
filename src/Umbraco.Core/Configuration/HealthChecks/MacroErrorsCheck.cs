using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Logging;
using Umbraco.Core.Macros;
using Umbraco.Core.Services;

namespace Umbraco.Core.Configuration.HealthChecks
{
    [HealthCheck("D0F7599E-9B2A-4D9E-9883-81C7EDC5616F", "Macro errors",
        Description = "Checks to make sure macro errors are not set to throw a YSOD (yellow screen of death), which would prevent certain or all pages from loading completely.",
        Group = "Configuration")]
    public class MacroErrorsCheck : AbstractSettingsCheck
    {
        private readonly ContentSettings _contentSettings;

        public MacroErrorsCheck(ILocalizedTextService textService, ILogger logger, IConfigurationService configurationService, IOptions<ContentSettings> contentSettings)
        : base(textService, logger, configurationService)
        {
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
        }

        public override string ItemPath => Constants.Configuration.ConfigContentMacroErrors;

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

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

        public override string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/macroErrorModeCheckSuccessMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return TextService.Localize("healthcheck/macroErrorModeCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/macroErrorModeCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value });
            }
        }
    }
}
