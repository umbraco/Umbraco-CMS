using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Permissions;

// [RequiresPermissionsFor] takes a free-form string, so a misspelled resource would compile and never match. Only the
// Management API assembly is scanned, so test fixtures can't stand in for real controllers.
[TestFixture]
internal sealed class PermissionResourceDeclarationTests
{
    private const string ResourceField = "Resource";

    private static readonly (Type Controller, string Resource)[] Declarations =
        typeof(RequiresPermissionsForAttribute).Assembly
            .GetTypes()
            .Select(type => (Controller: type, Attribute: type.GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: false)))
            .Where(declaration => declaration.Attribute is not null)
            .Select(declaration => (declaration.Controller, declaration.Attribute!.Resource))
            .ToArray();

    private static readonly string[] KnownResources =
        typeof(Constants.Permissions.Entities)
            .GetNestedTypes()
            .Select(type => type.GetField(ResourceField)?.GetRawConstantValue())
            .OfType<string>()
            .ToArray();

    [Test]
    public void Every_Umbraco_Permission_Resource_Is_Named_After_Its_Constants_Class()
        => Assert.Multiple(() =>
        {
            foreach (Type type in typeof(Constants.Permissions.Entities).GetNestedTypes())
            {
                Assert.That(
                    type.GetField(ResourceField)?.GetRawConstantValue(),
                    Is.EqualTo($"Umb.{type.Name}"),
                    type.Name);
            }
        });

    [Test]
    public void Every_Declared_Resource_Is_One_Of_Umbracos_Permission_Resources()
    {
        Assert.That(Declarations, Is.Not.Empty);

        var unknown = Declarations
            .Where(declaration => KnownResources.Contains(declaration.Resource) is false)
            .Select(declaration => $"{declaration.Controller.FullName} declares \"{declaration.Resource}\"")
            .ToArray();

        Assert.That(
            unknown,
            Is.Empty,
            $"Resources outside Constants.Permissions.Entities: {string.Join(", ", unknown)}.");
    }
}
