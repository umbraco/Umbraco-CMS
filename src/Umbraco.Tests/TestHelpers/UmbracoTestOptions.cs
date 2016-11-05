namespace Umbraco.Tests.TestHelpers
{
    public static class UmbracoTestOptions
    {
        public enum Logger
        {
            // pure mocks
            Mock,
            // log4net for tests
            Log4Net
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
    }
}