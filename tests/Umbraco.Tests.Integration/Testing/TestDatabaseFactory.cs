// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.Integration.Testing;

public static class TestDatabaseFactory
{
    private static readonly Lock s_lock = new();
    private static ITestDatabase? s_instance;

    /// <summary>
    ///     Gets or creates the shared <see cref="ITestDatabase"/> singleton.
    ///     All test infrastructure paths (UmbracoIntegrationTestBase, UmbracoIntegrationFixtureBase,
    ///     and TestDatabaseSwapper) must share a single instance to avoid competing for the same
    ///     physical databases.
    /// </summary>
    public static ITestDatabase GetOrCreate(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory)
    {
        lock (s_lock)
        {
            if (s_instance is not null)
            {
                return s_instance;
            }

            Directory.CreateDirectory(settings.FilesPath);
            s_instance = Create(settings, dbFactory, loggerFactory);
            return s_instance;
        }
    }

    /// <summary>
    ///     Creates a TestDatabase instance
    /// </summary>
    /// <remarks>
    ///     SQL Server setup requires configured master connection string &amp; privileges to create database.
    /// </remarks>
    /// <example>
    ///     <code>
    /// # SQL Server Environment variable setup
    /// $ export Tests__Database__DatabaseType="SqlServer"
    /// $ export Tests__Database__SQLServerMasterConnectionString="Server=localhost,1433; User Id=sa; Password=MySuperSecretPassword123!;"
    /// </code>
    /// </example>
    /// <example>
    ///     <code>
    /// # Docker cheat sheet
    /// $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=MySuperSecretPassword123!" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
    /// </code>
    /// </example>
    private static ITestDatabase Create(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory) =>
        settings.DatabaseType switch
        {
            TestDatabaseSettings.TestDatabaseType.Sqlite => new SqliteTestDatabase(settings, dbFactory, loggerFactory),
            TestDatabaseSettings.TestDatabaseType.SqlServer => CreateSqlServer(settings, loggerFactory, dbFactory),
            TestDatabaseSettings.TestDatabaseType.LocalDb => CreateLocalDb(settings, loggerFactory, dbFactory),
            _ => throw new ApplicationException("Unsupported test database provider")
        };

    private static ITestDatabase CreateLocalDb(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
    {
        var localDb = new LocalDb();

        if (!localDb.IsAvailable)
        {
            throw new InvalidOperationException("LocalDB is not available.");
        }

        return new LocalDbTestDatabase(settings, loggerFactory, localDb, dbFactory.Create());
    }

    private static ITestDatabase CreateSqlServer(TestDatabaseSettings settings, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory) =>
        new SqlServerTestDatabase(settings, loggerFactory, dbFactory.Create());
}

/// <summary>
///     Diagnostic decorator that wraps an <see cref="ISnapshotableTestDatabase"/> and logs
///     timing information for every operation. Useful for profiling snapshot performance.
///     Enable by uncommenting the wrapper in <see cref="TestDatabaseFactory"/>.
/// </summary>
internal class LoggingSnapshotableTestDatabase : ISnapshotableTestDatabase
{
    private readonly ISnapshotableTestDatabase _innerDb;
    private readonly ILogger _logger;
    private readonly Stopwatch _stopwatch = new();

    public LoggingSnapshotableTestDatabase(ISnapshotableTestDatabase innerDb, ILoggerFactory loggerFactory)
    {
        _innerDb = innerDb;
        _logger = loggerFactory.CreateLogger<LoggingSnapshotableTestDatabase>();
    }

    public TestDatabaseInformation AttachEmpty()
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachEmpty();
        _logger.LogInformation("{Type} attached empty db {Name} in {Elapsed}", _innerDb.GetType().Name, result.Name, _stopwatch.Elapsed);
        return result;
    }

    public TestDatabaseInformation AttachSchema()
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachSchema();
        _logger.LogInformation("{Type} attached schema db {Name} in {Elapsed}", _innerDb.GetType().Name, result.Name, _stopwatch.Elapsed);
        return result;
    }

    public void Detach(TestDatabaseInformation id)
    {
        _stopwatch.Restart();
        _innerDb.Detach(id);
        _logger.LogInformation("{Type} detached db {Name} in {Elapsed}", _innerDb.GetType().Name, id.Name, _stopwatch.Elapsed);
    }

    public bool HasSnapshot(string snapshotKey)
    {
        _stopwatch.Restart();
        var result = _innerDb.HasSnapshot(snapshotKey);
        _logger.LogInformation("{Type} {Result} snapshot {Key} in {Elapsed}", _innerDb.GetType().Name, result ? "found" : "did not find", snapshotKey, _stopwatch.Elapsed);
        return result;
    }

    public void CreateSnapshot(string snapshotKey, TestDatabaseInformation sourceMeta)
    {
        _stopwatch.Restart();
        _innerDb.CreateSnapshot(snapshotKey, sourceMeta);
        _logger.LogInformation("{Type} created snapshot {Key} from {Name} in {Elapsed}", _innerDb.GetType().Name, snapshotKey, sourceMeta.Name, _stopwatch.Elapsed);
    }

    public TestDatabaseInformation AttachFromSnapshot(string snapshotKey)
    {
        _stopwatch.Restart();
        var result = _innerDb.AttachFromSnapshot(snapshotKey);
        _logger.LogInformation("{Type} attached db {Name} from snapshot {Key} in {Elapsed}", _innerDb.GetType().Name, result.Name, snapshotKey, _stopwatch.Elapsed);
        return result;
    }
}
