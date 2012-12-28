using System;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ContentTypeRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectType = new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("nodeObjectType = 'a2cb7800-f571-4787-9638-bc48539a0efb'")
                .Where("IsDefault = 'True'");

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentTypeDto>()
                .RightJoin<ContentTypeDto>()
                .On<ContentTypeDto, DocumentTypeDto>(left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<DocumentTypeDto>(x => x.IsDefault == true);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_Clause()
        {
            var NodeObjectType = new Guid("A2CB7800-F571-4787-9638-BC48539A0EFB");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("nodeObjectType = 'a2cb7800-f571-4787-9638-bc48539a0efb'")
                .Where("IsDefault = 'True'")
                .Where("id = 1050");

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentTypeDto>()
                .RightJoin<ContentTypeDto>()
                .On<ContentTypeDto, DocumentTypeDto>(left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<DocumentTypeDto>(x => x.IsDefault == true)
                .Where<NodeDto>(x => x.NodeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }
    }
}