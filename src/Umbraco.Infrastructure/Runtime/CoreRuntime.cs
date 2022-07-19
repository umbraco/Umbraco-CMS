using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Runtime;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.DependencyInjection;
using Umbraco.Extensions;
using ComponentCollection = Umbraco.Cms.Core.Composing.ComponentCollection;
using IHostingEnvironment = Umbraco.Cms.Core.Hosting.IHostingEnvironment;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <inheritdoc />
public class CoreRuntime : IRuntime
{
    private readonly IApplicationShutdownRegistry _applicationShutdownRegistry;
    private readonly ComponentCollection _components;
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IEventAggregator _eventAggregator;
    private readonly IHostApplicationLifetime? _hostApplicationLifetime;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly ILogger<CoreRuntime> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMainDom _mainDom;
    private readonly IProfilingLogger _profilingLogger;
    private readonly IServiceProvider? _serviceProvider;
    private readonly IUmbracoVersion _umbracoVersion;
    private CancellationToken _cancellationToken;

    /// <summary>
    ///     Initializes a new instance of the <see cref="CoreRuntime" /> class.
    /// </summary>
    public CoreRuntime(
        IRuntimeState state,
        ILoggerFactory loggerFactory,
        ComponentCollection components,
        IApplicationShutdownRegistry applicationShutdownRegistry,
        IProfilingLogger profilingLogger,
        IMainDom mainDom,
        IUmbracoDatabaseFactory databaseFactory,
        IEventAggregator eventAggregator,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        IServiceProvider? serviceProvider,
        IHostApplicationLifetime? hostApplicationLifetime)
    {
        State = state;

        _loggerFactory = loggerFactory;
        _components = components;
        _applicationShutdownRegistry = applicationShutdownRegistry;
        _profilingLogger = profilingLogger;
        _mainDom = mainDom;
        _databaseFactory = databaseFactory;
        _eventAggregator = eventAggregator;
        _hostingEnvironment = hostingEnvironment;
        _umbracoVersion = umbracoVersion;
        _serviceProvider = serviceProvider;
        _hostApplicationLifetime = hostApplicationLifetime;
        _logger = _loggerFactory.CreateLogger<CoreRuntime>();
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete]
    public CoreRuntime(
        ILoggerFactory loggerFactory,
        IRuntimeState state,
        ComponentCollection components,
        IApplicationShutdownRegistry applicationShutdownRegistry,
        IProfilingLogger profilingLogger,
        IMainDom mainDom,
        IUmbracoDatabaseFactory databaseFactory,
        IEventAggregator eventAggregator,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion,
        IServiceProvider? serviceProvider)
        : this(
            state,
            loggerFactory,
            components,
            applicationShutdownRegistry,
            profilingLogger,
            mainDom,
            databaseFactory,
            eventAggregator,
            hostingEnvironment,
            umbracoVersion,
            serviceProvider,
            serviceProvider?.GetRequiredService<IHostApplicationLifetime>())
    {
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete]
    public CoreRuntime(
        ILoggerFactory loggerFactory,
        IRuntimeState state,
        ComponentCollection components,
        IApplicationShutdownRegistry applicationShutdownRegistry,
        IProfilingLogger profilingLogger,
        IMainDom mainDom,
        IUmbracoDatabaseFactory databaseFactory,
        IEventAggregator eventAggregator,
        IHostingEnvironment hostingEnvironment,
        IUmbracoVersion umbracoVersion)
        : this(
            loggerFactory,
            state,
            components,
            applicationShutdownRegistry,
            profilingLogger,
            mainDom,
            databaseFactory,
            eventAggregator,
            hostingEnvironment,
            umbracoVersion,
            null)
    {
    }

