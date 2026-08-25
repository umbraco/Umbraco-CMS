using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Reports a ModelsBuilder mode that generates models only at runtime while no live model factory is available,
/// in which case no models are generated at all.
/// </summary>
// TODO (V19): Replace this with an IRuntimeModeValidator, so that an explicitly invalid configuration fails the
// boot consistently on every startup path rather than only being reported.
public class RazorRuntimeCompilationValidator : INotificationHandler<UmbracoApplicationStartedNotification>
{
    /// <remarks>
    /// Not available on <see cref="Core.Constants.ModelsBuilder.ModelsModes"/>, which deliberately names only the
    /// modes that can be satisfied without an optional package.
    /// </remarks>
    private const string InMemoryAutoModelsMode = "InMemoryAuto";

    private readonly IConfiguration _configuration;
    private readonly ILogger<RazorRuntimeCompilationValidator> _logger;
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorRuntimeCompilationValidator"/> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">An <see cref="IOptionsMonitor{TOptions}"/> for <see cref="ModelsBuilderSettings"/> used to access the current ModelsBuilder configuration.</param>
    /// <param name="publishedModelFactory">An <see cref="IPublishedModelFactory"/> instance used to create published models.</param>
    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 19.")]
    public RazorRuntimeCompilationValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory)
        : this(
            modelsBuilderSettings,
            publishedModelFactory,
            StaticServiceProvider.Instance.GetRequiredService<IConfiguration>(),
            StaticServiceProvider.Instance.GetRequiredService<ILogger<RazorRuntimeCompilationValidator>>())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorRuntimeCompilationValidator"/> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">An <see cref="IOptionsMonitor{TOptions}"/> for <see cref="ModelsBuilderSettings"/> used to access the current ModelsBuilder configuration.</param>
    /// <param name="publishedModelFactory">An <see cref="IPublishedModelFactory"/> instance used to create published models.</param>
    /// <param name="configuration">The configuration, used to establish whether a mode has been explicitly configured.</param>
    /// <param name="logger">The logger.</param>
    public RazorRuntimeCompilationValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory,
        IConfiguration configuration,
        ILogger<RazorRuntimeCompilationValidator> logger)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Handles the <see cref="UmbracoApplicationStartedNotification"/> by reporting a ModelsBuilder mode that
    /// cannot be satisfied by the available model factory.
    /// </summary>
    /// <param name="notification">The notification instance triggered when the Umbraco application has started.</param>
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_modelsBuilderSettings.CurrentValue.ModelsMode != InMemoryAutoModelsMode
            || _publishedModelFactory.IsLiveFactoryEnabled())
        {
            return;
        }

        // Only a mode that was asked for is a misconfiguration to be acted on. The default predates the model
        // factory moving into an optional package, so a site that never chose this mode is reported without
        // being warned about a future version it will not be affected by.
        if (_configuration.IsModelsModeConfigured())
        {
            _logger.LogError(
                "ModelsBuilder is configured to use the {ModelsMode} models mode, but no live model factory is available, so no models will be generated. Install the Umbraco.Cms.DevelopmentMode.Backoffice package, set the runtime mode to BackofficeDevelopment, or configure a different ModelsBuilder mode. This configuration will prevent startup in Umbraco 19.",
                InMemoryAutoModelsMode);
        }
        else
        {
            _logger.LogError(
                "ModelsBuilder is using the default {ModelsMode} models mode, but no live model factory is available, so no models will be generated. Install the Umbraco.Cms.DevelopmentMode.Backoffice package, set the runtime mode to BackofficeDevelopment, or configure an explicit ModelsBuilder mode.",
                InMemoryAutoModelsMode);
        }
    }
}
