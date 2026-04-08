using System.Data.Common;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Persistence.SqlServer;

namespace Umbraco.Cms.Tests.Integration.Testing;

public class TestDatabaseInformation
{
    public TestDatabaseInformation(string name, bool isEmpty, string? connectionString, string providerName, string? path)
    {
        IsEmpty = isEmpty;
        Name = name;
        ConnectionString = connectionString;
        Provider = providerName;
        Path = path;
    }

    public string Name { get; }

    public bool IsEmpty { get; }

    public string? ConnectionString { get; set; }

    public string Provider { get; set; }

    public string? Path { get; set; } // Null if not embedded.

    public DbConnection? Connection { get; set; } // for SQLite in memory.

    private static string ConstructConnectionString(string masterConnectionString, string databaseName)
    {
        var prefix = Regex.Replace(masterConnectionString, "Database=.+?;", string.Empty);
        var connectionString = $"{prefix};Database={databaseName};";
        return connectionString.Replace(";;", ";");
    }

    public static TestDatabaseInformation CreateWithMasterConnectionString(string name, bool isEmpty, string masterConnectionString) =>
            new TestDatabaseInformation(name, isEmpty, ConstructConnectionString(masterConnectionString, name), Persistence.SqlServer.Constants.ProviderName, null);

    public static TestDatabaseInformation CreateWithoutConnectionString(string name, bool isEmpty) =>
        new(name, isEmpty, null, Constants.ProviderName, null);

    public ConnectionStrings ToStronglyTypedConnectionString() =>
        new() { ConnectionString = ConnectionString, ProviderName = Provider };
}
