using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.HealthCheck;
using Umbraco.Core.Hosting;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Core.Configuration.HealthChecks
{
    //TODO: this is not currently in the appsettings.JSON
    [HealthCheck("046A066C-4FB2-4937-B931-069964E16C66", "Try Skip IIS Custom Errors",
        Description = "Starting with IIS 7.5, this must be set to true for Umbraco 404 pages to show. Otherwise, IIS will takeover and render its built-in error page.",
        Group = "Configuration")]
    public class TrySkipIisCustomErrorsCheck : AbstractSettingsCheck
    {
        private readonly Version _iisVersion;
        private GlobalSettings _globalSettings;

        public TrySkipIisCustomErrorsCheck
            (ILocalizedTextService textService, IHostingEnvironment hostingEnvironment, ILogger logger, IConfigurationService configurationService, IOptions<GlobalSettings> globalSettings)
            : base(textService, logger, configurationService)
        {
            _iisVersion = hostingEnvironment.IISVersion;
            _globalSettings = globalSettings.Value;
        }

        public override string ItemPath => "/settings/web.routing/@trySkipIisCustomErrors";

        public override IEnumerable<HealthCheckStatus> GetStatus()
        {
            var status = new List<HealthCheckStatus>();
            var actions = new List<HealthCheckAction>();

            if (_globalSettings.UseHttps != true)
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

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                // beware! 7.5 and 7.5.0 are not the same thing!
                var recommendedValue = _iisVersion >= new Version("7.5")
                    ? bool.TrueString.ToLower()
                    : bool.FalseString.ToLower();
                return new List<AcceptableConfiguration> { new AcceptableConfiguration { IsRecommended = true, Value = recommendedValue } };
            }
        }

        public override string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckSuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value, _iisVersion.ToString() });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, _iisVersion.ToString() });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value, _iisVersion.ToString() });
            }
        }
    }
}
