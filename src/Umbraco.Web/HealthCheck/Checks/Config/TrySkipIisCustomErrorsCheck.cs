using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("046A066C-4FB2-4937-B931-069964E16C66", "Try Skip IIS Custom Errors",
        Description = "Starting with IIS 7.5, this must be set to true for Umbraco 404 pages to show. Otherwise, IIS will takeover and render its built-in error page.",
        Group = "Configuration")]
    public class TrySkipIisCustomErrorsCheck : AbstractConfigCheck
    {
        private readonly Version _serverVersion = HttpRuntime.IISVersion;
        private readonly ILocalizedTextService _textService;

        public TrySkipIisCustomErrorsCheck(HealthCheckContext healthCheckContext) : base(healthCheckContext)
        {
            _textService = healthCheckContext.ApplicationContext.Services.TextService;
        }

        public override string FilePath
        {
            get { return "~/Config/umbracoSettings.config"; }
        }

        public override string XPath
        {
            get { return "/settings/web.routing/@trySkipIisCustomErrors"; }
        }

        public override ValueComparisonType ValueComparisonType
        {
            get { return ValueComparisonType.ShouldEqual; }
        }

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                var recommendedValue = _serverVersion >= new Version("7.5.0")
                    ? bool.TrueString.ToLower()
                    : bool.FalseString.ToLower();
                return new List<AcceptableConfiguration> { new AcceptableConfiguration { IsRecommended =  true, Value = recommendedValue } };
            }
        }

        public override string CheckSuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/trySkipIisCustomErrorsCheckSuccessMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, _serverVersion.ToString() });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return _textService.Localize("healthcheck/trySkipIisCustomErrorsCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, _serverVersion.ToString() });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return _textService.Localize("healthcheck/trySkipIisCustomErrorsCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value, _serverVersion.ToString() });
            }
        }
    }
}