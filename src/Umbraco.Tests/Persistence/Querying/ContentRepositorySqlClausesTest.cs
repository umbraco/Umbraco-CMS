using System;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ContentRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var NodeObjectType = new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[cmsContent]").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = 'c66ba18e-eaf3-4cff-8a22-41b16d66a972'");

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_Clause()
        {
            var NodeObjectType = new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[cmsContent]").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = 'c66ba18e-eaf3-4cff-8a22-41b16d66a972'")
                .Where("[umbracoNode].[id] = 1050");

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<NodeDto>(x => x.NodeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_With_Version_Clause()
        {
            var NodeObjectType = new Guid("C66BA18E-EAF3-4CFF-8A22-41B16D66A972");
            var versionId = new Guid("2b543516-a944-4ee6-88c6-8813da7aaa07");

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocument]")
                .InnerJoin("[cmsContentVersion]").On("[cmsDocument].[versionId] = [cmsContentVersion].[VersionId]")
                .InnerJoin("[cmsContent]").On("[cmsContentVersion].[ContentId] = [cmsContent].[nodeId]")
                .InnerJoin("[umbracoNode]").On("[cmsContent].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = 'c66ba18e-eaf3-4cff-8a22-41b16d66a972'")
                .Where("[umbracoNode].[id] = 1050")
                .Where("[cmsContentVersion].[VersionId] = '2b543516-a944-4ee6-88c6-8813da7aaa07'")
                .OrderBy("[cmsContentVersion].[VersionDate] DESC");

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentDto>()
                .InnerJoin<ContentVersionDto>()
                .On<DocumentDto, ContentVersionDto>(left => left.VersionId, right => right.VersionId)
                .InnerJoin<ContentDto>()
                .On<ContentVersionDto, ContentDto>(left => left.NodeId, right => right.NodeId)
                .InnerJoin<NodeDto>()
                .On<ContentDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == NodeObjectType)
                .Where<NodeDto>(x => x.NodeId == 1050)
                .Where<ContentVersionDto>(x => x.VersionId == versionId)
                .OrderByDescending<ContentVersionDto>(x => x.VersionDate);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }
    }
}