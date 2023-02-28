// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Manages a pool of LocalDb databases for integration testing
/// </summary>
public class LocalDbTestDatabaseConfiguration : ITestDatabaseConfiguration
{
    private const string InstanceName = "UmbracoIntegrationTest";
    private readonly string _key = Guid.NewGuid().ToString();
    private static LocalDb.Instance s_localDbInstance;
    private static string s_filesPath;
    private readonly LocalDb _localDb;
    private readonly TestHelper _testHelper = new();

    private readonly TestDatabaseSettings _settings;

    // It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
    internal LocalDbTestDatabaseConfiguration(LocalDb localDb)
    {
        _localDb = localDb;

        s_filesPath = Path.Combine(_testHelper.WorkingDirectory, "databases");

        if (!Directory.Exists(s_filesPath))
        {
            Directory.CreateDirectory(s_filesPath);
        }

        s_localDbInstance = _localDb.GetInstance(InstanceName);
        if (s_localDbInstance != null)
        {
            return;
        }

        if (_localDb.CreateInstance(InstanceName) == false)
        {
            throw new Exception("Failed to create a LocalDb instance.");
        }

        // It looks wierd that we call this twice, but its the only way to get the database after it has been created.
        s_localDbInstance = _localDb.GetInstance(InstanceName);

    }

    public ConnectionStrings InitializeConfiguration()
    {
        s_localDbInstance.CreateDatabase(_key, s_filesPath);
        var connectionStrings = new ConnectionStrings
        {
            ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, _key),
            ProviderName = "Microsoft.Data.SqlClient",
        };
        connectionStrings.ConnectionString += ";TrustServerCertificate=true;";
        return connectionStrings;
    }

    public string Key => _key;

    public void Teardown(string key)
    {
        s_localDbInstance.KillConnections(key);
        s_localDbInstance.DropDatabase(key);
        _localDb.StopInstance(InstanceName);
        _localDb.DropInstance(InstanceName);
    }
}
