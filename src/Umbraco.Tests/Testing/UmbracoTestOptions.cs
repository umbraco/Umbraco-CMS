namespace Umbraco.Tests.Testing
{
    public static class UmbracoTestOptions
    {
        public enum Logger
        {
            // pure mocks
            Mock,
            // Serilog for tests
            Serilog,
            // console logger
            Console
        }

        public enum Database
        {
            // no database
            None,
            // new empty database file for the entire feature
            NewEmptyPerFixture,
            // new empty database file per test
            NewEmptyPerTest,
            // new database file with schema for the entire feature
            NewSchemaPerFixture,
            // new database file with schema per test
            NewSchemaPerTest
        }

        public enum PluginManager
        {
            // the default, global plugin manager for tests
            Default,
            // create one plugin manager for the feature
            PerFixture,
            // create one plugin manager for each test
            PerTest
        }
    }
}
