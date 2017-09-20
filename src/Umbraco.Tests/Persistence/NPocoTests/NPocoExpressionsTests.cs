using System;
using NPoco;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.NPocoTests
{
    [TestFixture]
    public class NPocoExpressionsTests : BaseUsingSqlCeSyntax
    {
        [Test]
        public void WhereInValueFieldTest()
        {
            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.NodeId, new[] { 1, 2, 3 });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[id] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void WhereInObjectFieldTest()
        {
            // this test used to fail because x => x.Text was evaluated as a lambda
            // and returned "[umbracoNode].[text] = @0"... had to fix WhereIn.

            var sql = new Sql<SqlContext>(SqlContext)
                .Select("*")
                .From<NodeDto>()
                .WhereIn<NodeDto>(x => x.Text, new[] { "a", "b", "c" });
            Assert.AreEqual("SELECT *\nFROM [umbracoNode]\nWHERE ([umbracoNode].[text] IN (@0,@1,@2))", sql.SQL);
        }

        [Test]
        public void SelectDtoTest()
        {
            var sql = Sql()
                .Select<NodeDto>()
                .From<NodeDto>();
            Assert.AreEqual("SELECT [umbracoNode].[id] AS [NodeId], [umbracoNode].[trashed] AS [Trashed], [umbracoNode].[parentID] AS [ParentId], [umbracoNode].[nodeUser] AS [UserId], [umbracoNode].[level] AS [Level], [umbracoNode].[path] AS [Path], [umbracoNode].[sortOrder] AS [SortOrder], [umbracoNode].[uniqueID] AS [UniqueId], [umbracoNode].[text] AS [Text], [umbracoNode].[nodeObjectType] AS [NodeObjectType], [umbracoNode].[createDate] AS [CreateDate] FROM [umbracoNode]", sql.SQL.NoCrLf());
        }

        [Test]
        public void SelectDtoFieldTest()
        {
            var sql = Sql()
                .Select<NodeDto>(x => x.NodeId)
                .From<NodeDto>();
            Assert.AreEqual("SELECT [umbracoNode].[id] FROM [umbracoNode]", sql.SQL.NoCrLf());

            sql = Sql()
                .Select<NodeDto>(x => x.NodeId, x => x.UniqueId)
                .From<NodeDto>();
            Assert.AreEqual("SELECT [umbracoNode].[id], [umbracoNode].[uniqueID] FROM [umbracoNode]", sql.SQL.NoCrLf());
        }

        [Test]
        public void SelectDtoRefTest()
        {
            var sql = Sql()
                .Select<NodeDto>(r => r.Select<ContentDto>())
                .From<NodeDto>();
            Console.WriteLine(sql.SQL);
            Assert.AreEqual("SELECT [umbracoNode].[id] AS [NodeId], [umbracoNode].[trashed] AS [Trashed], [umbracoNode].[parentID] AS [ParentId], [umbracoNode].[nodeUser] AS [UserId], [umbracoNode].[level] AS [Level], [umbracoNode].[path] AS [Path], [umbracoNode].[sortOrder] AS [SortOrder], [umbracoNode].[uniqueID] AS [UniqueId], [umbracoNode].[text] AS [Text], [umbracoNode].[nodeObjectType] AS [NodeObjectType], [umbracoNode].[createDate] AS [CreateDate] , [cmsContent].[pk] AS [ContentDto__PrimaryKey], [cmsContent].[nodeId] AS [ContentDto__NodeId], [cmsContent].[contentType] AS [ContentDto__ContentTypeId] FROM [umbracoNode]", sql.SQL.NoCrLf());
        }
    }
}
