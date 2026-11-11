using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Permissions;

[TestFixture]
public class RequiresPermissionsForAttributeTests
{
    [RequiresPermissionsFor(Constants.Permissions.MediaType.Prefix)]
    private class ControllerBaseWithDeclaration
    {
    }

    private class ControllerWithoutOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [RequiresPermissionsFor(Constants.Permissions.DocumentType.Prefix)]
    private class ControllerWithItsOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [Test]
    public void Declaration_On_A_Base_Controller_Is_Inherited()
        => Assert.That(
            typeof(ControllerWithoutOwnDeclaration).GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: true)?.PermissionPrefix,
            Is.EqualTo(Constants.Permissions.MediaType.Prefix));

    // With AllowMultiple, MVC would run both declarations and the base's would still apply.
    [Test]
    public void Declaration_On_A_Controller_Replaces_The_One_It_Inherits()
    {
        RequiresPermissionsForAttribute[] declared = typeof(ControllerWithItsOwnDeclaration)
            .GetCustomAttributes<RequiresPermissionsForAttribute>(inherit: true)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(declared, Has.Length.EqualTo(1));
            Assert.That(declared[0].PermissionPrefix, Is.EqualTo(Constants.Permissions.DocumentType.Prefix));
        });
    }

    // MVC only runs the attribute as a filter through this interface.
    [Test]
    public void Is_An_Authorization_Filter()
        => Assert.That(
            new RequiresPermissionsForAttribute(Constants.Permissions.DocumentType.Prefix),
            Is.InstanceOf<IAsyncAuthorizationFilter>());

    [TestCase("POST", Constants.Permissions.DocumentType.Create)]
    [TestCase("PUT", Constants.Permissions.DocumentType.Update)]
    [TestCase("PATCH", Constants.Permissions.DocumentType.Update)]
    [TestCase("DELETE", Constants.Permissions.DocumentType.Delete)]
    public void Infers_The_Permission_From_The_Http_Method(string httpMethod, string expected)
        => Assert.That(InferForDocumentType(httpMethod), Is.EqualTo(expected));

    [TestCase("TRACE")]
    [TestCase("LOCK")]
    [TestCase(null)]
    public void An_Unrecognised_Or_Missing_Http_Method_Needs_The_Update_Permission(string? httpMethod)
        => Assert.That(InferForDocumentType(httpMethod), Is.EqualTo(Constants.Permissions.DocumentType.Update));

    [TestCase("GET")]
    [TestCase("HEAD")]
    [TestCase("OPTIONS")]
    public void A_Safe_Http_Method_Needs_No_Permission(string httpMethod)
        => Assert.That(InferForDocumentType(httpMethod), Is.Null);

    [Test]
    public void Any_Prefix_Is_Joined_To_The_Operation()
        => Assert.That(
            new RequiresPermissionsForAttribute("My.Package.Widget").InferPermission(HttpMethods.Post),
            Is.EqualTo("My.Package.Widget.Create"));

    [TestCase("Umb.DocumentType.")]
    [TestCase("Umb.DocumentType..")]
    public void A_Trailing_Dot_Is_Ignored(string permissionPrefix)
        => Assert.That(
            new RequiresPermissionsForAttribute(permissionPrefix).InferPermission(HttpMethods.Post),
            Is.EqualTo(Constants.Permissions.DocumentType.Create));

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(".")]
    public void An_Empty_Prefix_Is_Rejected(string permissionPrefix)
        => Assert.Throws<ArgumentException>(() => new RequiresPermissionsForAttribute(permissionPrefix));

    private static string? InferForDocumentType(string? httpMethod)
        => new RequiresPermissionsForAttribute(Constants.Permissions.DocumentType.Prefix).InferPermission(httpMethod);
}
