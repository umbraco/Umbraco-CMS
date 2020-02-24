﻿using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Hosting;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;

namespace Umbraco.Web.HealthCheck.Checks.Config
{
    [HealthCheck("046A066C-4FB2-4937-B931-069964E16C66", "Try Skip IIS Custom Errors",
        Description = "Starting with IIS 7.5, this must be set to true for Umbraco 404 pages to show. Otherwise, IIS will takeover and render its built-in error page.",
        Group = "Configuration")]
    public class TrySkipIisCustomErrorsCheck : AbstractConfigCheck
    {
        private readonly IHostingEnvironment _hostingEnvironment;

        public TrySkipIisCustomErrorsCheck(ILocalizedTextService textService, IIOHelper ioHelper, ILogger logger,
            IHostingEnvironment hostingEnvironment)
            : base(textService, ioHelper, logger)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        public override string FilePath => "~/Config/umbracoSettings.config";

        public override string XPath => "/settings/web.routing/@trySkipIisCustomErrors";

        public override ValueComparisonType ValueComparisonType => ValueComparisonType.ShouldEqual;

        public override IEnumerable<AcceptableConfiguration> Values
        {
            get
            {
                // beware! 7.5 and 7.5.0 are not the same thing!
                var recommendedValue = _hostingEnvironment.IISVersion >= new Version("7.5")
                    ? bool.TrueString.ToLower()
                    : bool.FalseString.ToLower();
                return new List<AcceptableConfiguration> { new AcceptableConfiguration { IsRecommended =  true, Value = recommendedValue } };
            }
        }

        public override string CheckSuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckSuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value, _hostingEnvironment.IISVersion.ToString() });
            }
        }

        public override string CheckErrorMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckErrorMessage",
                    new[] { CurrentValue, Values.First(v => v.IsRecommended).Value, _hostingEnvironment.IISVersion.ToString() });
            }
        }

        public override string RectifySuccessMessage
        {
            get
            {
                return TextService.Localize("healthcheck/trySkipIisCustomErrorsCheckRectifySuccessMessage",
                    new[] { Values.First(v => v.IsRecommended).Value, _hostingEnvironment.IISVersion.ToString() });
            }
        }
    }
}
