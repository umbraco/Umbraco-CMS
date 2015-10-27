namespace Umbraco.Tests.TestHelpers
{
    /// <summary>
    /// The behavior used to control how the database is handled for test fixtures inheriting from BaseDatabaseFactoryTest
    /// </summary>
    public enum DatabaseBehavior
    {
        /// <summary>
        /// A database is not required whatsoever for the fixture
        /// </summary>
        NoDatabasePerFixture,

        /// <summary>
        /// For each test a new database file and schema will be created
        /// </summary>
        NewDbFileAndSchemaPerTest,
      
        /// <summary>
        /// Creates a new database file and schema for the whole fixture, each test will use the pre-existing one
        /// </summary>
        NewDbFileAndSchemaPerFixture,

        /// <summary>
        /// For each test a new database file without a schema
        /// </summary>
        EmptyDbFilePerTest,
      
    }
}