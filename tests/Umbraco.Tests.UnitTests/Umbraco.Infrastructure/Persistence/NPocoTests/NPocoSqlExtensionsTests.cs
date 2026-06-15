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

[TestFixture]
public class NPocoSqlExtensionsTests : BaseUsingSqlSyntax
{
    [Test]
    public void WhereTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == null);
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] is null))"),
            sql.SQL);

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == 123);
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))"),
            sql.SQL);

        var id = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == id);
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))"),
            sql.SQL);

        int? nid = 123;

        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => x.LanguageId == nid);
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE (([umbracoPropertyData].[languageId] = @0))"),
            sql.SQL);

        // but the above comparison fails if @0 is null
        // what we want is something similar to:
        sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<PropertyDataDto>()
            .Where<PropertyDataDto>(x => (nid == null && x.LanguageId == null) || (nid != null && x.LanguageId == nid));
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE ((((@0 is null) AND ([umbracoPropertyData].[languageId] is null)) OR ((@1 is not null) AND ([umbracoPropertyData].[languageId] = @2))))"),
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
        Assert.That(
            sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoPropertyData]\nWHERE ((COALESCE([umbracoPropertyData].[languageId],@0) = COALESCE(@1,@0)))"),
            sql.SQL);
    }

    [Test]
    public void SqlNullableEqualsTest()
    {
        int? a, b;
        a = b = null;
        Assert.That(a.SqlNullableEquals(b, -1), Is.True);
        b = 2;
        Assert.That(a.SqlNullableEquals(b, -1), Is.False);
        a = 2;
        Assert.That(a.SqlNullableEquals(b, -1), Is.True);
        b = null;
        Assert.That(a.SqlNullableEquals(b, -1), Is.False);
    }

    [Test]
    public void WhereInValueFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
        Assert.That(sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))"));
    }

    [Test]
    public void WhereInObjectFieldTest()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });
        Assert.That(sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))"));
    }

    [Test]
    public void WhereLike_Uses_Parameterized_Query()
    {
        var sql = new Sql<ISqlContext>(SqlContext)
            .Select("*")
            .From<NodeDto>()
            .WhereLike<NodeDto>(x => x.Text, "%test%");

        // Verify SQL uses parameterized query (LIKE @0) instead of inline value.
        Assert.That(sql.SQL, Is.EqualTo("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] LIKE @0)"));

        // Verify the argument is passed correctly.
        Assert.That(sql.Arguments.Length, Is.EqualTo(1));
        Assert.That(sql.Arguments[0], Is.EqualTo("%test%"));
    }

    [Test]
    public void SelectTests()
    {
        // select the whole DTO
        var sql = Sql()
            .Select<Dto1>()
            .From<Dto1>();
        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value] FROM [dto1]"));

        // select only 1 field
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .From<Dto1>();
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] FROM [dto1]"));

        // select 2 fields
        sql = Sql()
            .Select<Dto1>(x => x.Id, x => x.Name)
            .From<Dto1>();
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name] FROM [dto1]"));

        // select the whole DTO and a referenced DTO
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf()),
            sql.SQL);

        // select the whole DTO and nested referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2, r1 => r1.Select(x => x.Dto3)))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id)
            .InnerJoin<Dto3>().On<Dto2, Dto3>(left => left.Id, right => right.Dto2Id);
        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2__Id], [dto2].[dto1id] AS [Dto2__Dto1Id], [dto2].[name] AS [Dto2__Name]
, [dto3].[id] AS [Dto2__Dto3__Id], [dto3].[dto2id] AS [Dto2__Dto3__Dto2Id], [dto3].[name] AS [Dto2__Dto3__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]
INNER JOIN [dto3] ON [dto2].[id] = [dto3].[dto2id]".NoCrLf()));

        // select the whole DTO and referenced DTOs
        sql = Sql()
            .Select<Dto1>(r => r.Select(x => x.Dto2s))
            .From<Dto1>()
            .InnerJoin<Dto2>().On<Dto1, Dto2>(left => left.Id, right => right.Dto1Id);
        Assert.That(
            sql.SQL.NoCrLf(), Is.EqualTo(@"SELECT [dto1].[id] AS [Id], [dto1].[name] AS [Name], [dto1].[value] AS [Value]
, [dto2].[id] AS [Dto2s__Id], [dto2].[dto1id] AS [Dto2s__Dto1Id], [dto2].[name] AS [Dto2s__Name]
FROM [dto1]
INNER JOIN [dto2] ON [dto1].[id] = [dto2].[dto1id]".NoCrLf()));
    }

    [Test]
    public void SelectAliasTests()
    {
        // and select - not good
        var sql = Sql()
            .Select<Dto1>(x => x.Id)
            .Select<Dto2>(x => x.Id);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] SELECT [dto2].[id] AS [Id]".NoCrLf()));

        // and select - good
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => x.Id);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf()));

        // and select + alias
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => Alias(x.Id, "id2"));
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [id2]".NoCrLf()));
    }

    [Test]
    public void AndSelectWithAliasTests()
    {
        // without withAlias parameter - alias is included by default
        var sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(x => x.Id);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf()));

        // withAlias: true - explicit, same result as default
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: true, x => x.Id);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id] AS [Id]".NoCrLf()));

        // withAlias: false - column name only, no alias
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: false, x => x.Id);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id]".NoCrLf()));

        // withAlias: false with multiple fields - none have aliases
        sql = Sql()
            .Select<Dto1>(x => x.Id)
            .AndSelect<Dto2>(withAlias: false, x => x.Id, x => x.Name);
        Assert.That(sql.SQL.NoCrLf(), Is.EqualTo("SELECT [dto1].[id] AS [Id] , [dto2].[id], [dto2].[name]".NoCrLf()));
    }

    [Test]
    public void UpdateTests()
    {
        var sql = Sql()
            .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
            .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias");
    }

    [TableName("dto1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto1
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("value")]
        public int Value { get; set; }

        [Reference]
        public Dto2 Dto2 { get; set; }

        [Reference]
        public List<Dto2> Dto2s { get; set; }
    }

    [TableName("dto2")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto2
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dto1id")]
        public int Dto1Id { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Reference]
        public Dto3 Dto3 { get; set; }
    }

    [TableName("dto3")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Dto3
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dto2id")]
        public int Dto2Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
