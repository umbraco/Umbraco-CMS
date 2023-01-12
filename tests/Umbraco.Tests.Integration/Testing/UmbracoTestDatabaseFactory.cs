using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class UmbracoTestDatabaseFactory
{
    private readonly IConfiguration _configuration;

    public UmbracoTestDatabaseFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public ITestDatabaseConfiguration CreateTestDatabaseConfiguration()
    {
        var databaseType = _configuration.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType");

        switch (databaseType)
        {
            case TestDatabaseSettings.TestDatabaseType.Sqlite:
                return new SqliteTestDatabaseConfiguration();
            case TestDatabaseSettings.TestDatabaseType.LocalDb:
                return new LocalDbTestDatabaseConfiguration(new LocalDb());
            case TestDatabaseSettings.TestDatabaseType.SqlServer:
                return new SqlServerTestDatabaseConfiguration(_configuration.GetValue<string>("Tests:Database:SQLServerMasterConnectionString"));
        }

        throw new PanicException("Database not configured in appsettings.Test.Json");
    }
}
