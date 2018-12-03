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
    public class MediaTypeRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectTypeId = Constants.ObjectTypes.MediaType;

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContentType]")
                .InnerJoin("[umbracoNode]").On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("4ea4382b-2f5a-4c2b-9587-ae9b3cf3602e"));

            var sql = Sql();
            sql.SelectAll()
                .From<ContentTypeDto>()
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectTypeId);

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
