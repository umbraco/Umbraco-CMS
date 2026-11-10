using System.Reflection;
using NUnit.Framework;
using Umbraco.Cms.Api.Management.SchemaLockdown;
using Umbraco.Cms.Core;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.SchemaLockdown;

[TestFixture]
public class SchemaPermissionsTests
{
    // Every inner class of Constants.Permissions, paired with the entity type its name stands for.
    private static IEnumerable<TestCaseData> PermissionConstants()
        => typeof(Constants.Permissions)
            .GetNestedTypes(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(entityType => entityType
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral)
                .Select(field => new TestCaseData(
                        UdiEntityTypeFor(entityType.Name),
                        field.Name,
                        (string)field.GetRawConstantValue()!)
                    .SetName($"{entityType.Name}.{field.Name}")));

    private static string UdiEntityTypeFor(string constantsClassName)
        => (string)typeof(Constants.UdiEntityType).GetField(constantsClassName)!.GetRawConstantValue()!;

    [TestCaseSource(nameof(PermissionConstants))]
    public void Every_Permission_Constant_Is_The_Derived_Verb(string entityType, string operation, string permission)
        => Assert.That(SchemaPermissions.For(entityType, Enum.Parse<SchemaOperation>(operation)), Is.EqualTo(permission));

    [TestCase("document-type", "Create", "Umb.DocumentType.Create")]
    [TestCase("Document-Type", "Update", "Umb.DocumentType.Update")]
    [TestCase("DICTIONARY-ITEM", "Delete", "Umb.DictionaryItem.Delete")]
    [TestCase("my-package-widget", "Create", "Umb.MyPackageWidget.Create")]
    public void The_Verb_Is_The_Pascal_Cased_Entity_Type_And_The_Operation(string entityType, string operation, string expected)
        => Assert.That(SchemaPermissions.For(entityType, Enum.Parse<SchemaOperation>(operation)), Is.EqualTo(expected));

    [Test]
    public void An_Undefined_Operation_Has_No_Verb()
        => Assert.Throws<ArgumentOutOfRangeException>(
            () => SchemaPermissions.For(Constants.UdiEntityType.DocumentType, (SchemaOperation)0));
}
