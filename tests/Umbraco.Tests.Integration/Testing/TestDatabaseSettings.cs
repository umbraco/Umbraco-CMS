
namespace Umbraco.Cms.Tests.Integration.Testing
{
    public class TestDatabaseSettings
    {
        public string Engine { get; set; }
        public int PrepareThreadCount { get; set; }
        public int SchemaDatabaseCount { get; set; }
        public int EmptyDatabasesCount { get; set; }
        public string FilesPath { get; set; }
    }
}
