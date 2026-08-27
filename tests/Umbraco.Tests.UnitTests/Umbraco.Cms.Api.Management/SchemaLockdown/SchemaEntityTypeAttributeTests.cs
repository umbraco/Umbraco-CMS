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

    // Every governed controller declares its entity type on a base class and is served by concrete subclasses, so
    // the declaration only reaches those subclasses' actions by being inherited.
    [Test]
    public void Declaration_On_A_Base_Controller_Is_Inherited()
        => Assert.That(
            typeof(ControllerWithoutOwnDeclaration).GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: true)?.EntityType,
            Is.EqualTo(Constants.UdiEntityType.MediaType));

    // MVC only collects the attribute as a filter through this interface, so losing the implementation would
    // silently leave every governed controller ungoverned.
    [Test]
    public void Is_An_Authorization_Filter()
        => Assert.That(
            new SchemaEntityTypeAttribute(Constants.UdiEntityType.DocumentType),
            Is.InstanceOf<IAsyncAuthorizationFilter>());
}
