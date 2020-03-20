using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Migrations.Install;
using Umbraco.Tests.LegacyXmlPublishedCache;
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
                result = schema.ValidateSchema(
                    //TODO: When we remove the xml cache from tests we can remove this too
                    DatabaseSchemaCreator.OrderedTables.Concat(new []{typeof(ContentXmlDto), typeof(PreviewXmlDto)}));
            }

            // Assert
            Assert.That(result.Errors.Count, Is.EqualTo(0));
        }
    }
}
