using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class EntityTypeAttributeTests
{
    [EntityType(Constants.UdiEntityType.MediaType)]
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
            typeof(ControllerWithoutOwnDeclaration).GetCustomAttribute<EntityTypeAttribute>(inherit: true)?.EntityType,
            Is.EqualTo(Constants.UdiEntityType.MediaType));

    [Test]
    public void Yields_A_Single_Requirement_Carrying_The_Declared_Entity_Type()
    {
        var attribute = new EntityTypeAttribute(Constants.UdiEntityType.DocumentType);

        IAuthorizationRequirement[] requirements = attribute.GetRequirements().ToArray();

        Assert.That(requirements, Has.Length.EqualTo(1));
        Assert.That(requirements[0], Is.TypeOf<SchemaLockdownEntityTypeRequirement>());
        Assert.That(
            ((SchemaLockdownEntityTypeRequirement)requirements[0]).EntityType,
            Is.EqualTo(Constants.UdiEntityType.DocumentType));
    }

    // The framework only reaches GetRequirements through this interface, so losing the implementation would silently
    // leave every governed controller ungoverned.
    [Test]
    public void Is_Authorization_Requirement_Data()
        => Assert.That(
            new EntityTypeAttribute(Constants.UdiEntityType.DocumentType),
            Is.InstanceOf<IAuthorizationRequirementData>());
}
