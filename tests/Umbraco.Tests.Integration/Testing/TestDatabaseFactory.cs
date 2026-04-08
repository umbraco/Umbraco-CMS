// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
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
