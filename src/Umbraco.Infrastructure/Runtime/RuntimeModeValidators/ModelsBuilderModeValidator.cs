using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators
{
    internal class ModelsBuilderModeValidator : RuntimeModeProductionValidatorBase
    {
        private readonly IOptions<ModelsBuilderSettings> _modelsBuilderSettings;

        public ModelsBuilderModeValidator(IOptions<ModelsBuilderSettings> modelsBuilderSettings)
            => _modelsBuilderSettings = modelsBuilderSettings;

        protected override bool Validate(out string validationErrorMessage)
        {
            // Ensure ModelsBuilder mode is set to Nothing
            if (_modelsBuilderSettings.Value.ModelsMode != ModelsMode.Nothing)
            {
                validationErrorMessage = "ModelsBuilder mode needs to be set to Nothing in production mode.";
                return false;
            }

            validationErrorMessage = null;
            return true;
        }
    }
}
