using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    internal class RuntimeMinificationValidator : RuntimeModeProductionValidatorBase
    {
        private readonly IOptions<RuntimeMinificationSettings> _runtimeMinificationSettings;

        public RuntimeMinificationValidator(IOptions<RuntimeMinificationSettings> runtimeMinificationSettings)
            => _runtimeMinificationSettings = runtimeMinificationSettings;

        protected override bool Validate(out string validationErrorMessage)
        {
            // Ensure runtime minification is using a fixed cache buster
            if (_runtimeMinificationSettings.Value.CacheBuster == RuntimeMinificationCacheBuster.Timestamp)
            {
                validationErrorMessage = "Runtime minification setting needs to be set to a fixed cache buster (like Version or AppDomain) in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
