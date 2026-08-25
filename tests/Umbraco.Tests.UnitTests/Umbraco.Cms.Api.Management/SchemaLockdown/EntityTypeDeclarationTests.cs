using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

// [SchemaEntityType] carries a free-form string, so a misspelled entity type still compiles and simply never
// matches what a configurator wrote - the controller would look governed and be silently exempt. This is what
// makes that fail, and it reads the Management API assembly rather than the current AppDomain so no test
// fixture's own [SchemaEntityType] declaration can stand in for a real controller.
[TestFixture]
public class EntityTypeDeclarationTests
{
    private static readonly (Type Controller, string EntityType)[] Declarations =
        typeof(SchemaEntityTypeAttribute).Assembly
            .GetTypes()
            .Select(type => (Controller: type, Attribute: type.GetCustomAttribute<SchemaEntityTypeAttribute>(inherit: false)))
            .Where(declaration => declaration.Attribute is not null)
            .Select(declaration => (declaration.Controller, declaration.Attribute!.EntityType))
            .ToArray();

    private static readonly string[] UdiEntityTypes =
        typeof(Constants.UdiEntityType)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!)
            .ToArray();

    [Test]
    public void Every_Declared_Entity_Type_Is_A_Real_Udi_Entity_Type()
    {
        Assert.That(Declarations, Is.Not.Empty);

        var unknown = Declarations
            .Where(declaration => UdiEntityTypes.Contains(declaration.EntityType) is false)
            .Select(declaration => $"{declaration.Controller.FullName} declares \"{declaration.EntityType}\"")
            .ToArray();

        Assert.That(
            unknown,
            Is.Empty,
            $"Entity types outside {nameof(Constants)}.{nameof(Constants.UdiEntityType)}: {string.Join(", ", unknown)}.");
    }
}
