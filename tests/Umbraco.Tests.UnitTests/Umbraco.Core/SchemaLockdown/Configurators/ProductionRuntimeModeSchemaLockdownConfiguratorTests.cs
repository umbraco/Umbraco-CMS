using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.SchemaLockdown;
using Umbraco.Cms.Core.SchemaLockdown.Configurators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.SchemaLockdown.Configurators;

[TestFixture]
public class ProductionRuntimeModeSchemaLockdownConfiguratorTests
{
    private static readonly string[] FileBackedEntityTypes =
    [
        Constants.UdiEntityType.Template,
        Constants.UdiEntityType.PartialView,
    ];

    private static SchemaRestrictions CreateRestrictions(RuntimeMode mode)
        => new(new SchemaLockdownConfiguratorCollection(
            () => [new ProductionRuntimeModeSchemaLockdownConfigurator(Options.Create(new RuntimeSettings { Mode = mode }))]));

    [Test]
    public void Production_Denies_Creating_And_Updating_File_Backed_Schema()
    {
        SchemaRestrictions restrictions = CreateRestrictions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            foreach (var entityType in FileBackedEntityTypes)
            {
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Create), Is.False, entityType);
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Update), Is.False, entityType);
                Assert.That(restrictions.IsAllowed(entityType, SchemaOperation.Read), Is.True, entityType);
            }
        });
    }

    // Deleting a template only removes its database record, which is how a template removed from the code is cleaned
    // up where it was deployed. A partial view has no record, only the file the deployment owns.
    [Test]
    public void Production_Permits_Deleting_Templates_But_Not_Partial_Views()
    {
        SchemaRestrictions restrictions = CreateRestrictions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.Template, SchemaOperation.Delete), Is.True);
            Assert.That(restrictions.IsAllowed(Constants.UdiEntityType.PartialView, SchemaOperation.Delete), Is.False);
        });
    }

    [Test]
    public void Production_Restricts_Only_File_Backed_Schema()
        => Assert.That(
            CreateRestrictions(RuntimeMode.Production).RestrictedEntityTypes,
            Is.EquivalentTo(FileBackedEntityTypes));

    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    public void Other_Runtime_Modes_Restrict_Nothing(RuntimeMode mode)
        => Assert.That(CreateRestrictions(mode).RestrictedEntityTypes, Is.Empty);
}
