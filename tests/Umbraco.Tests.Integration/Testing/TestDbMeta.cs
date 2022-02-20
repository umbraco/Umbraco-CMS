// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.RegularExpressions;

namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDbMeta
    {
        public string Name { get; }

        public bool IsEmpty { get; }

        public string ConnectionString { get; set; }

        private TestDbMeta(string name, bool isEmpty, string connectionString)
        {
            IsEmpty = isEmpty;
            Name = name;
            ConnectionString = connectionString;
        }

        private static string ConstructConnectionString(string masterConnectionString, string databaseName)
        {
            string prefix = Regex.Replace(masterConnectionString, "Database=.+?;", string.Empty);
            string connectionString = $"{prefix};Database={databaseName};";
            return connectionString.Replace(";;", ";");
        }

        public static TestDbMeta CreateWithMasterConnectionString(string name, bool isEmpty, string masterConnectionString) =>
            new TestDbMeta(name, isEmpty, ConstructConnectionString(masterConnectionString, name));

        // LocalDb mdf funtimes
        public static TestDbMeta CreateWithoutConnectionString(string name, bool isEmpty) =>
            new TestDbMeta(name, isEmpty, null);
    }
}
