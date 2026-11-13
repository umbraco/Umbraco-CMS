using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Persistence;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
internal sealed class MemberSchemaTests : UmbracoIntegrationTest
{
    private const string TableName = "cmsMember";
    private static readonly string[] _columnNames = ["Email", "LoginName", "Password"];

    [Test]
    public void Text_Columns_Default_To_An_Empty_String()
    {
        Dictionary<string, string> defaults = GetColumnDefaults();

        Assert.Multiple(() =>
        {
            foreach (var columnName in _columnNames)
            {
                Assert.That(defaults.TryGetValue(columnName, out var definition), Is.True, $"{columnName} has no default");
                Assert.That(definition, Is.EqualTo("''"), columnName);
            }
        });
    }

    // Queried directly rather than through the syntax provider, so the assertion doesn't share a code path with
    // the schema creation it is verifying.
    private Dictionary<string, string> GetColumnDefaults()
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        IUmbracoDatabase database = scope.Database;

        List<ColumnDefault> defaults = BaseTestDatabase.IsSqlite()
            ? database.Fetch<ColumnDefault>(
                "SELECT [name] AS [ColumnName], [dflt_value] AS [Definition] FROM pragma_table_info(@0) WHERE [dflt_value] IS NOT NULL",
                TableName)
            : database.Fetch<ColumnDefault>(
                @"SELECT col.[name] AS [ColumnName], dc.[definition] AS [Definition]
FROM sys.default_constraints dc
JOIN sys.columns col ON col.object_id = dc.parent_object_id AND col.column_id = dc.parent_column_id
WHERE dc.parent_object_id = OBJECT_ID(@0)",
                TableName);

        // SQL Server wraps the stored expression in brackets, SQLite doesn't.
        return defaults.ToDictionary(
            x => x.ColumnName,
            x => x.Definition.StartsWith('(') && x.Definition.EndsWith(')') ? x.Definition[1..^1] : x.Definition);
    }

    private sealed class ColumnDefault
    {
        public string ColumnName { get; set; } = null!;

        public string Definition { get; set; } = null!;
    }
}
