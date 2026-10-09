using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates that a ModelsBuilder mode generating models only at runtime is not in force unless a model factory
/// able to generate them is available.
/// </summary>
/// <remarks>
/// Whichever component supplies that factory satisfies the mode, so only a mode that nothing can meet fails. The
/// default is a mode that needs no such factory, so a site that configured nothing never fails here.
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
    private readonly Lazy<IPublishedModelFactory> _publishedModelFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryModelsBuilderModeValidator" /> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">The ModelsBuilder settings.</param>
    /// <param name="publishedModelFactory">The factory for creating published models.</param>
    /// <remarks>
    /// The factory is resolved lazily because validation runs while the runtime level is being determined, which
    /// is earlier in the boot than a model factory is otherwise built. It is read only for a mode that needs one.
    /// </remarks>
    public InMemoryModelsBuilderModeValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        Lazy<IPublishedModelFactory> publishedModelFactory)
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
            || _publishedModelFactory.Value.IsLiveFactoryEnabled())
        {
            validationErrorMessage = null;
            return true;
        }

        validationErrorMessage =
            $"ModelsBuilder mode cannot be set to {InMemoryAutoModelsMode} without a model factory that can generate models at runtime. Install the Umbraco.Cms.DevelopmentMode.Backoffice package and set the runtime mode to {RuntimeMode.BackofficeDevelopment}, or configure a different ModelsBuilder mode.";
        return false;
    }
}
