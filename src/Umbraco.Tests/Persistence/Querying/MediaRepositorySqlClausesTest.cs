using System;
using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class MediaRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var nodeObjectTypeId = Constants.ObjectTypes.Media;

            var expected = new Sql();
            expected.Select("*")
                .From($"[{Constants.DatabaseSchema.Tables.ContentVersion}]")
                .InnerJoin($"[{Constants.DatabaseSchema.Tables.Content}]").On($"[{Constants.DatabaseSchema.Tables.ContentVersion}].[nodeId] = [{Constants.DatabaseSchema.Tables.Content}].[nodeId]")
                .InnerJoin("[umbracoNode]").On($"[{Constants.DatabaseSchema.Tables.Content}].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("b796f64c-1f99-4ffb-b886-4bf4bc011a9c"));

            var sql = Sql();
            sql.SelectAll()
                .From<ContentVersionDto>()
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == nodeObjectTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Debug.Print(sql.SQL);
        }
    }
}
