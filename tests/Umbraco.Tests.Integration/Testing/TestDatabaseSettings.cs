
namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDatabaseSettings
    {
        public TestDatabaseType DatabaseType { get; set; }

        public int PrepareThreadCount { get; set; }

        public int SchemaDatabaseCount { get; set; }

        public int EmptyDatabasesCount { get; set; }

        public string FilesPath { get; set; }

        /// <remarks>
        /// Only used for SQL Server e.g. on Linux/MacOS (not required for localdb).
        /// </remarks>
        public string SQLServerMasterConnectionString { get; set; }

        public enum TestDatabaseType
        {
            Unknown,
            Sqlite,
            SqlServer,
            LocalDb
        }
    }
}
