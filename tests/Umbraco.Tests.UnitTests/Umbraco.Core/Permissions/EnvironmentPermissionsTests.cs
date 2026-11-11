using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Permissions;

[TestFixture]
public class EnvironmentPermissionsTests
{
    private sealed class DelegateConfigurator : IEnvironmentPermissionsConfigurator
    {
        private readonly Action<IEnvironmentPermissionsBuilder> _configure;

        public DelegateConfigurator(Action<IEnvironmentPermissionsBuilder> configure) => _configure = configure;

        public void Configure(IEnvironmentPermissionsBuilder builder) => _configure(builder);
    }

    private static EnvironmentPermissions CreateEnvironmentPermissions(params Action<IEnvironmentPermissionsBuilder>[] configure)
        => new(new EnvironmentPermissionsConfiguratorCollection(
            () => configure.Select(IEnvironmentPermissionsConfigurator (x) => new DelegateConfigurator(x))));

    [Test]
    public void Nothing_Is_Denied_When_No_Configurator_Is_Registered()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions();

        Assert.Multiple(() =>
        {
            Assert.That(environmentPermissions.DeniedPermissions, Is.Empty);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.DocumentType.Create), Is.False);
        });
    }

    [Test]
    public void Only_The_Permissions_A_Configurator_Names_Are_Denied()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(x => x.Deny(Constants.Permissions.DocumentType.Create));

        Assert.Multiple(() =>
        {
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.DocumentType.Create), Is.True);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.DocumentType.Update), Is.False);
            Assert.That(environmentPermissions.IsDenied(Constants.Permissions.DataType.Create), Is.False);
            Assert.That(environmentPermissions.DeniedPermissions, Is.EquivalentTo(new[] { Constants.Permissions.DocumentType.Create }));
        });
    }

    [Test]
    public void Every_Configurators_Denials_Are_Kept_Whatever_The_Order()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(
            x => x.Deny(Constants.Permissions.DocumentType.Delete),
            x => x.Deny(Constants.Permissions.DocumentType.Create),
            x => x.Deny(Constants.Permissions.DataType.Update));

        Assert.That(
            environmentPermissions.DeniedPermissions,
            Is.EquivalentTo(new[]
            {
                Constants.Permissions.DocumentType.Delete,
                Constants.Permissions.DocumentType.Create,
                Constants.Permissions.DataType.Update,
            }));
    }

    [Test]
    public void Denying_The_Same_Permission_Twice_Lists_It_Once()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(
            x => x.Deny(Constants.Permissions.DataType.Create, Constants.Permissions.DataType.Delete),
            x => x.Deny(Constants.Permissions.DataType.Delete));

        Assert.That(
            environmentPermissions.DeniedPermissions,
            Is.EquivalentTo(new[] { Constants.Permissions.DataType.Create, Constants.Permissions.DataType.Delete }));
    }

    [Test]
    public void A_Permission_For_An_Entity_Type_Umbraco_Does_Not_Define_Can_Be_Denied()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(x => x.Deny("Umb.MyWidget.Delete"));

        Assert.That(environmentPermissions.IsDenied("Umb.MyWidget.Delete"), Is.True);
    }

    [Test]
    public void Permissions_Are_Matched_Case_Sensitively()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(x => x.Deny(Constants.Permissions.DocumentType.Create));

        Assert.That(environmentPermissions.IsDenied(Constants.Permissions.DocumentType.Create.ToLowerInvariant()), Is.False);
    }

    [TestCase("")]
    [TestCase(" ")]
    public void Denying_An_Empty_Permission_Throws(string permission)
        => Assert.Throws<ArgumentException>(() => CreateEnvironmentPermissions(x => x.Deny(permission)));

    [Test]
    public void Denied_Permissions_Are_Visible_To_A_Configurator_Still_Running()
    {
        string[]? seen = null;

        CreateEnvironmentPermissions(
            x => x.Deny(Constants.Permissions.DocumentType.Create),
            x => seen = x.DeniedPermissions.ToArray());

        Assert.That(seen, Is.EquivalentTo(new[] { Constants.Permissions.DocumentType.Create }));
    }

    [Test]
    public void Denying_After_Construction_Throws()
    {
        EnvironmentPermissions environmentPermissions = CreateEnvironmentPermissions(x => x.Deny(Constants.Permissions.DocumentType.Create));

        Assert.Throws<InvalidOperationException>(() => environmentPermissions.Deny(Constants.Permissions.DataType.Create));
    }
}
