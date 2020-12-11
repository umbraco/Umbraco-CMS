using System;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Umbraco.Core.Persistence;

namespace Umbraco.Tests.Integration.Testing
{
    public class TestDatabaseFactory
    {
        public static ITestDatabase Create(string filesPath, ILoggerFactory loggerFactory, TestUmbracoDatabaseFactoryProvider dbFactory)
        {
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? CreateLocalDb(filesPath, loggerFactory, dbFactory.Create())
                : CreateSqlDeveloper(loggerFactory, dbFactory.Create());
        }

        private static ITestDatabase CreateLocalDb(string filesPath, ILoggerFactory loggerFactory, IUmbracoDatabaseFactory dbFactory)
        {
            var localDb = new LocalDb();

            if (!localDb.IsAvailable)
            {
                throw new InvalidOperationException("LocalDB is not available.");
            }

            return new LocalDbTestDatabase(loggerFactory, localDb, filesPath, dbFactory);
        }

        private static ITestDatabase CreateSqlDeveloper(ILoggerFactory loggerFactory, IUmbracoDatabaseFactory dbFactory)
        {
            // $ export SA_PASSWORD=Foobar123!
            // $ export UmbracoIntegrationTestConnectionString="Server=localhost,1433;User Id=sa;Password=$SA_PASSWORD;"
            // $ docker run -e 'ACCEPT_EULA=Y' -e "SA_PASSWORD=$SA_PASSWORD" -e 'MSSQL_PID=Developer' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2017-latest-ubuntu
            var connectionString = Environment.GetEnvironmentVariable("UmbracoIntegrationTestConnectionString");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("ENV: UmbracoIntegrationTestConnectionString is not set");
            }

            return new SqlDeveloperTestDatabase(loggerFactory, dbFactory, connectionString);
        }
    }
}
