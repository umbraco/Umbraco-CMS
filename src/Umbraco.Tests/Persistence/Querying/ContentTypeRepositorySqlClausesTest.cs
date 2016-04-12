using System;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence.Querying
{
    [TestFixture]
    public class ContentTypeRepositorySqlClausesTest : BaseUsingSqlCeSyntax
    {
        [Test]
        public void Can_Verify_Base_Clause()
        {
            var nodeObjectType = new Guid(Constants.ObjectTypes.DocumentType);

            var expected = Sql();
            expected.Select("*")
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("a2cb7800-f571-4787-9638-bc48539a0efb"))
                .Where("([cmsDocumentType].[IsDefault] = @0)", true);

            var sql = Sql();
            sql.SelectAll()
                .From<ContentTypeTemplateDto>()
                .RightJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentTypeTemplateDto>(left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == nodeObjectType)
                .Where<ContentTypeTemplateDto>(x => x.IsDefault == true);

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
            var nodeObjectType = new Guid(Constants.ObjectTypes.DocumentType);

            var expected = Sql();
            expected.SelectAll()
                .From("[cmsDocumentType]")
                .RightJoin("[cmsContentType]")
                .On("[cmsContentType].[nodeId] = [cmsDocumentType].[contentTypeNodeId]")
                .InnerJoin("[umbracoNode]")
                .On("[cmsContentType].[nodeId] = [umbracoNode].[id]")
                .Where("([umbracoNode].[nodeObjectType] = @0)", new Guid("a2cb7800-f571-4787-9638-bc48539a0efb"))
                .Where("[cmsDocumentType].[IsDefault] = @0", true)
                .Where("([umbracoNode].[id] = @0)", 1050);

            var sql = Sql();
            sql.SelectAll()
                .From<ContentTypeTemplateDto>()
                .RightJoin<ContentTypeDto>()
                .On<ContentTypeDto, ContentTypeTemplateDto>(left => left.NodeId, right => right.ContentTypeNodeId)
                .InnerJoin<NodeDto>()
                .On<ContentTypeDto, NodeDto>(left => left.NodeId, right => right.NodeId)
                .Where<NodeDto>(x => x.NodeObjectType == nodeObjectType)
                .Where<ContentTypeTemplateDto>(x => x.IsDefault)
                .Where<NodeDto>(x => x.NodeId == 1050);

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
            var expected = Sql();
            expected.SelectAll()
                .From("[cmsPropertyTypeGroup]")
                .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
                .InnerJoin("[cmsDataType]").On("[cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]");

            var sql = Sql();
            sql.SelectAll()
               .From<PropertyTypeGroupDto>()
               .RightJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId);

            Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

            Console.WriteLine(sql.SQL);
        }

        [Test]
        public void Can_Verify_AllowedContentTypeIds_Clause()
        {
            var expected = Sql();
            expected.SelectAll()
                .From("[cmsContentTypeAllowedContentType]")
                .Where("([cmsContentTypeAllowedContentType].[Id] = @0)", 1050);

            var sql = Sql();
            sql.SelectAll()
               .From<ContentTypeAllowedContentTypeDto>()
               .Where<ContentTypeAllowedContentTypeDto>(x => x.Id == 1050);

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
            var expected = Sql();
            expected.SelectAll()
                .From("[cmsPropertyTypeGroup]")
                .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
                .InnerJoin("[cmsDataType]").On("[cmsPropertyType].[dataTypeId] = [cmsDataType].[nodeId]")
                .Where("([cmsPropertyType].[contentTypeId] = @0)", 1050);

            var sql = Sql();
            sql.SelectAll()
               .From<PropertyTypeGroupDto>()
               .RightJoin<PropertyTypeDto>()
               .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
               .InnerJoin<DataTypeDto>()
               .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.DataTypeId)
               .Where<PropertyTypeDto>(x => x.ContentTypeId == 1050);

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