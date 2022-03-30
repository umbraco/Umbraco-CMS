using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    internal class UseHttpsValidator : RuntimeModeProductionValidatorBase
    {
        private readonly IOptions<GlobalSettings> _globalSettings;

        public UseHttpsValidator(IOptions<GlobalSettings> globalSettings)
            => _globalSettings = globalSettings;

        protected override bool Validate(out string validationErrorMessage)
        {
            // Ensure HTTPS is enforced
            if (!_globalSettings.Value.UseHttps)
            {
                validationErrorMessage = "Using HTTPS should be enforced in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
