using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

internal class ModelsBuilderModeValidator : IRuntimeModeValidator
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;

    public ModelsBuilderModeValidator(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings)
        => _modelsBuilderSettings = modelsBuilderSettings;

    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        ModelsMode modelsMode = _modelsBuilderSettings.CurrentValue.ModelsMode;

        if (runtimeMode == RuntimeMode.Development && modelsMode == ModelsMode.InMemoryAuto)
        {
            validationErrorMessage = "ModelsBuilder mode cannot be set to InMemoryAuto in development mode.";
            return false;
        }

        if (runtimeMode == RuntimeMode.Production && modelsMode != ModelsMode.Nothing)
        {
            validationErrorMessage = "ModelsBuilder mode needs to be set to Nothing in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
