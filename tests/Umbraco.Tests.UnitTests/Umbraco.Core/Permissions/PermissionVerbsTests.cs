using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Permissions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Permissions;

[TestFixture]
internal sealed class PermissionVerbsTests
{
    private const string ResourceField = "Resource";

    // Every verb constant of each entity type, with the type's resource and the operation of the same name.
    private static IEnumerable<TestCaseData> PermissionConstants()
        => typeof(Constants.Permissions.Entities)
            .GetNestedTypes()
            .SelectMany(type => type
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && OperationNamed(field.Name) is not null)
                .Select(field => new TestCaseData(
                        (string)type.GetField(ResourceField)!.GetRawConstantValue()!,
                        OperationNamed(field.Name),
                        (string)field.GetRawConstantValue()!)
                    .SetName($"{type.Name}.{field.Name}")));

    private static string? OperationNamed(string name)
        => (string?)typeof(Constants.Permissions.Operations).GetField(name)?.GetRawConstantValue();

    [TestCaseSource(nameof(PermissionConstants))]
    public void Every_Permission_Constant_Is_The_Built_Verb(string resource, string operation, string permission)
        => Assert.That(PermissionVerbs.For(resource, operation), Is.EqualTo(permission));

    [TestCase(Constants.Permissions.Operations.Create, "My.Package.Widget.Create")]
    [TestCase(Constants.Permissions.Operations.Delete, "My.Package.Widget.Delete")]
    [TestCase("Publish", "My.Package.Widget.Publish")]
    public void The_Verb_Is_The_Resource_And_The_Operation(string operation, string expected)
        => Assert.That(PermissionVerbs.For("My.Package.Widget", operation), Is.EqualTo(expected));

    [TestCase("Umb.DocumentType.")]
    [TestCase("Umb.DocumentType..")]
    public void A_Trailing_Dot_On_The_Resource_Is_Ignored(string resource)
        => Assert.That(
            PermissionVerbs.For(resource, Constants.Permissions.Operations.Create),
            Is.EqualTo(Constants.Permissions.Entities.DocumentType.Create));

    [TestCase("")]
    [TestCase(" ")]
    [TestCase(".")]
    public void An_Empty_Resource_Is_Rejected(string resource)
        => Assert.Throws<ArgumentException>(() => PermissionVerbs.For(resource, Constants.Permissions.Operations.Create));

    [TestCase("")]
    [TestCase(" ")]
    public void An_Empty_Operation_Is_Rejected(string operation)
        => Assert.Throws<ArgumentException>(
            () => PermissionVerbs.For(Constants.Permissions.Entities.DocumentType.Resource, operation));
}
