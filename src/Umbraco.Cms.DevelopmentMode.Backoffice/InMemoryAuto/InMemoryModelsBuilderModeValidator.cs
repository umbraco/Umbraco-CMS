using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Runtime;

namespace Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;

/// <summary>
/// Validates that the ModelsBuilder mode is not set to InMemoryAuto when in development runtime mode.
/// </summary>
public class InMemoryModelsBuilderModeValidator : IRuntimeModeValidator
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;

    public InMemoryModelsBuilderModeValidator(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
    }

    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (runtimeMode != RuntimeMode.BackofficeDevelopment &&
            _modelsBuilderSettings.CurrentValue.ModelsMode == ModelsModeConstants.InMemoryAuto)
        {
            validationErrorMessage = "ModelsBuilder mode cannot be set to InMemoryAuto in development mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