    /// <summary>
    ///     Gets the state of the Umbraco runtime.
    /// </summary>
    public IRuntimeState State { get; }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken) => await StartAsync(cancellationToken, false);

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken) => await StopAsync(cancellationToken, false);

    /// <inheritdoc />
    public async Task RestartAsync()
    {
        await StopAsync(_cancellationToken, true);
        await _eventAggregator.PublishAsync(new UmbracoApplicationStoppedNotification(true), _cancellationToken);
        await StartAsync(_cancellationToken, true);
        await _eventAggregator.PublishAsync(new UmbracoApplicationStartedNotification(true), _cancellationToken);
    }

    private async Task StartAsync(CancellationToken cancellationToken, bool isRestarting)
    {
        // Store token, so we can re-use this during restart
        _cancellationToken = cancellationToken;

        // Just in-case HostBuilder.ConfigureUmbracoDefaults() isn't used (e.g. upgrade from 9 and ignored advice).
        if (StaticServiceProvider.Instance == null!)
        {
            StaticServiceProvider.Instance = _serviceProvider!;
        }

        if (isRestarting == false)
        {
            AppDomain.CurrentDomain.UnhandledException += (_, args)
                => _logger.LogError(
                    args.ExceptionObject as Exception,
                    $"Unhandled exception in AppDomain{(args.IsTerminating ? " (terminating)" : null)}.");
        }

        // Acquire the main domain - if this fails then anything that should be registered with MainDom will not operate
        AcquireMainDom();

        // TODO (V10): Remove this obsoleted notification publish.
        await _eventAggregator.PublishAsync(new UmbracoApplicationMainDomAcquiredNotification(), cancellationToken);

        // Notify for unattended install
        await _eventAggregator.PublishAsync(new RuntimeUnattendedInstallNotification(), cancellationToken);
        DetermineRuntimeLevel();

        if (!State.UmbracoCanBoot())
        {
            // We cannot continue here, the exception will be rethrown by BootFailedMiddelware
            return;
        }

        IApplicationShutdownRegistry hostingEnvironmentLifetime = _applicationShutdownRegistry;
        if (hostingEnvironmentLifetime == null)
        {
            throw new InvalidOperationException(
                $"An instance of {typeof(IApplicationShutdownRegistry)} could not be resolved from the container, ensure that one if registered in your runtime before calling {nameof(IRuntime)}.{nameof(StartAsync)}");
        }

        // If level is Run and reason is UpgradeMigrations, that means we need to perform an unattended upgrade
        var unattendedUpgradeNotification = new RuntimeUnattendedUpgradeNotification();
        await _eventAggregator.PublishAsync(unattendedUpgradeNotification, cancellationToken);
        switch (unattendedUpgradeNotification.UnattendedUpgradeResult)
        {
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors:
                if (State.BootFailedException == null)
                {
                    throw new InvalidOperationException(
                        $"Unattended upgrade result was {RuntimeUnattendedUpgradeNotification.UpgradeResult.HasErrors} but no {nameof(BootFailedException)} was registered");
                }

                // We cannot continue here, the exception will be rethrown by BootFailedMiddelware
                return;
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.CoreUpgradeComplete:
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.PackageMigrationComplete:
                // Upgrade is done, set reason to Run
                DetermineRuntimeLevel();
                break;
            case RuntimeUnattendedUpgradeNotification.UpgradeResult.NotRequired:
                break;
        }

        // TODO (V10): Remove this obsoleted notification publish
        await _eventAggregator.PublishAsync(
            new UmbracoApplicationComponentsInstallingNotification(State.Level),
            cancellationToken);

        // Initialize the components
        _components.Initialize();

        await _eventAggregator.PublishAsync(
            new UmbracoApplicationStartingNotification(State.Level, isRestarting),
            cancellationToken);

        if (isRestarting == false)
        {
            // Add application started and stopped notifications last (to ensure they're always published after starting)
            _hostApplicationLifetime?.ApplicationStarted.Register(() =>
                _eventAggregator.Publish(new UmbracoApplicationStartedNotification(false)));
            _hostApplicationLifetime?.ApplicationStopped.Register(() =>
                _eventAggregator.Publish(new UmbracoApplicationStoppedNotification(false)));
        }
    }

    private async Task StopAsync(CancellationToken cancellationToken, bool isRestarting)
    {
        _components.Terminate();
        await _eventAggregator.PublishAsync(
            new UmbracoApplicationStoppingNotification(isRestarting),
            cancellationToken);
    }

    private void AcquireMainDom()
    {
        using DisposableTimer? timer = _profilingLogger.DebugDuration<CoreRuntime>("Acquiring MainDom.", "Acquired.");

        try
        {
            _mainDom.Acquire(_applicationShutdownRegistry);
        }
        catch
        {
            timer?.Fail();
            throw;
        }
    }

    private void DetermineRuntimeLevel()
    {
        if (State.BootFailedException is not null)
        {
            // There's already been an exception, so cannot boot and no need to check
            return;
        }

        using DisposableTimer? timer =
            _profilingLogger.DebugDuration<CoreRuntime>("Determining runtime level.", "Determined.");

        try
        {
            State.DetermineRuntimeLevel();

            _logger.LogDebug("Runtime level: {RuntimeLevel} - {RuntimeLevelReason}", State.Level, State.Reason);

            if (State.Level == RuntimeLevel.Upgrade)
            {
                _logger.LogDebug("Configure database factory for upgrades.");
                _databaseFactory.ConfigureForUpgrade();
            }
        }
        catch (Exception ex)
        {
            State.Configure(RuntimeLevel.BootFailed, RuntimeLevelReason.BootFailedOnException);
            timer?.Fail();
            _logger.LogError(ex, "Boot Failed");

            // We do not throw the exception, it will be rethrown by BootFailedMiddleware
        }
    }
}
