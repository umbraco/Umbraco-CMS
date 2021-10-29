// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDbMeta
    {
#pragma warning disable SA1516 // That's just your opinion man.
        public string Name { get; }
        public bool IsEmpty { get; }
        public string ConnectionString { get; set; }
        public string Provider { get; set; }
        public string Path { get; set; } // Null if not embedded.
#pragma warning restore SA1516

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
            new TestDbMeta(name, isEmpty, ConstructConnectionString(masterConnectionString, name), Constants.DatabaseProviders.SqlServer, null);

        // LocalDb mdf funtimes
        public static TestDbMeta CreateWithoutConnectionString(string name, bool isEmpty) =>
            new TestDbMeta(name, isEmpty, null, Constants.DatabaseProviders.SqlServer, null);
    }
}
