using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class MediaRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectTypeId = new Guid(Constants.ObjectTypes.Media);

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContentVersion]")
                .InnerJoin("[cmsContent]").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = @0", new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"));

            var sql = new Sql();
            sql.Select("*")
                .From<ContentVersionDto>(SqlSyntax)
                .InnerJoin<ContentDto>(SqlSyntax)
                .On<ContentVersionDto, ContentDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(SqlSyntax, x => x.NodeObjectType == NodeObjectTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Console.WriteLine(sql.SQL);
        }
    }
}