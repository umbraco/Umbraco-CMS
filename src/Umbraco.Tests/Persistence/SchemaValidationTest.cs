using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Migrations.Initial;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture]
    public class SchemaValidationTest : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void DatabaseSchemaCreation_Produces_DatabaseSchemaResult_With_Zero_Errors()
        {
            // Arrange
            var db = DatabaseContext.Database;
            var schema = new DatabaseSchemaCreation(db, Logger, new SqlCeSyntaxProvider());

            // Act
            var result = schema.ValidateSchema();

            // Assert
            Assert.That(result.Errors.Count, Is.EqualTo(0));
            Assert.AreEqual(result.DetermineInstalledVersion(), UmbracoVersion.Current);
        }
    }
}