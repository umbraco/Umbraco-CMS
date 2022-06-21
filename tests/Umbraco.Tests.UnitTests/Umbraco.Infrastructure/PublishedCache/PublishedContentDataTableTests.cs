using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.PublishedCache;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.PublishedCache;

/// <summary>
///     Unit tests for IPublishedContent and extensions
/// </summary>
[TestFixture]
public class PublishedContentDataTableTests : PublishedSnapshotServiceTestBase
{
    private readonly DataType[] _dataTypes = GetDefaultDataTypes();

    private static ContentType CreateContentType(string name, IDataType dataType, IReadOnlyDictionary<string, string> propertyAliasesAndNames)
    {
        var contentType = new ContentType(TestHelper.ShortStringHelper, -1)
        {
            Alias = name,
            Name = name,
            Key = Guid.NewGuid(),
            Id = name.GetHashCode(),
        };
        foreach (var prop in propertyAliasesAndNames)
        {
            contentType.AddPropertyType(new PropertyType(TestHelper.ShortStringHelper, dataType, prop.Key)
            {
                Name = prop.Value,
            });
        }

        return contentType;
    }

    private IEnumerable<ContentNodeKit> CreateCache(
        bool createChildren,
        IDataType dataType,
        out ContentType[] contentTypes)
    {
        var result = new List<ContentNodeKit>();
        var valueCounter = 1;
        var parentId = 3;

        var properties = new Dictionary<string, string> { ["property1"] = "Property 1", ["property2"] = "Property 2" };

        var parentContentType = CreateContentType(
            "Parent",
            dataType,
            new Dictionary<string, string>(properties) { ["property3"] = "Property 3" });
        var childContentType = CreateContentType(
            "Child",
            dataType,
            new Dictionary<string, string>(properties) { ["property4"] = "Property 4" });
        var child2ContentType = CreateContentType(
            "Child2",
            dataType,
            new Dictionary<string, string>(properties) { ["property4"] = "Property 4" });

        contentTypes = new[] { parentContentType, childContentType, child2ContentType };

        var parentData = new ContentDataBuilder()
            .WithName("Page" + Guid.NewGuid())
            .WithProperties(new PropertyDataBuilder()
                .WithPropertyData("property1", "value" + valueCounter)
                .WithPropertyData("property2", "value" + (valueCounter + 1))
                .WithPropertyData("property3", "value" + (valueCounter + 2))
                .Build())
            .Build();

        var parent = ContentNodeKitBuilder.CreateWithContent(
            parentContentType.Id,
            parentId,
            $"-1,{parentId}",
            draftData: parentData,
            publishedData: parentData);

        result.Add(parent);

        if (createChildren)
        {
            for (var i = 0; i < 3; i++)
            {
                valueCounter += 3;
                var childId = parentId + i + 1;

                var childData = new ContentDataBuilder()
                    .WithName("Page" + Guid.NewGuid())
                    .WithProperties(new PropertyDataBuilder()
                        .WithPropertyData("property1", "value" + valueCounter)
                        .WithPropertyData("property2", "value" + (valueCounter + 1))
                        .WithPropertyData("property4", "value" + (valueCounter + 2))
                        .Build())
                    .Build();

                var child = ContentNodeKitBuilder.CreateWithContent(
                    i > 0 ? childContentType.Id : child2ContentType.Id,
                    childId,
                    $"-1,{parentId},{childId}",
                    i,
                    draftData: childData,
                    publishedData: childData);

                result.Add(child);
            }
        }

        return result;
    }

    [Test]
    public void To_DataTable()
    {
        var cache = CreateCache(true, _dataTypes[0], out var contentTypes);
        InitializedCache(cache, contentTypes, _dataTypes);

        var snapshot = GetPublishedSnapshot();
        var root = snapshot.Content.GetAtRoot().First();

        var dt = root.ChildrenAsTable(
            VariationContextAccessor,
            ContentTypeService,
            MediaTypeService,
            Mock.Of<IMemberTypeService>(),
            Mock.Of<IPublishedUrlProvider>());

        Assert.AreEqual(11, dt.Columns.Count);
        Assert.AreEqual(3, dt.Rows.Count);
        Assert.AreEqual("value4", dt.Rows[0]["Property 1"]);
        Assert.AreEqual("value5", dt.Rows[0]["Property 2"]);
        Assert.AreEqual("value6", dt.Rows[0]["Property 4"]);
        Assert.AreEqual("value7", dt.Rows[1]["Property 1"]);
        Assert.AreEqual("value8", dt.Rows[1]["Property 2"]);
        Assert.AreEqual("value9", dt.Rows[1]["Property 4"]);
        Assert.AreEqual("value10", dt.Rows[2]["Property 1"]);
        Assert.AreEqual("value11", dt.Rows[2]["Property 2"]);
        Assert.AreEqual("value12", dt.Rows[2]["Property 4"]);
    }

    [Test]
    public void To_DataTable_With_Filter()
    {
        var cache = CreateCache(true, _dataTypes[0], out var contentTypes);
        InitializedCache(cache, contentTypes, _dataTypes);

        var snapshot = GetPublishedSnapshot();
        var root = snapshot.Content.GetAtRoot().First();

        var dt = root.ChildrenAsTable(
            VariationContextAccessor,
            ContentTypeService,
            MediaTypeService,
            Mock.Of<IMemberTypeService>(),
            Mock.Of<IPublishedUrlProvider>(),
            "Child");

        Assert.AreEqual(11, dt.Columns.Count);
        Assert.AreEqual(2, dt.Rows.Count);
        Assert.AreEqual("value7", dt.Rows[0]["Property 1"]);
        Assert.AreEqual("value8", dt.Rows[0]["Property 2"]);
        Assert.AreEqual("value9", dt.Rows[0]["Property 4"]);
        Assert.AreEqual("value10", dt.Rows[1]["Property 1"]);
        Assert.AreEqual("value11", dt.Rows[1]["Property 2"]);
        Assert.AreEqual("value12", dt.Rows[1]["Property 4"]);
    }

    [Test]
    public void To_DataTable_No_Rows()
    {
        var cache = CreateCache(false, _dataTypes[0], out var contentTypes);
        InitializedCache(cache, contentTypes, _dataTypes);

        var snapshot = GetPublishedSnapshot();
        var root = snapshot.Content.GetAtRoot().First();

        var dt = root.ChildrenAsTable(
            VariationContextAccessor,
            ContentTypeService,
            MediaTypeService,
            Mock.Of<IMemberTypeService>(),
            Mock.Of<IPublishedUrlProvider>());

        // will return an empty data table
        Assert.AreEqual(0, dt.Columns.Count);
        Assert.AreEqual(0, dt.Rows.Count);
    }
}
