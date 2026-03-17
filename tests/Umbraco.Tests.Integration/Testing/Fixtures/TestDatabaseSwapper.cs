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
using Umbraco.Cms.Tests.Integration.Testing;

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
    private static readonly Lock s_dbLocker = new();
    private static ITestDatabase? s_dbInstance;

    private ConnectionStrings? _controlledConnectionStrings;
    private TestDbMeta? _currentMeta;

    /// <summary>
    ///     Performs the initial database setup during host startup.
    ///     This replaces the normal <see cref="UmbracoIntegrationFixtureBase.UseTestDatabase"/> call.
    ///     The key difference is that we pass a <see cref="ConnectionStrings"/> object that we retain
    ///     a reference to, enabling subsequent <see cref="SwapDatabase"/> calls.
    /// </summary>
    public void InitialSetup(
        IServiceProvider services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        var databaseFactory = services.GetRequiredService<IUmbracoDatabaseFactory>();
        var connectionStrings = services.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        _currentMeta = db.AttachSchema();
        _controlledConnectionStrings = _currentMeta.ToStronglyTypedConnectionString();

        databaseFactory.Configure(_controlledConnectionStrings);
        connectionStrings.CurrentValue.ConnectionString = _currentMeta.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = _currentMeta.Provider;

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
        if (_currentMeta is not null)
        {
            db.Detach(_currentMeta);
        }

        // Attach a fresh database
        _currentMeta = db.AttachSchema();

        // Mutate the ConnectionStrings object that UmbracoDatabaseFactory holds a reference to.
        // New NPoco connections will pick up the new connection string automatically
        // because CreateDatabaseInstance() reads ConnectionString via the property getter each time.
        _controlledConnectionStrings.ConnectionString = _currentMeta.ConnectionString;
        _controlledConnectionStrings.ProviderName = _currentMeta.Provider;

        // Also update the IOptionsMonitor<ConnectionStrings> so EF Core picks up the change
        var connectionStrings = services.GetRequiredService<IOptionsMonitor<ConnectionStrings>>();
        connectionStrings.CurrentValue.ConnectionString = _currentMeta.ConnectionString;
        connectionStrings.CurrentValue.ProviderName = _currentMeta.Provider;

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
    ///     Detaches the current database without attaching a new one.
    ///     Call this during final teardown.
    /// </summary>
    public void DetachCurrentDatabase(
        IServiceProvider services,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        if (_currentMeta is null)
        {
            return;
        }

        var db = GetOrCreateDatabase(
            services.GetRequiredService<ILoggerFactory>(),
            services.GetRequiredService<TestUmbracoDatabaseFactoryProvider>(),
            configuration,
            testHelper);

        db.Detach(_currentMeta);
        _currentMeta = null;
    }

    private static ITestDatabase GetOrCreateDatabase(
        ILoggerFactory loggerFactory,
        TestUmbracoDatabaseFactoryProvider dbFactory,
        Microsoft.Extensions.Configuration.IConfiguration configuration,
        TestHelper testHelper)
    {
        lock (s_dbLocker)
        {
            if (s_dbInstance is not null)
            {
                return s_dbInstance;
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
            s_dbInstance = TestDatabaseFactory.Create(settings, dbFactory, loggerFactory);
            return s_dbInstance;
        }
    }
}
