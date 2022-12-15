// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.Integration.Testing;

/// <summary>
///     Manages a pool of LocalDb databases for integration testing
/// </summary>
public class LocalDbTestDatabase : ITestDatabase
{
    public const string InstanceName = "UmbracoTests";
    public const string DatabaseName = "UmbracoTests";
    private static LocalDb.Instance s_localDbInstance;
    private static string s_filesPath;
    private readonly LocalDb _localDb;
    private readonly IUmbracoDatabaseFactory _databaseFactory;

    private readonly TestDatabaseSettings _settings;

    // It's internal because `Umbraco.Core.Persistence.LocalDb` is internal
    internal LocalDbTestDatabase(LocalDb localDb, IUmbracoDatabaseFactory databaseFactory)
    {
        _localDb = localDb;
        _databaseFactory = databaseFactory;
        string? projectDirectory = Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
        string tempFolder = @"TEMP\databases";
        s_filesPath = Path.Combine(projectDirectory!, tempFolder);

        s_localDbInstance = _localDb.GetInstance(InstanceName);
        if (s_localDbInstance != null)
        {
            return;
        }

        if (_localDb.CreateInstance(InstanceName) == false)
        {
            throw new Exception("Failed to create a LocalDb instance.");
        }

        s_localDbInstance = _localDb.GetInstance(InstanceName);
    }

    public ConnectionStrings Initialize()
    {
        var tempName = Guid.NewGuid().ToString("N");
        s_localDbInstance.CreateDatabase(tempName, s_filesPath);
        var connectionStrings = new ConnectionStrings
        {
            ConnectionString = s_localDbInstance.GetConnectionString(InstanceName, tempName),
            ProviderName = "Microsoft.Data.SqlClient",
        };
        
        _databaseFactory.Configure(connectionStrings);
        return connectionStrings;
    }

    public void Teardown()
    {
        if (s_filesPath == null)
        {
            return;
        }

        var filename = Path.Combine(s_filesPath, DatabaseName).ToUpper();

        Parallel.ForEach(s_localDbInstance.GetDatabases(), instance =>
        {
            if (instance.StartsWith(filename))
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
