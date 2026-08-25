// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Configuration;

/// <summary>
///     Health check for a ModelsBuilder mode that generates models only at runtime while no live model factory is
///     available, in which case no models are generated at all.
/// </summary>
[HealthCheck(
    "B8F0486B-CFE2-4057-8C52-624769EE5256",
    "ModelsBuilder Mode",
    Description = "The ModelsBuilder mode must be one that the available model factory can satisfy, or no models are generated.",
    Group = "Configuration")]
public class ModelsBuilderModeCheck : HealthCheck
{
    /// <remarks>
    ///     Not available on <see cref="Constants.ModelsBuilder.ModelsModes" />, which deliberately names only the
    ///     modes that can be satisfied without an optional package.
    /// </remarks>
    private const string InMemoryAutoModelsMode = "InMemoryAuto";

    private readonly IConfiguration _configuration;
    private readonly ILocalizedTextService _textService;
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelsBuilderModeCheck" /> class.
    /// </summary>
    public ModelsBuilderModeCheck(
        ILocalizedTextService textService,
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory,
        IConfiguration configuration)
    {
        _textService = textService;
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
        _configuration = configuration;
    }

    /// <inheritdoc />
    public override Task<IEnumerable<HealthCheckStatus>> GetStatusAsync()
        => Task.FromResult<IEnumerable<HealthCheckStatus>>([CheckModelsMode()]);

    private HealthCheckStatus CheckModelsMode()
    {
        var modelsMode = _modelsBuilderSettings.CurrentValue.ModelsMode;

        if (modelsMode != InMemoryAutoModelsMode || _publishedModelFactory.IsLiveFactoryEnabled())
        {
            return new HealthCheckStatus(Localize("modelsBuilderModeCheckSuccessMessage", modelsMode))
            {
                ResultType = StatusResultType.Success,
            };
        }

        // A mode that was explicitly asked for and cannot be met is a misconfiguration to act on. The default
        // predates the model factory moving into an optional package, so a site that never chose this mode is
        // told that models are unavailable without being told it configured something wrong.
        return _configuration.IsModelsModeConfigured()
            ? new HealthCheckStatus(Localize("modelsBuilderModeCheckConfiguredErrorMessage", modelsMode))
            {
                ResultType = StatusResultType.Error,
            }
            : new HealthCheckStatus(Localize("modelsBuilderModeCheckDefaultErrorMessage", modelsMode))
            {
                ResultType = StatusResultType.Warning,
            };
    }

    private string Localize(string alias, string modelsMode)
        => _textService.Localize("healthcheck", alias, [modelsMode]);
}
