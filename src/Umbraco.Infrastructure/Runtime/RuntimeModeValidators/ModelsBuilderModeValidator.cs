using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates that ModelsBuilderMode is set to <see cref="Constants.ModelsBuilder.ModelsModes.Nothing" /> when in production runtime mode.
/// </summary>
/// <seealso cref="IRuntimeModeValidator" />
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
        var modelsMode = _modelsBuilderSettings.CurrentValue.ModelsMode;

        if (runtimeMode == RuntimeMode.Production && modelsMode != Constants.ModelsBuilder.ModelsModes.Nothing)
        {
            validationErrorMessage = "ModelsBuilder mode needs to be set to Nothing in production mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
