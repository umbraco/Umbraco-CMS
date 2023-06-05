using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Building;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

/// <summary>
///     Notification handlers used by <see cref="ModelsMode.SourceCodeAuto" />.
/// </summary>
/// <remarks>
///     supports <see cref="ModelsMode.SourceCodeAuto" /> mode but not <see cref="ModelsMode.InMemoryAuto" /> mode.
/// </remarks>
public sealed class AutoModelsNotificationHandler : INotificationHandler<UmbracoApplicationStartingNotification>,
    INotificationHandler<UmbracoRequestEndNotification>,
    INotificationHandler<ContentTypeCacheRefresherNotification>,
    INotificationHandler<DataTypeCacheRefresherNotification>
{
    private static int _req;
    private readonly ModelsBuilderSettings _config;
    private readonly ILogger<AutoModelsNotificationHandler> _logger;
    private readonly IMainDom _mainDom;
    private readonly ModelsGenerationError _mbErrors;
    private readonly IModelsGenerator _modelGenerator;

    // TODO: Remove in v13
    private readonly ModelsGenerator? _concreteModelGenerator;

    [Obsolete("This constructor is obsolete and will be removed in v13. Use the constructor with IModelsGenerator instead.")]
    [Browsable(false)]
    public AutoModelsNotificationHandler(
        ILogger<AutoModelsNotificationHandler> logger,
        IOptionsMonitor<ModelsBuilderSettings> config,
        ModelsGenerator modelGenerator,
        ModelsGenerationError mbErrors,
        IMainDom mainDom)
        : this(logger, config, StaticServiceProvider.Instance.GetRequiredService<IModelsGenerator>(), mbErrors, mainDom)
    {
    }

    [Obsolete("This constructor is obsolete and will be removed in v13. Use the constructor with only IModelsGenerator instead.")]
    [Browsable(false)]
    public AutoModelsNotificationHandler(
        ILogger<AutoModelsNotificationHandler> logger,
        IOptionsMonitor<ModelsBuilderSettings> config,
        ModelsGenerator concreteModelGenerator,
        IModelsGenerator modelGenerator,
        ModelsGenerationError mbErrors,
        IMainDom mainDom)
        : this(logger, config, modelGenerator, mbErrors, mainDom)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AutoModelsNotificationHandler" /> class.
    /// </summary>
    public AutoModelsNotificationHandler(
        ILogger<AutoModelsNotificationHandler> logger,
        IOptionsMonitor<ModelsBuilderSettings> config,
        IModelsGenerator modelGenerator,
        ModelsGenerationError mbErrors,
        IMainDom mainDom)
    {
        _logger = logger;

        // We cant use IOptionsSnapshot here, cause this is used in the Core runtime, and that cannot use a scoped service as it has no scope
        _config = config.CurrentValue ?? throw new ArgumentNullException(nameof(config));
        _modelGenerator = modelGenerator;
        _mbErrors = mbErrors;
        _mainDom = mainDom;
    }

    // we do not manage InMemory models here
    internal bool IsEnabled => _config.ModelsMode.IsAutoNotInMemory();

    public void Handle(ContentTypeCacheRefresherNotification notification) => RequestModelsGeneration();

    public void Handle(DataTypeCacheRefresherNotification notification) => RequestModelsGeneration();

    /// <summary>
    ///     Handles the <see cref="UmbracoApplicationStartingNotification" /> notification
    /// </summary>
    public void Handle(UmbracoApplicationStartingNotification notification) => Install();

    public void Handle(UmbracoRequestEndNotification notification)
    {
        if (IsEnabled && _mainDom.IsMainDom)
        {
            GenerateModelsIfRequested();
        }
    }

    private void Install()
    {
        // don't run if not enabled
        if (!IsEnabled)
        {
        }
    }

    // NOTE
    // CacheUpdated triggers within some asynchronous backend task where
    // we have no HttpContext. So we use a static (non request-bound)
    // var to register that models
    // need to be generated. Could be by another request. Anyway. We could
    // have collisions but... you know the risk.
    private void RequestModelsGeneration()
    {
        if (!_mainDom.IsMainDom)
        {
            return;
        }
        if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
        {
            _logger.LogDebug("Requested to generate models.");
        }

        Interlocked.Exchange(ref _req, 1);
    }

    private void GenerateModelsIfRequested()
    {
        if (Interlocked.Exchange(ref _req, 0) == 0)
        {
            return;
        }

        // cannot proceed unless we are MainDom
        if (_mainDom.IsMainDom)
        {
            try
            {
                if (_logger.IsEnabled(Microsoft.Extensions.Logging.LogLevel.Debug))
                {
                    _logger.LogDebug("Generate models...");
                }
                _logger.LogInformation("Generate models now.");
                _modelGenerator.GenerateModels();
                _mbErrors.Clear();
                _logger.LogInformation("Generated.");
            }
            catch (TimeoutException)
            {
                _logger.LogWarning("Timeout, models were NOT generated.");
            }
            catch (Exception e)
            {
                _mbErrors.Report("Failed to build Live models.", e);
                _logger.LogError("Failed to generate models.", e);
            }
        }
        else
        {
            // this will only occur if this appdomain was MainDom and it has
            // been released while trying to regenerate models.
            _logger.LogWarning("Cannot generate models while app is shutting down");
        }
    }
}
