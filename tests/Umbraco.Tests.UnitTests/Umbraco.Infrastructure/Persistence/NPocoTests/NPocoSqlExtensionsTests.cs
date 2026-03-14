// Copyright (c) Umbraco.
// See LICENSE for more details.

using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Persistence.SqlExtensionsStatics;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.NPocoTests;

/// <summary>
/// Contains unit tests for the NPoco SQL extension methods in the Umbraco CMS infrastructure.
/// </summary>
[TestFixture]
public class NPocoSqlExtensionsTests : BaseUsingSqlSyntax
{
    /// <summary>
    /// Verifies that NPoco generates correct SQL for various where clause scenarios involving nullable and non-nullable values.
    /// Tests include comparisons with null, constants, variables, and the use of SqlNullableEquals, ensuring correct SQL output for each case.
    /// </summary>
    [Test]
    public void WhereTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == null);
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] is null))",
            sql.SQL,
            sql.SQL);

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == 123);
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))",
            sql.SQL,
            sql.SQL);

        var id = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == id);
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))",
            sql.SQL,
            sql.SQL);

        int? nid = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == nid);
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))",
            sql.SQL,
            sql.SQL);

        // but the above comparison fails if @0 is null
        // what we want is something similar to:
        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => (nid == null && x.LanguageId == null) || (nid != null && x.LanguageId == nid));
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE ((((@0 is null) AND ([umbracoPropertyData].[languageId] is null)) OR ((@1 is not null) AND ([umbracoPropertyData].[languageId] = @2))))",
            sql.SQL,
            sql.SQL);

        // new SqlNullableEquals method does it automatically
        // 'course it would be nicer if '==' could do it
        // see note in ExpressionVisitorBase for SqlNullableEquals

        // sql = new Sql<ISqlContext>(SqlContext)
        //    .Select("*")
        //    .From<PropertyDataDto>()
        //    .Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(nid));
        // Assert.AreEqual("SELECT *\nFROM [umbracoPropertyData]\nWHERE ((((@0 is null) AND ([umbracoPropertyData].[languageId] is null)) OR ((@0 is not null) AND ([umbracoPropertyData].[languageId] = @0))))", sql.SQL, sql.SQL);

        // but, the expression above fails with SQL CE, 'specified argument for the function is not valid' in 'isnull' function
        // so... compare with fallback values
        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId.SqlNullableEquals(nid, -1));
        Assert.AreEqual(
            "SELECT *\nFROM [umbracoPropertyData]\nWHERE ((COALESCE([umbracoPropertyData].[languageId],@0) = COALESCE(@1,@0)))",
            sql.SQL,
            sql.SQL);
    }

    /// <summary>
    /// Tests the <c>SqlNullableEquals</c> extension method for nullable integers.
    /// Verifies that the method correctly determines equality when both values are null, when one is null, and when both have values.
    /// </summary>
    [Test]
    public void SqlNullableEqualsTest()
    {
        int? a, b;
        a = b = null;
        Assert.IsTrue(a.SqlNullableEquals(b, -1));
        b = 2;
        Assert.IsFalse(a.SqlNullableEquals(b, -1));
        a = 2;
        Assert.IsTrue(a.SqlNullableEquals(b, -1));
        b = null;
        Assert.IsFalse(a.SqlNullableEquals(b, -1));
    }

    /// <summary>
    /// Tests the WhereIn extension method for SQL queries with value fields.
    /// </summary>
    [Test]
    public void WhereInValueFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
        Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))", sql.SQL);
    }

    /// <summary>
    /// Tests the WhereIn extension method for object fields in NPoco SQL queries.
    /// </summary>
    [Test]
    public void WhereInObjectFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });
        Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))", sql.SQL);
    }

    /// <summary>
    /// Tests that the WhereLike extension method generates a parameterized SQL LIKE query.
    /// </summary>
    [Test]
    public void WhereLike_Uses_Parameterized_Query()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereLike<NodeDto>(x => x.Text, "%test%");

        // Verify SQL uses parameterized query (LIKE @0) instead of inline value.
        Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] LIKE @0)", sql.SQL);

        // Verify the argument is passed correctly.
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("%test%", sql.Arguments[0]);
    }

    /// <summary>
    /// Tests various Select methods of NPoco SQL extensions to ensure correct SQL generation.
    /// </summary>
    [Test]
    public void SelectTests()
    {
        // select the whole DTO
        var sql = Sql()
            .Select<Dto1>()
            .From<Dto1>();
        Assert.AreEqual(
            "SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value] FROM [dto1]",
            sql.SQL.NoCrLf());

        // select only 1 field
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .From<Dto1>();
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] FROM [dto1]", sql.SQL.NoCrLf());

        // select 2 fields
        sql = Sql()
            .Select<Dto1>(x => x.Id, x => x.Name)
            .From<Dto1>();
        Assert.AreEqual("SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name] FROM [dto1]", sql.SQL.NoCrLf());

        // select the whole DTO and a referenced DTO
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.AreEqual(
            @"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf(),
            sql.SQL.NoCrLf(),
            sql.SQL);

        // select the whole DTO and nested referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2, r1 => r1.Select(x => x.Dto3)))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id)
            .InnerJoin<Dto3>().On<Dto2, Dto3>(left => left.Id, right => right.Dto2Id);
        Assert.AreEqual(
            @"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
, [dto3].[id] AS [Dto2__Dto3__Id], [dto3].[dto2id] AS [Dto2__Dto3__Dto2Id], [dto3].[name] AS [Dto2__Dto3__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]
INNER JOIN [dto3] ON [dto2].[id] = [dto3].[dto2id]".NoCrLf(),
            sql.SQL.NoCrLf());

        // select the whole DTO and referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2s))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.AreEqual(
            @"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2s__Id], [dto2].[dto1id] AS [Dto2s__Dto1Id], [dto2].[name] AS [Dto2s__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf(),
            sql.SQL.NoCrLf());
    }

    /// <summary>
    /// Verifies the SQL generation behavior of the Select and AndSelect extension methods
    /// for NPoco, both with and without column aliases. Ensures that multiple selects and
    /// aliasing produce the expected SQL output.
    /// </summary>
    [Test]
    public void SelectAliasTests()
    {
        // and select - not good
        var sql = Sql()
            .Select<Dto1>(x => x.Id)
            .Select<Dto2>(x => x.Id);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] SELECT [dto2].[id] AS [Id]".NoCrLf(), sql.SQL.NoCrLf());

        // and select - good
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => x.Id);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf(), sql.SQL.NoCrLf());

        // and select + alias
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => Alias(x.Id, "id2"));
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [id2]".NoCrLf(), sql.SQL.NoCrLf());
    }

    /// <summary>
    /// Tests the <c>AndSelect</c> extension method for SQL query generation with and without column aliasing.
    /// Verifies that:
    /// <list type="bullet">
    /// <item>By default and when <c>withAlias</c> is true, column aliases are included in the generated SQL.</item>
    /// <item>When <c>withAlias</c> is false, columns are selected without aliases, including when selecting multiple fields.</item>
    /// </list>
    /// </summary>
    [Test]
    public void AndSelectWithAliasTests()
    {
        // without withAlias parameter - alias is included by default
        var sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => x.Id);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf(), sql.SQL.NoCrLf());

        // withAlias: true - explicit, same result as default
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: true, x => x.Id);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf(), sql.SQL.NoCrLf());

        // withAlias: false - column name only, no alias
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: false, x => x.Id);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id]".NoCrLf(), sql.SQL.NoCrLf());

        // withAlias: false with multiple fields - none have aliases
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: false, x => x.Id, x => x.Name);
        Assert.AreEqual("SELECT [dto1].[id] AS [Id] , [dto2].[id], [dto2].[name]".NoCrLf(), sql.SQL.NoCrLf());
    }

    /// <summary>
    /// Verifies that the Update extension method for NPoco SQL queries correctly constructs an update statement for the DataTypeDto entity.
    /// </summary>
    [Test]
    public void UpdateTests()
    {
        var sql = Sql()
            .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
            .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias");
    }

    /// <summary>
    /// Represents a simple data transfer object used in NPoco SQL extension tests.
    /// </summary>
    [TableName("dto1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto1
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [Column("value")]
        public int Value { get; set; }

    /// <summary>
    /// Represents the Dto2 nested class within Dto1.
    /// </summary>
        [Reference]
        public Dto2 Dto2 { get; set; }

        /// <summary>
        /// Gets or sets the list of Dto2 objects associated with this Dto1.
        /// </summary>
        [Reference]
        public List<Dto2> Dto2s { get; set; }
    }

    /// <summary>
    /// Data transfer object (DTO) used in NPoco SQL extension unit tests.
    /// </summary>
    [TableName("dto2")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto2
    {
    /// <summary>
    /// Gets or sets the identifier.
    /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the Dto1 identifier.
        /// </summary>
        [Column("dto1id")]
        public int Dto1Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }

    /// <summary>
    /// Gets or sets the Dto3 reference.
    /// </summary>
        [Reference]
        public Dto3 Dto3 { get; set; }
    }

    /// <summary>
    /// Data transfer object (DTO) used specifically for testing NPoco SQL extension methods in unit tests.
    /// </summary>
    [TableName("dto3")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto3
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        [Column("id")]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the identifier for Dto2, mapped to the 'dto2id' column in the database.
        /// </summary>
        [Column("dto2id")]
        public int Dto2Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Column("name")]
        public string Name { get; set; }
    }
}
