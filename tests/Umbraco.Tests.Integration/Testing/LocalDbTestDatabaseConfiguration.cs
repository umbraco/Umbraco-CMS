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
    public const string InstanceName = "UmbracoIntegrationTest";
    public const string DatabaseName = "UmbracoTests";
    private static LocalDb.Instance s_localDbInstance;
    private static string s_filesPath;
    private readonly LocalDb _localDb;
    private readonly IUmbracoDatabaseFactory _databaseFactory;
    private readonly IOptionsMonitor<ConnectionStrings> _connectionStrings;
    private TestHelper _testHelper = new ();

    private readonly TestDatabaseSettings _settings;

    // It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
    internal LocalDbTestDatabaseConfiguration(LocalDb localDb, IUmbracoDatabaseFactory databaseFactory, IOptionsMonitor<ConnectionStrings> connectionStrings)
    {
        _localDb = localDb;
        _databaseFactory = databaseFactory;
        _connectionStrings = connectionStrings;
        // string? projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        // string tempFolder = @"TEMP\databases";
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
        s_localDbInstance.CreateDatabase(DatabaseName, s_filesPath);
        var connectionStrings = new ConnectionStrings
        {
            ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, DatabaseName),
            ProviderName = "Microsoft.Data.SqlClient",
        };
        connectionStrings.ConnectionString += ";TrustServerCertificate=true;";

        _connectionStrings.CurrentValue.ConnectionString = connectionStrings.ConnectionString;
        _connectionStrings.CurrentValue.ProviderName = connectionStrings.ProviderName;

        _databaseFactory.Configure(connectionStrings);
        return connectionStrings;
    }

    public void Teardown()
    {
        if (s_filesPath == null)
        {
            return;
        }

        Parallel.ForEach(s_localDbInstance.GetDatabases(), instance =>
        {
            if (instance.StartsWith(DatabaseName))
            {
                s_localDbInstance.DropDatabase(instance);
            }
        });

        _localDb.StopInstance(InstanceName);

        foreach (var file in Directory.EnumerateFiles(s_filesPath))
        {
            if (file.EndsWith(".mdf") == false && file.EndsWith(".ldf") == false)
            {
                continue;
            }

            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                // ignore, must still be in use but nothing we can do
            }
        }
    }
}
