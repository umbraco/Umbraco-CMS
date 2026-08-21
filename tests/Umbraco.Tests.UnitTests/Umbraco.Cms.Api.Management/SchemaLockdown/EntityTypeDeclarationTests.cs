using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core.SchemaLockdown;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

// [EntityType] carries a free-form string, so a value that names no governed entity type still compiles and simply
// never matches the decision table - the controller would look governed and be silently exempt. These tests are what
// makes that mismatch fail, in both directions, and they read the Management API assembly rather than the current
// AppDomain so no test fixture's own [EntityType] declaration can stand in for a real controller.
[TestFixture]
public class EntityTypeDeclarationTests
{
    private static readonly (Type Controller, string EntityType)[] Declarations =
        typeof(EntityTypeAttribute).Assembly
            .GetTypes()
            .Select(type => (Controller: type, Attribute: type.GetCustomAttribute<EntityTypeAttribute>(inherit: false)))
            .Where(declaration => declaration.Attribute is not null)
            .Select(declaration => (declaration.Controller, declaration.Attribute!.EntityType))
            .ToArray();

    [Test]
    public void Every_Declared_Entity_Type_Is_Governed()
    {
        Assert.That(Declarations, Is.Not.Empty);

        var unknown = Declarations
            .Where(declaration => SchemaEntityTypes.All.Contains(declaration.EntityType) is false)
            .Select(declaration => $"{declaration.Controller.FullName} declares \"{declaration.EntityType}\"")
            .ToArray();

        Assert.That(
            unknown,
            Is.Empty,
            $"Entity types outside {nameof(SchemaEntityTypes)}.{nameof(SchemaEntityTypes.All)}: {string.Join(", ", unknown)}.");
    }

    [Test]
    public void Every_Governed_Entity_Type_Is_Declared()
    {
        var undeclared = SchemaEntityTypes.All
            .Where(entityType => Declarations.Any(declaration => declaration.EntityType == entityType) is false)
            .ToArray();

        Assert.That(
            undeclared,
            Is.Empty,
            $"Governed entity types no controller declares: {string.Join(", ", undeclared)}.");
    }
}
