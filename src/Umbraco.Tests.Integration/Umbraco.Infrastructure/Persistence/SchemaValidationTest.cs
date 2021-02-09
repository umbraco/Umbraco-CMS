using Microsoft.Extensions.Logging;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Core.Configuration;
using Umbraco.Core.Migrations.Install;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class SchemaValidationTest : UmbracoIntegrationTest
    {
        private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

        [Test]
        public void DatabaseSchemaCreation_Produces_DatabaseSchemaResult_With_Zero_Errors()
        {
            DatabaseSchemaResult result;

            using (var scope = ScopeProvider.CreateScope())
            {
                var schema = new DatabaseSchemaCreator(scope.Database, LoggerFactory.CreateLogger<DatabaseSchemaCreator>(), LoggerFactory, UmbracoVersion);
                result = schema.ValidateSchema(DatabaseSchemaCreator.OrderedTables);
            }

            // Assert
            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }
    }
}
