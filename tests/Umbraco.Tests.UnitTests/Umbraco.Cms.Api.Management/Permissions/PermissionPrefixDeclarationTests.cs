using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.Permissions;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.Permissions;

// [RequiresPermissionsFor] takes a free-form string, so a misspelled prefix would compile and never match. Only the
// Management API assembly is scanned, so test fixtures can't stand in for real controllers.
[TestFixture]
public class PermissionPrefixDeclarationTests
{
    private static readonly (Type Controller, string Prefix)[] Declarations =
        typeof(RequiresPermissionsForAttribute).Assembly
            .GetTypes()
            .Select(type => (Controller: type, Attribute: type.GetCustomAttribute<RequiresPermissionsForAttribute>(inherit: false)))
            .Where(declaration => declaration.Attribute is not null)
            .Select(declaration => (declaration.Controller, declaration.Attribute!.PermissionPrefix))
            .ToArray();

    private static readonly string[] KnownPrefixes =
        typeof(Constants.Permissions)
            .GetNestedTypes()
            .Select(type => type.GetField(nameof(Constants.Permissions.DocumentType.Prefix))?.GetRawConstantValue())
            .OfType<string>()
            .ToArray();

    [Test]
    public void Every_Umbraco_Permission_Prefix_Is_Named_After_Its_Constants_Class()
        => Assert.Multiple(() =>
        {
            foreach (Type type in typeof(Constants.Permissions).GetNestedTypes())
            {
                Assert.That(
                    type.GetField(nameof(Constants.Permissions.DocumentType.Prefix))?.GetRawConstantValue(),
                    Is.EqualTo($"Umb.{type.Name}"),
                    type.Name);
            }
        });

    [Test]
    public void Every_Declared_Prefix_Is_One_Of_Umbracos_Permission_Prefixes()
    {
        Assert.That(Declarations, Is.Not.Empty);

        var unknown = Declarations
            .Where(declaration => KnownPrefixes.Contains(declaration.Prefix) is false)
            .Select(declaration => $"{declaration.Controller.FullName} declares \"{declaration.Prefix}\"")
            .ToArray();

        Assert.That(
            unknown,
            Is.Empty,
            $"Prefixes outside {nameof(Constants)}.{nameof(Constants.Permissions)}: {string.Join(", ", unknown)}.");
    }
}
