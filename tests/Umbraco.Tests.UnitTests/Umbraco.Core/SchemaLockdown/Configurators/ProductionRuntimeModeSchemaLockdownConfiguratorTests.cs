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
    private static SchemaRestrictions CreateRestrictions(RuntimeMode mode)
        => new(new SchemaLockdownConfiguratorCollection(
            () => [new ProductionRuntimeModeSchemaLockdownConfigurator(Options.Create(new RuntimeSettings { Mode = mode }))]));

    [Test]
    public void Production_Denies_Creating_And_Updating_File_Backed_Schema()
    {
        SchemaRestrictions restrictions = CreateRestrictions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsDenied(Constants.Permissions.Template.Create), Is.True);
            Assert.That(restrictions.IsDenied(Constants.Permissions.Template.Update), Is.True);
            Assert.That(restrictions.IsDenied(Constants.Permissions.PartialView.Create), Is.True);
            Assert.That(restrictions.IsDenied(Constants.Permissions.PartialView.Update), Is.True);
        });
    }

    // Deleting a template only removes its database record; a partial view is only its file.
    [Test]
    public void Production_Permits_Deleting_Templates_But_Not_Partial_Views()
    {
        SchemaRestrictions restrictions = CreateRestrictions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            Assert.That(restrictions.IsDenied(Constants.Permissions.Template.Delete), Is.False);
            Assert.That(restrictions.IsDenied(Constants.Permissions.PartialView.Delete), Is.True);
        });
    }

    [Test]
    public void Production_Denies_Only_File_Backed_Schema()
        => Assert.That(
            CreateRestrictions(RuntimeMode.Production).DeniedPermissions,
            Is.EquivalentTo(new[]
            {
                Constants.Permissions.Template.Create,
                Constants.Permissions.Template.Update,
                Constants.Permissions.PartialView.Create,
                Constants.Permissions.PartialView.Update,
                Constants.Permissions.PartialView.Delete,
            }));

    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    public void Other_Runtime_Modes_Deny_Nothing(RuntimeMode mode)
        => Assert.That(CreateRestrictions(mode).DeniedPermissions, Is.Empty);
}
