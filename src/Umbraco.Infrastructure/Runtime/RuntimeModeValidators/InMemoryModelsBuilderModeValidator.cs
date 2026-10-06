using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates that a ModelsBuilder mode generating models only at runtime is not in force unless a package
/// supplying a model factory capable of it is available.
/// </summary>
/// <remarks>
/// The package that supplies such a factory removes this validator, so it only fails for a mode that cannot be
/// met. The default is a mode that needs no such factory, so a site that configured nothing never fails here.
/// </remarks>
/// <seealso cref="IRuntimeModeValidator" />
public class InMemoryModelsBuilderModeValidator : IRuntimeModeValidator
{
    /// <remarks>
    /// Not available on <see cref="Constants.ModelsBuilder.ModelsModes"/>, which deliberately names only the
    /// modes that can be satisfied without an optional package.
    /// </remarks>
    private const string InMemoryAutoModelsMode = "InMemoryAuto";

    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryModelsBuilderModeValidator" /> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">The ModelsBuilder settings.</param>
    /// <param name="publishedModelFactory">The factory for creating published models.</param>
    public InMemoryModelsBuilderModeValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
    }

    /// <inheritdoc />
    public bool Validate(RuntimeMode runtimeMode, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        // Read the mode in force rather than the configured one, so that a mode set in code is validated the
        // same as one set in configuration.
        if (_modelsBuilderSettings.CurrentValue.ModelsMode != InMemoryAutoModelsMode
            || _publishedModelFactory.IsLiveFactoryEnabled())
        {
            validationErrorMessage = null;
            return true;
        }

        validationErrorMessage =
            $"ModelsBuilder mode cannot be set to {InMemoryAutoModelsMode} without a model factory that can generate models at runtime. Install the Umbraco.Cms.DevelopmentMode.Backoffice package and set the runtime mode to {RuntimeMode.BackofficeDevelopment}, or configure a different ModelsBuilder mode.";
        return false;
    }
}
