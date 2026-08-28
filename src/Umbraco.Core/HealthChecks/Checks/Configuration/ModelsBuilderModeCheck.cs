// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
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
    private readonly IConfiguration _configuration;
    private readonly ILocalizedTextService _textService;
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ModelsBuilderModeCheck" /> class.
    /// </summary>
    /// <param name="textService">The localized text service, used to resolve the reported messages.</param>
    /// <param name="modelsBuilderSettings">An <see cref="IOptionsMonitor{TOptions}" /> for <see cref="ModelsBuilderSettings" />, used to access the models mode in force.</param>
    /// <param name="publishedModelFactory">The published model factory, used to establish what it is capable of generating.</param>
    /// <param name="configuration">The configuration, used to establish whether a mode has been explicitly configured.</param>
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

        if (modelsMode != Constants.ModelsBuilder.InMemoryAutoModelsMode || _publishedModelFactory.IsLiveFactoryEnabled())
        {
            return new HealthCheckStatus(Localize("modelsBuilderModeCheckSuccessMessage", modelsMode))
            {
                ResultType = StatusResultType.Success,
            };
        }

        // The runtime mode blocks the factory before any package can supply it, so it is the first thing to
        // report. Once it allows the factory and there still is not one, the package providing it is what is
        // missing — reporting both remedies at once would tell most sites to set a runtime mode they already
        // have, since the one this mode requires is also the default.
        RuntimeMode runtimeMode = _configuration.GetRuntimeMode();

        // A mode that was explicitly asked for and cannot be met is a misconfiguration to act on. The default
        // predates the model factory moving into an optional package, so a site that never chose this mode is
        // told that models are unavailable without being told it configured something wrong.
        var modelsModeConfigured = _configuration.IsModelsModeConfigured();

        if (runtimeMode != RuntimeMode.BackofficeDevelopment)
        {
            return modelsModeConfigured
                ? new HealthCheckStatus(Localize("modelsBuilderModeCheckRuntimeModeConfiguredErrorMessage", modelsMode, runtimeMode.ToString()))
                {
                    ResultType = StatusResultType.Error,
                }
                : new HealthCheckStatus(Localize("modelsBuilderModeCheckRuntimeModeDefaultErrorMessage", modelsMode, runtimeMode.ToString()))
                {
                    ResultType = StatusResultType.Warning,
                };
        }

        return modelsModeConfigured
            ? new HealthCheckStatus(Localize("modelsBuilderModeCheckPackageMissingConfiguredErrorMessage", modelsMode))
            {
                ResultType = StatusResultType.Error,
            }
            : new HealthCheckStatus(Localize("modelsBuilderModeCheckPackageMissingDefaultErrorMessage", modelsMode))
            {
                ResultType = StatusResultType.Warning,
            };
    }

    private string Localize(string alias, params string[] tokens)
        => _textService.Localize("healthcheck", alias, tokens);
}
