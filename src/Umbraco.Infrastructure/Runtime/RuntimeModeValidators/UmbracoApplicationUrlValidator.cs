using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    internal class UmbracoApplicationUrlValidator : RuntimeModeProductionValidatorBase
    {
        private readonly IOptions<WebRoutingSettings> _webRoutingSettings;

        public UmbracoApplicationUrlValidator(IOptions<WebRoutingSettings> webRoutingSettings)
            => _webRoutingSettings = webRoutingSettings;

        protected override bool Validate(out string validationErrorMessage)
        {
            // Ensure fixed Umbraco application URL is set
            if (string.IsNullOrWhiteSpace(_webRoutingSettings.Value.UmbracoApplicationUrl))
            {
                validationErrorMessage = "Umbraco application URL needs to be set in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
