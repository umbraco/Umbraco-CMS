// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.IO;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration.Testing
{
    public class TestDatabaseFactory
    {
        public static ITestDatabase Create(string filesPath, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
        {
            string connectionString = Environment.GetEnvironmentVariable("UmbracoIntegrationTestConnectionString");

            return string.IsNullOrEmpty(connectionString)
                ? CreateLocalDb(filesPath, loggerFactory, dbFactory)
                : CreateSqlDeveloper(loggerFactory, dbFactory, connectionString);
        }

        private static ITestDatabase CreateLocalDb(string filesPath, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
        {
            if (!Directory.Exists(filesPath))
            {
                Directory.CreateDirectory(filesPath);
            }

            var localDb = new LocalDb();

            if (!localDb.IsAvailable)
            {
                throw new InvalidOperationException("LocalDB is not available.");
            }

            return new LocalDbTestDatabase(loggerFactory, localDb, filesPath, dbFactory.Create());
        }

        private static ITestDatabase CreateSqlDeveloper(ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory, string connectionString)
        {
            // NOTE: Example setup for Linux box.
            // $ export SA_PASSWORD=Foobar123!
            // $ export UmbracoIntegrationTestConnectionString="Server=localhost,1433;User Id=sa;Password=$SA_PASSWORD;"
            // $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=$SA_PASSWORD" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ENV: UmbracoIntegrationTestConnectionString is not set");
            }

            return new SqlDeveloperTestDatabase(loggerFactory, dbFactory.Create(), connectionString);
        }
    }
}
