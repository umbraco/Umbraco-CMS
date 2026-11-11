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
    [RequiresPermissionsFor(Constants.UdiEntityType.MediaType)]
    private class ControllerBaseWithDeclaration
    {
    }

    private class ControllerWithoutOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [RequiresPermissionsFor(Constants.UdiEntityType.DocumentType)]
    private class ControllerWithItsOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [Test]
    public void Declaration_On_A_Base_Controller_Is_Inherited()
        => Assert.That(
            typeof(ControllerWithoutOwnDeclaration).GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: true)?.EntityType,
            Is.EqualTo(Constants.UdiEntityType.MediaType));

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
            Assert.That(declared[0].EntityType, Is.EqualTo(Constants.UdiEntityType.DocumentType));
        });
    }

    // MVC only runs the attribute as a filter through this interface.
    [Test]
    public void Is_An_Authorization_Filter()
        => Assert.That(
            new RequiresPermissionsForAttribute(Constants.UdiEntityType.DocumentType),
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
    public void The_Entity_Types_Casing_Does_Not_Change_The_Permission()
        => Assert.That(
            new RequiresPermissionsForAttribute("Document-Type").InferPermission(HttpMethods.Post),
            Is.EqualTo(Constants.Permissions.DocumentType.Create));

    private static string? InferForDocumentType(string? httpMethod)
        => new RequiresPermissionsForAttribute(Constants.UdiEntityType.DocumentType).InferPermission(httpMethod);
}
