// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing;

public static class TestDatabaseFactory
{
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
    public static ITestDatabase Create(TestDatabaseSettings settings, TestUmbracoDatabaseFactoryProvider dbFactory, ILoggerFactory loggerFactory) =>
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
