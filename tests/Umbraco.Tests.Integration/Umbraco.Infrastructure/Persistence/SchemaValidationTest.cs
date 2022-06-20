using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Infrastructure.Migrations.Install;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewEmptyPerTest)]
public class SchemaValidationTest : UmbracoIntegrationTest
{
    private IUmbracoVersion UmbracoVersion => GetRequiredService<IUmbracoVersion>();

    private IEventAggregator EventAggregator => GetRequiredService<IEventAggregator>();

    [Test]
    public void DatabaseSchemaCreation_Produces_DatabaseSchemaResult_With_Zero_Errors()
    {
        DatabaseSchemaResult result;

        using (ScopeProvider.CreateScope(autoComplete: true))
        {
            var schema = new DatabaseSchemaCreator(
                ScopeAccessor.AmbientScope.Database,
                LoggerFactory.CreateLogger<DatabaseSchemaCreator>(),
                LoggerFactory,
                UmbracoVersion,
                EventAggregator,
                Mock.Of<IOptionsMonitor<InstallDefaultDataSettings>>(x =>
                    x.CurrentValue == new InstallDefaultDataSettings()));
            schema.InitializeDatabaseSchema();
            result = schema.ValidateSchema(DatabaseSchemaCreator._orderedTables);
        }

        // Assert
        Assert.That(result.Errors.Count, Is.EqualTo(0));
    }
}
