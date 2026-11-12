using Microsoft.Extensions.Options;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Permissions;
using Umbraco.Cms.Core.Permissions.Configurators;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Permissions.Configurators;

[TestFixture]
internal sealed class ProductionRuntimeModeEnvironmentPermissionsConfiguratorTests
{
    private static EnvironmentPermissions CreateEnvironmentPermissions(RuntimeMode mode)
        => new(new EnvironmentPermissionsConfiguratorCollection(
            () => [new ProductionRuntimeModeEnvironmentPermissionsConfigurator(Options.Create(new RuntimeSettings { Mode = mode }))]));

    [Test]
    public void Production_Denies_Creating_And_Updating_File_Backed_Schema()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.Template.Create), Is.True);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.Template.Update), Is.True);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.PartialView.Create), Is.True);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.PartialView.Update), Is.True);
        });
    }

    // Deleting a template only removes its database record; a partial view is only its file.
    [Test]
    public void Production_Permits_Deleting_Templates_But_Not_Partial_Views()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(RuntimeMode.Production);

        Assert.Multiple(() =>
        {
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.Template.Delete), Is.False);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.Entities.PartialView.Delete), Is.True);
        });
    }

    [Test]
    public void Production_Denies_Only_File_Backed_Schema()
        => Assert.That(
            CreateEnvironmentPermissions(RuntimeMode.Production).DeniedPermissions,
            Is.EquivalentTo(new[]
            {
                Constants.Permissions.Entities.Template.Create,
                Constants.Permissions.Entities.Template.Update,
                Constants.Permissions.Entities.PartialView.Create,
                Constants.Permissions.Entities.PartialView.Update,
                Constants.Permissions.Entities.PartialView.Delete,
            }));

    [TestCase(RuntimeMode.Development)]
    [TestCase(RuntimeMode.BackofficeDevelopment)]
    public void Other_Runtime_Modes_Deny_Nothing(RuntimeMode mode)
        => Assert.That(CreateEnvironmentPermissions(mode).DeniedPermissions, Is.Empty);
}
