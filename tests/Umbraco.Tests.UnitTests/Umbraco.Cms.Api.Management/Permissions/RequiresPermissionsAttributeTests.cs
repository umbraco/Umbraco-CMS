using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Permissions;

[TestFixture]
internal sealed class RequiresPermissionsAttributeTests
{
    [Test]
    public void A_Permission_Declared_Twice_Is_Needed_Once()
        => Assert.That(
            new RequiresPermissionsAttribute([Constants.Permissions.Entities.DocumentType.Update, Constants.Permissions.Entities.DocumentType.Update])
                .Permissions,
            Is.EqualTo(new[] { Constants.Permissions.Entities.DocumentType.Update }));

    [Test]
    public void Declaring_No_Permissions_Needs_None()
        => Assert.That(new RequiresPermissionsAttribute([]).Permissions, Is.Empty);

    [TestCase("")]
    [TestCase(" ")]
    public void An_Empty_Permission_Is_Rejected(string permission)
        => Assert.Throws<ArgumentException>(() => new RequiresPermissionsAttribute([permission]));

    // MVC only runs the attribute as a filter through this interface.
    [Test]
    public void Is_An_Authorization_Filter()
        => Assert.That(new RequiresPermissionsAttribute([]), Is.InstanceOf<IAsyncAuthorizationFilter>());
}
