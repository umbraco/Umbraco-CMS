using System;
using NUnit.Framework;
using Umbraco.Core;
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
            var NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType);

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = @0", new Guid("a2cb7800-f571-4787-9638-bc48539a0efb"))
                .Where("[cmsDocumentType].[IsDefault] = @0", true);

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentTypeDto>(SqlSyntax)
                .RightJoin<ContentTypeDto>(SqlSyntax)
                .On<ContentTypeDto, DocumentTypeDto>(SqlSyntax, left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentTypeDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(SqlSyntax, x => x.NodeObjectType == NodeObjectType)
                .Where<DocumentTypeDto>(SqlSyntax, x => x.IsDefault == true);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_Base_Where_Clause()
        {
            var NodeObjectType = new Guid(Constants.ObjectTypes.DocumentType);

            var expected = new Sql();
            expected.Select("*")
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("[umbracoNode].[nodeObjectType] = @0", new Guid("a2cb7800-f571-4787-9638-bc48539a0efb"))
                .Where("[cmsDocumentType].[IsDefault] = @0", true)
                .Where("[umbracoNode].[id] = @0", 1050);

            var sql = new Sql();
            sql.Select("*")
                .From<DocumentTypeDto>(SqlSyntax)
                .RightJoin<ContentTypeDto>(SqlSyntax)
                .On<ContentTypeDto, DocumentTypeDto>(SqlSyntax, left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>(SqlSyntax)
                .On<ContentTypeDto, NodeDto>(SqlSyntax, left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(SqlSyntax, x => x.NodeObjectType == NodeObjectType)
                .Where<DocumentTypeDto>(SqlSyntax, x => x.IsDefault)
                .Where<NodeDto>(SqlSyntax, x => x.NodeId == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_PerformQuery_Clause()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsPropertyTypeGroup]")
                .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
                .InnerJoin("[cmsDataType]").On("[cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]");

            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeGroupDto>(SqlSyntax)
               .RightJoin<PropertyTypeDto>(SqlSyntax)
               .On<PropertyTypeGroupDto, PropertyTypeDto>(SqlSyntax, left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>(SqlSyntax)
               .On<PropertyTypeDto, DataTypeDto>(SqlSyntax, left => left.DataTypeId, right => right.DataTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_AllowedContentTypeIds_Clause()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsContentTypeAllowedContentType]")
                .Where("[cmsContentTypeAllowedContentType].[Id] = @0", 1050);

            var sql = new Sql();
            sql.Select("*")
               .From<ContentTypeAllowedContentTypeDto>(SqlSyntax)
               .Where<ContentTypeAllowedContentTypeDto>(SqlSyntax, x => x.Id == 1050);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
            for (int i = 0; i < expected.Arguments.Length; i++)
            {
                Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
            }

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_PropertyGroupCollection_Clause()
        {
            var expected = new Sql();
            expected.Select("*")
                .From("[cmsPropertyTypeGroup]")
                .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
                .InnerJoin("[cmsDataType]").On("[cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]")
                .Where("[cmsPropertyType].[contentTypeId] = @0", 1050);

            var sql = new Sql();
            sql.Select("*")
               .From<PropertyTypeGroupDto>(SqlSyntax)
               .RightJoin<PropertyTypeDto>(SqlSyntax)
               .On<PropertyTypeGroupDto, PropertyTypeDto>(SqlSyntax, left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>(SqlSyntax)
               .On<PropertyTypeDto, DataTypeDto>(SqlSyntax, left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeDto>(SqlSyntax, x => x.ContentTypeId == 1050);

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