using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates that a ModelsBuilder mode generating models only at runtime is not configured unless a package
/// supplying a model factory capable of it is available.
/// </summary>
/// <remarks>
/// The package that supplies such a factory removes this validator, so it only fails for a mode that was asked
/// for and cannot be met. A mode that was never configured resolves to a mode that needs no such factory.
/// </remarks>
/// <seealso cref="IRuntimeModeValidator" />
public class InMemoryModelsBuilderModeValidator : IRuntimeModeValidator
{
    /// <remarks>
    /// Not available on <see cref="Constants.ModelsBuilder.ModelsModes"/>, which deliberately names only the
    /// modes that can be satisfied without an optional package.
    /// </remarks>
    private const string InMemoryAutoModelsMode = "InMemoryAuto";

    private readonly IConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryModelsBuilderModeValidator" /> class.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public InMemoryModelsBuilderModeValidator(IConfiguration configuration)
        => _configuration = configuration;

    /// <inheritdoc />
    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        if (_configuration.IsModelsModeConfigured() && _configuration.GetModelsMode() == InMemoryAutoModelsMode)
        {
            validationErrorMessage =
                $"ModelsBuilder mode cannot be set to {InMemoryAutoModelsMode} without a model factory that can generate models at runtime. Install the Umbraco.Cms.DevelopmentMode.Backoffice package and set the runtime mode to {RuntimeMode.BackofficeDevelopment}, or configure a different ModelsBuilder mode.";
            return false;
        }

        validationErrorMessage = null;
        return true;
    }
}
