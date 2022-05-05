// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Data.Common;
using System.Text.RegularExpressions;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDbMeta
    {
        public string Name { get; }
        public bool IsEmpty { get; }
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public string Path { get; set; } // Null if not embedded.
        public DbConnection Connection { get; set; } // for SQLite in memory, can move to subclass later.

        public TestDbMeta(string name, bool isEmpty, string connectionString, string providerName, string path)
        {
            IsEmpty = isEmpty;
            Name = name;
            ConnectionString = connectionString;
            Provider = providerName;
            Path = path;
        }

        private static string ConstructConnectionString(string masterConnectionString, string databaseName)
        {
            string prefix = Regex.Replace(masterConnectionString, "Database=.+?;", string.Empty);
            string connectionString = $"{prefix};Database={databaseName};";
            return connectionString.Replace(";;", ";");
        }

        public static TestDbMeta CreateWithMasterConnectionString(string name, bool isEmpty, string masterConnectionString) =>
            new TestDbMeta(name, isEmpty, ConstructConnectionString(masterConnectionString, name), Persistence.SqlServer.Constants.ProviderName, null);

        // LocalDb mdf funtimes
        public static TestDbMeta CreateWithoutConnectionString(string name, bool isEmpty) =>
            new TestDbMeta(name, isEmpty, null, Persistence.SqlServer.Constants.ProviderName, null);

        public ConnectionStrings ToStronglyTypedConnectionString() =>
            new ConnectionStrings
            {
                Name = Name,
                ConnectionString = ConnectionString,
                ProviderName = Provider
            };
    }
}
