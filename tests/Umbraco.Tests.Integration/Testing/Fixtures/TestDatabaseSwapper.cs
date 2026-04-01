using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Navigation;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Security;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Testing.Fixtures;

/// <summary>
///     Swaps the database on an already-running host without restarting it.
///     Works by mutating the <see cref="ConnectionStrings"/> object that was passed to
///     <see cref="IUmbracoDatabaseFactory.Configure"/> during initial setup.
///     New NPoco connections read <see cref="ConnectionStrings.ConnectionString"/> each time,
///     so they automatically pick up the new database.
/// </summary>
public class TestDatabaseSwapper
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _seedLocks = new();
    private static readonly Lock _dbLocker = new();
    private static ITestDatabase? _dbInstance;

    private ConnectionStrings? _controlledConnectionStrings;
    private TestDatabaseInformation? _currentDatabaseInformation;

    /// <summary>
    ///     Performs the initial database setup during host startup.
    ///     This replaces the normal <see cref="UmbracoIntegrationFixtureBase.UseTestDatabase"/> call.
    ///     The key difference is that we pass a <see cref="ConnectionStrings"/> object that we retain
    ///     a reference to, enabling subsequent <see cref="SwapDatabase"/> calls.
    /// </summary>
    public void InitialSetup(
        IServiceProvider services,
        IConfiguration configuration,
        TestHelper testHelper)
    {
        var databaseFactory = services.GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = services.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        _currentDatabaseInformation = db.AttachSchema();
        _controlledConnectionStrings = _currentDatabaseInformation.ToStronglyTypedConnectionString();

        databaseFactory.Configure(_controlledConnectionStrings);
        connectionStrings.CurrentValue.ConnectionString = _currentDatabaseInformation.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = _currentDatabaseInformation.Provider;

        var state = services.GetRequiredService<IRuntimeState>();
        state.DetermineRuntimeLevel();
        services.GetRequiredService<IEventAggregator>().Publish(new UnattendedInstallNotification());
    }

    /// <summary>
    ///     Swaps to a fresh database on an already-running host.
    ///     Detaches the current database, attaches a new one, and mutates the stored
    ///     <see cref="ConnectionStrings"/> object so new connections use the new database.
    /// </summary>
    public async Task SwapDatabaseAsync(
        IServiceProvider services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        if (_controlledConnectionStrings is null)
        {
            throw new InvalidOperationException(
                "InitialSetup must be called before SwapDatabase.");
        }

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        // Detach the current database (return to pool)
        if (_currentDatabaseInformation is not null)
        {
            db.Detach(_currentDatabaseInformation);
        }

        // Attach a fresh database
        _currentDatabaseInformation = db.AttachSchema();

        // Mutate the ConnectionStrings object that UmbracoDatabaseFactory holds a reference to.
        // New NPoco connections will pick up the new connection string automatically
        // because CreateDatabaseInstance() reads ConnectionString via the property getter each time.
        _controlledConnectionStrings.ConnectionString = _currentDatabaseInformation.ConnectionString;
        _controlledConnectionStrings.ProviderName = _currentDatabaseInformation.Provider;

        // Also update the IOptionsMonitor<ConnectionStrings> so EF Core picks up the change
        var connectionStrings = services.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        connectionStrings.CurrentValue.ConnectionString = _currentDatabaseInformation.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = _currentDatabaseInformation.Provider;

        // Re-determine runtime level with the new database
        var state = services.GetRequiredService<IRuntimeState>();
        state.DetermineRuntimeLevel();

        // Publish notification so the system knows we have a fresh install
        services.GetRequiredService<IEventAggregator>().Publish(new UnattendedInstallNotification());

        // Clear the IdKeyMap cache — it holds int-to-Guid mappings from the previous database
        services.GetRequiredService<IIdKeyMap>().ClearCache();

        // Rebuild in-memory navigation structures — they hold tree data from the old database.
        // We call the services directly instead of publishing PostRuntimePremigrationsUpgradeNotification
        // because the handlers are INotificationAsyncHandler (async) and the extension-method
        // overload of PublishAsync dispatches only to INotificationHandler (sync).
        var docNav = services.GetRequiredService<IDocumentNavigationManagementService>();
        await docNav.RebuildAsync();
        await docNav.RebuildBinAsync();

        var mediaNav = services.GetRequiredService<IMediaNavigationManagementService>();
        await mediaNav.RebuildAsync();
        await mediaNav.RebuildBinAsync();

        // Re-initialize publish status cache from the new database
        var publishStatus = services.GetRequiredService<IPublishStatusManagementService>();
        await publishStatus.InitializeAsync(CancellationToken.None);

        // Re-register the OpenIddict backoffice application in the new database.
        // The BackOfficeAuthorizationInitializationMiddleware only runs once per host lifetime,
        // so after a DB swap the new database won't have the OpenIddict application.
        using var scope = services.CreateScope();
        var backOfficeAppManager = scope.ServiceProvider.GetRequiredService<IBackOfficeApplicationManager>();
        var backOfficeHost = new Uri("https://localhost/");
        await backOfficeAppManager.EnsureBackOfficeApplicationAsync([backOfficeHost]);
    }

    /// <summary>
    ///     Swaps to a seeded database on an already-running host.
    ///     If a snapshot for the seed profile's key already exists, restores from it.
    ///     Otherwise, attaches a fresh schema database, runs the seed profile, and creates
    ///     a snapshot so subsequent callers with the same key restore instantly.
    ///     <para>
    ///         The seed profile is responsible for all database-level setup (e.g. unattended
    ///         install, content creation). The swapper handles only the connection swap and
    ///         in-memory cache rebuilds.
    ///     </para>
    /// </summary>
    public async Task SwapToSeededDatabaseAsync(
        IServiceProvider services,
        IConfiguration configuration,
        TestHelper testHelper,
        ITestDatabaseSeedProfile seedProfile)
    {
        if (_controlledConnectionStrings is null)
        {
            throw new InvalidOperationException(
                "InitialSetup must be called before SwapToSeededDatabaseAsync.");
        }

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        if (db is not ISnapshotableTestDatabase snapshotDb)
        {
            // Fallback: swap to a fresh schema DB and seed manually every time
            await SwapDatabaseAsync(services, configuration, testHelper);
            await seedProfile.SeedAsync(services);
            return;
        }

        // Synchronize per seed key so only one thread seeds and snapshots
        var seedLock = _seedLocks.GetOrAdd(seedProfile.SeedKey, _ => new SemaphoreSlim(1, 1));
        await seedLock.WaitAsync();
        try
        {
            // Detach current database
            if (_currentDatabaseInformation is not null)
            {
                db.Detach(_currentDatabaseInformation);
                _currentDatabaseInformation = null;
            }

            if (snapshotDb.HasSnapshot(seedProfile.SeedKey))
            {
                // Restore from existing snapshot
                _currentDatabaseInformation = snapshotDb.AttachFromSnapshot(seedProfile.SeedKey);
            }
            else
            {
                // First caller: attach fresh schema DB, let the seed profile do all setup, then snapshot
                _currentDatabaseInformation = db.AttachSchema();

                // Point the host at the new DB before the seed runs
                MutateConnectionStrings(services, _currentDatabaseInformation);

                try
                {
                    // The seed profile handles everything: install, content creation, etc.
                    await seedProfile.SeedAsync(services);

                    // Snapshot the seeded state
                    snapshotDb.CreateSnapshot(seedProfile.SeedKey, _currentDatabaseInformation);
                }
                catch
                {
                    // Seed or snapshot failed — detach the partially-seeded database
                    // to prevent it from leaking. The next caller will retry with a fresh DB.
                    db.Detach(_currentDatabaseInformation);
                    _currentDatabaseInformation = null;
                    throw;
                }
            }
        }
        finally
        {
            seedLock.Release();
        }

        // Mutate connection strings for the (possibly snapshot-restored) database
        MutateConnectionStrings(services, _currentDatabaseInformation);

        // Re-determine runtime level so the host knows we're in Run state
        var runtimeState = services.GetRequiredService<IRuntimeState>();
        runtimeState.DetermineRuntimeLevel();

        // Rebuild in-memory caches — these are host-side state, not in the DB snapshot
        await RebuildInMemoryCachesAsync(services);
    }

    /// <summary>
    ///     Mutates the stored <see cref="ConnectionStrings"/> and the
    ///     <see cref="IOptionsMonitor{ConnectionStrings}"/> so new connections use the given database.
    /// </summary>
    private void MutateConnectionStrings(IServiceProvider services, TestDatabaseInformation meta)
    {
        _controlledConnectionStrings!.ConnectionString = meta.ConnectionString;
        _controlledConnectionStrings.ProviderName = meta.Provider;

        var connectionStrings = services.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        connectionStrings.CurrentValue.ConnectionString = meta.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = meta.Provider;
    }

    /// <summary>
    ///     Rebuilds in-memory caches that hold data from the previous database.
    ///     Must be called after every database swap regardless of whether the DB
    ///     came from a snapshot or a fresh install.
    /// </summary>
    private static async Task RebuildInMemoryCachesAsync(IServiceProvider services)
    {
        // Clear the IdKeyMap cache — it holds int-to-Guid mappings from the previous database
        services.GetRequiredService<IIdKeyMap>().ClearCache();

        // Rebuild in-memory navigation structures
        var docNav = services.GetRequiredService<IDocumentNavigationManagementService>();
        await docNav.RebuildAsync();
        await docNav.RebuildBinAsync();

        var mediaNav = services.GetRequiredService<IMediaNavigationManagementService>();
        await mediaNav.RebuildAsync();
        await mediaNav.RebuildBinAsync();

        // Re-initialize publish status cache from the new database
        var publishStatus = services.GetRequiredService<IPublishStatusManagementService>();
        await publishStatus.InitializeAsync(CancellationToken.None);
    }

    /// <summary>
    ///     Detaches the current database without attaching a new one.
    ///     Call this during final teardown.
    /// </summary>
    public void DetachCurrentDatabase(
        IServiceProvider services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        if (_currentDatabaseInformation is null)
        {
            return;
        }

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        db.Detach(_currentDatabaseInformation);
        _currentDatabaseInformation = null;
    }

    private static ITestDatabase GetOrCreateDatabase(
        ILoggerFactory loggerFactory,
        TestUmbracoDatabaseFactoryProvider dbFactory,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        lock (_dbLocker)
        {
            if (_dbInstance is not null)
            {
                return _dbInstance;
            }

            var settings = new TestDatabaseSettings
            {
                FilesPath = Path.Combine(testHelper.WorkingDirectory, "databases"),
                DatabaseType = configuration.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType"),
                PrepareThreadCount = configuration.GetValue<int>("Tests:Database:PrepareThreadCount"),
                EmptyDatabasesCount = configuration.GetValue<int>("Tests:Database:EmptyDatabasesCount"),
                SchemaDatabaseCount = configuration.GetValue<int>("Tests:Database:SchemaDatabaseCount"),
                SQLServerMasterConnectionString = configuration.GetValue<string>("Tests:Database:SQLServerMasterConnectionString"),
            };

            Directory.CreateDirectory(settings.FilesPath);
            _dbInstance = TestDatabaseFactory.Create(settings, dbFactory, loggerFactory);
            return _dbInstance;
        }
    }
}
