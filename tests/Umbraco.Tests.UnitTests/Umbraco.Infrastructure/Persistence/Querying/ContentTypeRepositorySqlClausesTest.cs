// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;
using static Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Persistence.Querying;

/// <summary>
/// Contains unit tests that verify the SQL clauses used by the <c>ContentTypeRepository</c> in the persistence layer.
/// </summary>
[TestFixture]
public class ContentTypeRepositorySqlClausesTest : BaseUsingSqlSyntax
{
    /// <summary>
    /// Verifies that the base SQL clause for content type repository is constructed correctly.
    /// </summary>
    [Test]
    public void Can_Verify_Base_Clause()
    {
        var nodeObjectType = Constants.ObjectTypes.DocumentType;

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
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    /// <summary>
    /// Verifies that the base WHERE clause for the ContentType repository SQL query is correctly constructed.
    /// </summary>
    [Test]
    public void Can_Verify_Base_Where_Clause()
    {
        var nodeObjectType = Constants.ObjectTypes.DocumentType;

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
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    /// <summary>
    /// Tests that the PerformQuery clause generates the correct SQL statement by comparing
    /// the SQL output of the query builder against an explicitly constructed expected SQL statement.
    /// Ensures that the SQL generated for joining property type groups, property types, and data types is as expected.
    /// </summary>
    [Test]
    public void Can_Verify_PerformQuery_Clause()
    {
        var expected = Sql();
        expected.SelectAll()
            .From("[cmsPropertyTypeGroup]")
            .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
            .InnerJoin($"[{Constants.DatabaseSchema.Tables.DataType}]")
            .On($"[cmsPropertyType].[dataTypeId] = [{Constants.DatabaseSchema.Tables.DataType}].[nodeId]");

        var sql = Sql();
        sql.SelectAll()
            .From<PropertyTypeGroupDto>()
            .RightJoin<PropertyTypeDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Debug.Print(sql.SQL);
    }

    /// <summary>
    /// Tests that the SQL clause for verifying allowed content type IDs is generated correctly.
    /// </summary>
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
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    /// <summary>
    /// Verifies that the SQL clause generated for the PropertyGroupCollection matches the expected SQL statement.
    /// This ensures that the query construction logic for property group collections is correct and produces the intended SQL output.
    /// </summary>
    [Test]
    public void Can_Verify_PropertyGroupCollection_Clause()
    {
        var expected = Sql();
        expected.SelectAll()
            .From("[cmsPropertyTypeGroup]")
            .RightJoin("[cmsPropertyType]").On("[cmsPropertyTypeGroup].[id] = [cmsPropertyType].[propertyTypeGroupId]")
            .InnerJoin($"[{Constants.DatabaseSchema.Tables.DataType}]")
            .On($"[cmsPropertyType].[dataTypeId] = [{Constants.DatabaseSchema.Tables.DataType}].[nodeId]")
            .Where("([cmsPropertyType].[contentTypeId] = @0)", 1050);

        var sql = Sql();
        sql.SelectAll()
            .From<PropertyTypeGroupDto>()
            .RightJoin<PropertyTypeDto>()
            .On<PropertyTypeGroupDto, PropertyTypeDto>(left => left.Id, right => right.PropertyTypeGroupId)
            .InnerJoin<DataTypeDto>()
            .On<PropertyTypeDto, DataTypeDto>(left => left.DataTypeId, right => right.NodeId)
            .Where<PropertyTypeDto>(x => x.ContentTypeId == 1050);

        Assert.That(sql.SQL, Is.EqualTo(expected.SQL));

        Assert.AreEqual(expected.Arguments.Length, sql.Arguments.Length);
        for (var i = 0; i < expected.Arguments.Length; i++)
        {
            Assert.AreEqual(expected.Arguments[i], sql.Arguments[i]);
        }

        Debug.Print(sql.SQL);
    }

    /// <summary>
    /// Tests that the SQL generated by the WhereLike clause is correct.
    /// </summary>
    [Test]
    public void Can_Verify_WhereLike_Clause()
    {
        var key = Guid.NewGuid();
        var propertyTypeId = 1234;
        Sql<ISqlContext> sql = Sql()
            .Delete<UserGroup2GranularPermissionDto>()
            .Where<UserGroup2GranularPermissionDto>(c => c.UniqueId == key)
            .WhereLike<UserGroup2GranularPermissionDto>(
                c => c.Permission,
                Sql()
                    .SelectClosure<PropertyTypeDto>(c => c.ConvertUniqueIdentifierToString(x => x.UniqueId))
                    .From<PropertyTypeDto>()
                    .WhereClosure<PropertyTypeDto>(c => c.Id == propertyTypeId),
                $"'|{SqlContext.SqlSyntax.GetWildcardPlaceholder()}'");

        string expectedSQL =
@"DELETE FROM [umbracoUserGroup2GranularPermission]
WHERE (([umbracoUserGroup2GranularPermission].[uniqueId] = @0))
AND ([umbracoUserGroup2GranularPermission].[permission] LIKE CONCAT(((SELECT 
 CONVERT(nvarchar(36), [cmsPropertyType].[UniqueId])
 
FROM [cmsPropertyType]
WHERE (([cmsPropertyType].[id] = @1))
)),'|%'))".Replace("\r", string.Empty);
        var typedSql = sql.SQL;
        Assert.That(typedSql, Is.EqualTo(expectedSQL));
    }
}
