using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Infrastructure.Persistence;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class UmbracoTestDatabaseFactory
{
    private readonly IUmbracoDatabaseFactory _umbracoDatabaseFactory;
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private readonly IConfiguration _configuration;

    public UmbracoTestDatabaseFactory(IUmbracoDatabaseFactory umbracoDatabaseFactory, IOptionsMonitor<ConnectionStrings> connectionStrings, IConfiguration configuration)
    {
        _umbracoDatabaseFactory = umbracoDatabaseFactory;
        _connectionStrings = connectionStrings;
        _configuration = configuration;
    }
    public ITestDatabase CreateTestDatabase()
    {
        var databaseType = _configuration.GetValue<TestDatabaseSettings.TestDatabaseType>("Tests:Database:DatabaseType");

        switch (databaseType)
        {
            case TestDatabaseSettings.TestDatabaseType.Sqlite:
                return new SqliteTestDatabase(_connectionStrings, _umbracoDatabaseFactory, _configuration);
            case TestDatabaseSettings.TestDatabaseType.LocalDb:
                break;
            case TestDatabaseSettings.TestDatabaseType.SqlServer:
                break;
            case TestDatabaseSettings.TestDatabaseType.Unknown:
                throw new PanicException("Database not configured in appsettings.Test.Json");
        }

        return null;
    }
}
