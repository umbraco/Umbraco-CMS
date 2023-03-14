using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether the ModelsBuilder mode is not set to <see cref="ModelsMode.InMemoryAuto" /> when in development runtime mode and set to <see cref="ModelsMode.Nothing" /> when in production runtime mode.
/// </summary>
/// <seealso cref="Umbraco.Cms.Infrastructure.Runtime.IRuntimeModeValidator" />
public class ModelsBuilderModeValidator : IRuntimeModeValidator
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;

    /// <summary>
    /// Initializes a new instance of the <see cref="ModelsBuilderModeValidator" /> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">The models builder settings.</param>
    public ModelsBuilderModeValidator(IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings)
        => _modelsBuilderSettings = modelsBuilderSettings;

    /// <inheritdoc />
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
