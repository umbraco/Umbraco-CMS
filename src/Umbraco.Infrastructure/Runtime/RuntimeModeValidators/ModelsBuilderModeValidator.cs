using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

internal class ModelsBuilderModeValidator : RuntimeModeProductionValidatorBase
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;

    public ModelsBuilderModeValidator(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings)
        => _modelsBuilderSettings = modelsBuilderSettings;

    protected override bool Validate([NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Ensure ModelsBuilder mode is set to Nothing
        if (_modelsBuilderSettings.CurrentValue.ModelsMode != ModelsMode.Nothing)
        {
            validationErrorMessage = "ModelsBuilder mode needs to be set to Nothing in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
