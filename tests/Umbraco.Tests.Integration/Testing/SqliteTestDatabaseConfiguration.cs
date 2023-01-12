using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Integration.Implementations;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class SqliteTestDatabaseConfiguration : ITestDatabaseConfiguration
{
    private Guid? _key;
    private TestHelper _testHelper = new();
    private string s_filesPath;

    public SqliteTestDatabaseConfiguration()
    {
        _key = Guid.NewGuid();
        s_filesPath = Path.Combine(_testHelper.WorkingDirectory, "databases");

        if (!Directory.Exists(s_filesPath))
        {
            Directory.CreateDirectory(s_filesPath);
        }
    }

    public ConnectionStrings InitializeConfiguration()
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = $"{GetAbsolutePath()}",
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true,
            Pooling = false, // When pooling true, files kept open after connections closed, bad for cleanup.
            Cache = SqliteCacheMode.Shared,
        };

        var connectionStrings = new ConnectionStrings
        {
            ConnectionString = builder.ConnectionString,
            ProviderName = "Microsoft.Data.Sqlite",
        };
        return connectionStrings;
    }

    public string Key => _key.ToString();

    public void Teardown(string key) => TryDeleteFile(GetAbsolutePath(key));

    private string GetAbsolutePath() => Path.Combine(s_filesPath, _key.ToString());

    private string GetAbsolutePath(string key) => Path.Combine(s_filesPath, key);

    private void TryDeleteFile(string filePath)
    {
        // This can sometimes fail if a thread is hanging and Sqlite file is therefore in use
        // which is why we need retry logic that swallows the exceptions.
        const int maxRetries = 5;
        var retries = 0;
        var retry = true;
        do
        {
            try
            {
                File.Delete(filePath);
                retry = false;
            }
            catch (IOException)
            {
                retries++;
                if (retries >= maxRetries)
                {
                    throw;
                }

                Thread.Sleep(500);
            }
        }
        while (retry);
    }
}
