using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Packaging;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Core.Semver;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Infrastructure.Runtime;

/// <summary>
/// Represents the state of the Umbraco runtime.
/// </summary>
public class RuntimeState : IRuntimeState
{
    internal const string PendingPackageMigrationsStateKey = "PendingPackageMigrations";

    private readonly IOptions<GlobalSettings> _globalSettings = null!;
    private readonly IOptions<UnattendedSettings> _unattendedSettings = null!;
    private readonly IUmbracoVersion _umbracoVersion = null!;
    private readonly IUmbracoDatabaseFactory _databaseFactory = null!;
    private readonly ILogger<RuntimeState> _logger = null!;
    private readonly PendingPackageMigrations _packageMigrationState = null!;
    private readonly Dictionary<string, object> _startupState = new Dictionary<string, object>();
    private readonly IConflictingRouteService _conflictingRouteService = null!;
    private readonly IEnumerable<IDatabaseProviderMetadata> _databaseProviderMetadata = null!;
    private readonly IRuntimeModeValidationService _runtimeModeValidationService = null!;
    private readonly IDatabaseInfo _databaseInfo = null!;

    /// <summary>
    /// The initial <see cref="RuntimeState"/>
    /// </summary>
    public static RuntimeState Booting() => new RuntimeState() { Level = RuntimeLevel.Boot };

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeState" /> class.
    /// </summary>
    private RuntimeState()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeState" /> class.
    /// </summary>
    public RuntimeState(
       IOptions<GlobalSettings> globalSettings,
       IOptions<UnattendedSettings> unattendedSettings,
       IUmbracoVersion umbracoVersion,
       IUmbracoDatabaseFactory databaseFactory,
       ILogger<RuntimeState> logger,
       PendingPackageMigrations packageMigrationState,
       IConflictingRouteService conflictingRouteService,
       IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata,
       IRuntimeModeValidationService runtimeModeValidationService,
       IDatabaseInfo databaseInfo)
    {
        _globalSettings = globalSettings;
        _unattendedSettings = unattendedSettings;
        _umbracoVersion = umbracoVersion;
        _databaseFactory = databaseFactory;
        _logger = logger;
        _packageMigrationState = packageMigrationState;
        _conflictingRouteService = conflictingRouteService;
        _databaseProviderMetadata = databaseProviderMetadata;
        _runtimeModeValidationService = runtimeModeValidationService;
        _databaseInfo = databaseInfo;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeState" /> class.
    /// </summary>
    [Obsolete("Use ctor with all params. This will be removed in Umbraco 12.")]
    public RuntimeState(
        IOptions<GlobalSettings> globalSettings,
        IOptions<UnattendedSettings> unattendedSettings,
        IUmbracoVersion umbracoVersion,
        IUmbracoDatabaseFactory databaseFactory,
        ILogger<RuntimeState> logger,
        PendingPackageMigrations packageMigrationState,
        IConflictingRouteService conflictingRouteService,
        IEnumerable<IDatabaseProviderMetadata> databaseProviderMetadata)
        : this(
            globalSettings,
            unattendedSettings,
            umbracoVersion,
            databaseFactory,
            logger,
            packageMigrationState,
            conflictingRouteService,
            databaseProviderMetadata,
            StaticServiceProvider.Instance.GetRequiredService<IRuntimeModeValidationService>(),
            StaticServiceProvider.Instance.GetRequiredService<IDatabaseInfo>())
    { }

    [Obsolete("Use ctor with all params. This will be removed in Umbraco 12.")]
    public RuntimeState(
        IOptions<GlobalSettings> globalSettings,
        IOptions<UnattendedSettings> unattendedSettings,
        IUmbracoVersion umbracoVersion,
        IUmbracoDatabaseFactory databaseFactory,
        ILogger<RuntimeState> logger,
        PendingPackageMigrations packageMigrationState,
        IConflictingRouteService conflictingRouteService)
        : this(
            globalSettings,
            unattendedSettings,
            umbracoVersion,
            databaseFactory,
            logger,
            packageMigrationState,
            StaticServiceProvider.Instance.GetRequiredService<IConflictingRouteService>(),
            StaticServiceProvider.Instance.GetServices<IDatabaseProviderMetadata>())
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="RuntimeState" /> class.
    /// </summary>
    [Obsolete("Use ctor with all params. This will be removed in Umbraco 12.")]
    public RuntimeState(
        IOptions<GlobalSettings> globalSettings,
        IOptions<UnattendedSettings> unattendedSettings,
        IUmbracoVersion umbracoVersion,
        IUmbracoDatabaseFactory databaseFactory,
        ILogger<RuntimeState> logger,
        PendingPackageMigrations packageMigrationState)
        : this(
            globalSettings,
            unattendedSettings,
            umbracoVersion,
            databaseFactory,
            logger,
            packageMigrationState,
            StaticServiceProvider.Instance.GetRequiredService<IConflictingRouteService>())
    { }

    /// <inheritdoc />
    public Version Version => _umbracoVersion.Version;

    /// <inheritdoc />
    public string VersionComment => _umbracoVersion.Comment;

    /// <inheritdoc />
    public SemVersion SemanticVersion => _umbracoVersion.SemanticVersion;

    /// <inheritdoc />
    public string? CurrentMigrationState { get; private set; }

    /// <inheritdoc />
    public string? FinalMigrationState { get; private set; }

    /// <inheritdoc />
    public RuntimeLevel Level { get; internal set; } = RuntimeLevel.Unknown;

    /// <inheritdoc />
    public RuntimeLevelReason Reason { get; internal set; } = RuntimeLevelReason.Unknown;

    /// <inheritdoc />
    public BootFailedException? BootFailedException { get; internal set; }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, object> StartupState => _startupState;

    /// <inheritdoc />
    public void DetermineRuntimeLevel()
    {
        if (_databaseInfo.IsConfigured == false)
        {
            HandleNotConfigured();
            return;
        }

        // Validate runtime mode
        if (_runtimeModeValidationService.Validate(out var validationErrorMessage) == false)
        {
            _logger.LogError(validationErrorMessage);

            Level = RuntimeLevel.BootFailed;
            Reason = RuntimeLevelReason.BootFailedOnException;
            BootFailedException = new BootFailedException(validationErrorMessage);

            return;
        }

        // Check if we have multiple controllers with the same name.
        if (_conflictingRouteService.HasConflictingRoutes(out string controllerName))
        {
            var message = $"Conflicting routes, you cannot have multiple controllers with the same name: {controllerName}";
            _logger.LogError(message);

            Level = RuntimeLevel.BootFailed;
            Reason = RuntimeLevelReason.BootFailedOnException;
            BootFailedException = new BootFailedException(message);

            return;
        }

        // Check the database state, whether we can connect or if it's in an upgrade or empty state, etc...
        switch (_databaseInfo.GetStateAsync().GetAwaiter().GetResult())
        {
            case DatabaseState.CannotConnect:
                {
                    // cannot connect to configured database, this is bad, fail
                    _logger.LogDebug("Could not connect to database.");

                    if (_globalSettings.Value.InstallMissingDatabase || _databaseProviderMetadata.CanForceCreateDatabase(_databaseFactory))
                    {
                        // ok to install on a configured but missing database
                        Level = RuntimeLevel.Install;
                        Reason = RuntimeLevelReason.InstallMissingDatabase;
                        return;
                    }

                    // else it is bad enough that we want to throw
                    Reason = RuntimeLevelReason.BootFailedCannotConnectToDatabase;
                    BootFailedException = new BootFailedException("A connection string is configured but Umbraco could not connect to the database.");
                    throw BootFailedException;
                }
            case DatabaseState.NotInstalled:
                {
                    // ok to install on an empty database
                    Level = RuntimeLevel.Install;
                    Reason = RuntimeLevelReason.InstallEmptyDatabase;
                    return;
                }
            case DatabaseState.NeedsUpgrade:
                {
                    var upgrader = new Upgrader(new UmbracoPlan(_umbracoVersion));
                    var stateValueKey = upgrader.StateValueKey;

                    CurrentMigrationState = _databaseInfo.CurrentMigrationState(stateValueKey).GetAwaiter().GetResult();

                    FinalMigrationState = _databaseInfo.FinalMigrationState().GetAwaiter().GetResult();
                    _logger.LogDebug("Final upgrade state is {FinalMigrationState}, database contains {DatabaseState}", FinalMigrationState, CurrentMigrationState ?? "<null>");
                    // the db version does not match... but we do have a migration table
                    // so, at least one valid table, so we quite probably are installed & need to upgrade

                    // although the files version matches the code version, the database version does not
                    // which means the local files have been upgraded but not the database - need to upgrade
                    _logger.LogDebug("Has not reached the final upgrade step, need to upgrade Umbraco.");
                    Level = _unattendedSettings.Value.UpgradeUnattended ? RuntimeLevel.Run : RuntimeLevel.Upgrade;
                    Reason = RuntimeLevelReason.UpgradeMigrations;
                }
                break;
            case DatabaseState.NeedsPackageMigration:

                // no matter what the level is run for package migrations.
                // they either run unattended, or only manually via the back office.
                Level = RuntimeLevel.Run;

                if (_unattendedSettings.Value.PackageMigrationsUnattended)
                {
                    _logger.LogDebug("Package migrations need to execute.");
                    Reason = RuntimeLevelReason.UpgradePackageMigrations;
                }
                else
                {
                    _logger.LogInformation("Package migrations need to execute but unattended package migrations is disabled. They will need to be run from the back office.");
                    Reason = RuntimeLevelReason.Run;
                }

                break;
            case DatabaseState.NotConfigured:
                HandleNotConfigured();
                break;
            case DatabaseState.Ok:
            default:
                {
                    // the database version matches the code & files version, all clear, can run
                    Level = RuntimeLevel.Run;
                    Reason = RuntimeLevelReason.Run;
                }
                break;
        }
    }

    private void HandleNotConfigured()
    {
        // local version *does* match code version, but the database is not configured
        // install - may happen with Deploy/Cloud/etc
        _logger.LogDebug("Database is not configured, need to install Umbraco.");

        Level = RuntimeLevel.Install;
        Reason = RuntimeLevelReason.InstallNoDatabase;
    }

    public void Configure(RuntimeLevel level, RuntimeLevelReason reason, Exception? bootFailedException = null)
    {
        Level = level;
        Reason = reason;

        if (bootFailedException != null)
        {
            BootFailedException = new BootFailedException(bootFailedException.Message, bootFailedException);
        }
    }
}
