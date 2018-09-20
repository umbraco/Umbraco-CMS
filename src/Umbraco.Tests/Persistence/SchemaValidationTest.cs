using Moq;
using NUnit.Framework;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations.Install;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class SchemaValidationTest : TestWithDatabaseBase
    {
        [Test]
        public void DatabaseSchemaCreation_Produces_DatabaseSchemaResult_With_Zero_Errors()
        {
            DatabaseSchemaResult result;

            using (var scope = ScopeProvider.CreateScope())
            {
                var schema = new DatabaseSchemaCreator(scope.Database, Logger);
                result = schema.ValidateSchema();
            }

            // Assert
            Assert.That(result.Errors.Count, Is.EqualTo(0));
            Assert.AreEqual(result.DetermineInstalledVersion(), UmbracoVersion.Current);
        }
    }
}
