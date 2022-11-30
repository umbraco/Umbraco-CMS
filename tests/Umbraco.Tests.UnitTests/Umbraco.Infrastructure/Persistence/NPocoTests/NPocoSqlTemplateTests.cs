// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Mappers;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Extensions;
using MapperCollection = NPoco.MapperCollection;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.NPocoTests;

[TestFixture]
public class NPocoSqlTemplateTests
{
    [Test]
    public void SqlTemplates()
    {
        var sqlContext = new SqlContext(
            new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())),
            DatabaseType.SqlServer2012,
            Mock.Of<IPocoDataFactory>());
        var sqlTemplates = new SqlTemplates(sqlContext);

        // this can be used for queries that we know we'll use a *lot* and
        // want to cache as a (static) template for ever, and ever - note
        // that using a MemoryCache would allow us to set a size limit, or
        // something equivalent, to reduce risk of memory explosion
        var sql = sqlTemplates.Get("xxx", s => s
            .SelectAll()
            .From("zbThing1")
            .Where("id=@id", new { id = SqlTemplate.Arg("id") })).Sql(new { id = 1 });

        var sql2 = sqlTemplates.Get("xxx", x => throw new InvalidOperationException("Should be cached.")).Sql(1);

        var sql3 = sqlTemplates.Get("xxx", x => throw new InvalidOperationException("Should be cached."))
            .Sql(new { id = 1 });
    }

    [Test]
    public void SqlTemplateArgs()
    {
        var mappers = new MapperCollection { new NullableDateMapper() };
        var factory = new FluentPocoDataFactory(
            (type, iPocoDataFactory) => new PocoDataBuilder(type, mappers).Init(),
            mappers);

        var sqlContext = new SqlContext(
            new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())),
            DatabaseType.SQLCe,
            factory);
        var sqlTemplates = new SqlTemplates(sqlContext);

        const string sqlBase = "SELECT [zbThing1].[id] AS [Id], [zbThing1].[name] AS [Name] FROM [zbThing1] WHERE ";

        var template = sqlTemplates.Get("sql1", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("value")));

        var sql = template.Sql("foo");
        Assert.AreEqual(sqlBase + "(([zbThing1].[name] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(123);
        Assert.AreEqual(sqlBase + "(([zbThing1].[name] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(123, sql.Arguments[0]);

        template = sqlTemplates.Get("sql2", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("value")));

        sql = template.Sql(new { value = "foo" });
        Assert.AreEqual(sqlBase + "(([zbThing1].[name] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(new { value = 123 });
        Assert.AreEqual(sqlBase + "(([zbThing1].[name] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(123, sql.Arguments[0]);

        Assert.Throws<InvalidOperationException>(() => template.Sql(new { xvalue = 123 }));
        Assert.Throws<InvalidOperationException>(() => template.Sql(new { value = 123, xvalue = 456 }));

        var i = 666;

        template = sqlTemplates.Get("sql3", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Id == i));

        sql = template.Sql("foo");
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(123);
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(123, sql.Arguments[0]);

        // but we cannot name them, because the arg name is the value of "i"
        // so we have to explicitely create the argument
        template = sqlTemplates.Get("sql4", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Id == SqlTemplate.Arg<int>("i")));

        sql = template.Sql("foo");
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(123);
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(123, sql.Arguments[0]);

        // and thanks to a patched visitor, this now works
        sql = template.Sql(new { i = "foo" });
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(new { i = 123 });
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual(123, sql.Arguments[0]);

        Assert.Throws<InvalidOperationException>(() => template.Sql(new { j = 123 }));
        Assert.Throws<InvalidOperationException>(() => template.Sql(new { i = 123, j = 456 }));

        // now with more arguments
        template = sqlTemplates.Get("sql4a", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Id == SqlTemplate.Arg<int>("i") && x.Name == SqlTemplate.Arg<string>("name")));
        sql = template.Sql(0, 1);
        Assert.AreEqual(sqlBase + "((([zbThing1].[id] = @0) AND ([zbThing1].[name] = @1)))", sql.SQL.NoCrLf());
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(0, sql.Arguments[0]);
        Assert.AreEqual(1, sql.Arguments[1]);

        template = sqlTemplates.Get("sql4b", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .Where<Thing1Dto>(x => x.Id == SqlTemplate.Arg<int>("i"))
            .Where<Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("name")));
        sql = template.Sql(0, 1);
        Assert.AreEqual(sqlBase + "(([zbThing1].[id] = @0)) AND (([zbThing1].[name] = @1))", sql.SQL.NoCrLf());
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual(0, sql.Arguments[0]);
        Assert.AreEqual(1, sql.Arguments[1]);

        // works, magic
        template = sqlTemplates.Get("sql5", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .WhereIn<Thing1Dto>(x => x.Id, SqlTemplate.ArgIn<int>("i")));

        sql = template.Sql("foo");
        Assert.AreEqual(sqlBase + "([zbThing1].[id] IN (@0))", sql.SQL.NoCrLf());
        Assert.AreEqual(1, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);

        sql = template.Sql(new[] { 1, 2, 3 });
        Assert.AreEqual(sqlBase + "([zbThing1].[id] IN (@0,@1,@2))", sql.SQL.NoCrLf());
        Assert.AreEqual(3, sql.Arguments.Length);
        Assert.AreEqual(1, sql.Arguments[0]);
        Assert.AreEqual(2, sql.Arguments[1]);
        Assert.AreEqual(3, sql.Arguments[2]);

        template = sqlTemplates.Get("sql5a", s => s.Select<Thing1Dto>().From<Thing1Dto>()
            .WhereIn<Thing1Dto>(x => x.Id, SqlTemplate.ArgIn<int>("i"))
            .Where<Thing1Dto>(x => x.Name == SqlTemplate.Arg<string>("name")));

        sql = template.Sql("foo", "bar");
        Assert.AreEqual(sqlBase + "([zbThing1].[id] IN (@0)) AND (([zbThing1].[name] = @1))", sql.SQL.NoCrLf());
        Assert.AreEqual(2, sql.Arguments.Length);
        Assert.AreEqual("foo", sql.Arguments[0]);
        Assert.AreEqual("bar", sql.Arguments[1]);

        sql = template.Sql(new[] { 1, 2, 3 }, "bar");
        Assert.AreEqual(sqlBase + "([zbThing1].[id] IN (@0,@1,@2)) AND (([zbThing1].[name] = @3))", sql.SQL.NoCrLf());
        Assert.AreEqual(4, sql.Arguments.Length);
        Assert.AreEqual(1, sql.Arguments[0]);
        Assert.AreEqual(2, sql.Arguments[1]);
        Assert.AreEqual(3, sql.Arguments[2]);
        Assert.AreEqual("bar", sql.Arguments[3]);

        // note however that using WhereIn in a template means that the SQL is going
        // to be parsed and arguments are going to be expanded etc - it *may* be a better
        // idea to just add the WhereIn to a templated, immutable SQL template

        // more fun...
        template = sqlTemplates.Get("sql6", s => s.Select<Thing1Dto>().From<Thing1Dto>()

            // do NOT do this, this is NOT a visited expression
            //// .Append(" AND whatever=@0", SqlTemplate.Arg<string>("j"))

            // does not work anymore - due to proper TemplateArg
            //// instead, directly name the argument
            ////.Append("AND whatever=@0", "j")
            ////.Append("AND whatever=@0", "k")

            // instead, explicitely create the argument
            .Append("AND whatever=@0", SqlTemplate.Arg("j"))
            .Append("AND whatever=@0", SqlTemplate.Arg("k")));

        sql = template.Sql(new { j = new[] { 1, 2, 3 }, k = "oops" });
        Assert.AreEqual(sqlBase.TrimEnd("WHERE ") + "AND whatever=@0,@1,@2 AND whatever=@3", sql.SQL.NoCrLf());
        Assert.AreEqual(4, sql.Arguments.Length);
        Assert.AreEqual(1, sql.Arguments[0]);
        Assert.AreEqual(2, sql.Arguments[1]);
        Assert.AreEqual(3, sql.Arguments[2]);
        Assert.AreEqual("oops", sql.Arguments[3]);
    }

    [TableName("zbThing1")]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    public class Thing1Dto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("name")]
        public string Name { get; set; }
    }
}
