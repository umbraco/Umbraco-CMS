using System.Reflection;
using Microsoft.AspNetCore.Mvc.Filters;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaEntityTypeAttributeTests
{
    [SchemaEntityType(Constants.UdiEntityType.MediaType)]
    private class ControllerBaseWithDeclaration
    {
    }

    private class ControllerWithoutOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [SchemaEntityType(Constants.UdiEntityType.DocumentType)]
    private class ControllerWithItsOwnDeclaration : ControllerBaseWithDeclaration
    {
    }

    [Test]
    public void Declaration_On_A_Base_Controller_Is_Inherited()
        => Assert.That(
            typeof(ControllerWithoutOwnDeclaration).GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true)?.EntityTypes,
            Is.EqualTo(new[] { Constants.UdiEntityType.MediaType }));

    // With AllowMultiple, MVC would run both declarations and the base's would still apply.
    [Test]
    public void Declaration_On_A_Controller_Replaces_The_One_It_Inherits()
    {
        SchemaEntityTypeAttribute[] declared = typeof(ControllerWithItsOwnDeclaration)
            .GetCustomAttributes<SchemaEntityTypeAttribute>(inherit: true)
            .ToArray();

        Assert.Multiple(() =>
        {
            Assert.That(declared, Has.Length.EqualTo(1));
            Assert.That(declared[0].EntityTypes, Is.EqualTo(new[] { Constants.UdiEntityType.DocumentType }));
        });
    }

    // MVC only runs the attribute as a filter through this interface.
    [Test]
    public void Is_An_Authorization_Filter()
        => Assert.That(
            new SchemaEntityTypeAttribute(Constants.UdiEntityType.DocumentType),
            Is.InstanceOf<IAsyncAuthorizationFilter>());
}
