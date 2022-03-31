using System.Linq;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

namespace Umbraco.Cms.Web.Common.Runtime.RuntimeModeValidators
{
    internal class RazorCompileValidator : RuntimeModeProductionValidatorBase
    {
        private readonly ApplicationPartManager _applicationPartManager;

        public RazorCompileValidator(ApplicationPartManager applicationPartManager)
            => _applicationPartManager = applicationPartManager;

        protected override bool Validate(out string validationErrorMessage)
        {
            // Compiled Razor views are stored as application parts
            if (!_applicationPartManager.ApplicationParts.Any(x => x is IRazorCompiledItemProvider))
            {
                validationErrorMessage = "RazorCompileOnBuild and/or RazorCompileOnPublish needs to be set to true in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
