// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="ContentType"/> model in Umbraco CMS, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class ContentTypeTests
{
    /// <summary>
    /// Tests that adding a property type with a duplicate alias to a content type throws an InvalidOperationException.
    /// </summary>
    [Test]
    [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
    public void Cannot_Add_Duplicate_Property_Aliases()
    {
        var contentType = BuildContentType();

        var propertyTypeBuilder = new PropertyTypeBuilder();
        var additionalPropertyType = propertyTypeBuilder
            .WithAlias("title")
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            contentType.PropertyTypeCollection.Add(additionalPropertyType));
    }

    /// <summary>
    /// Tests that updating a property alias to a duplicate alias throws an InvalidOperationException.
    /// </summary>
    [Test]
    [Ignore("Ignoring this test until we actually enforce this, see comments in ContentTypeBase.PropertyTypesChanged")]
    public void Cannot_Update_Duplicate_Property_Aliases()
    {
        var contentType = BuildContentType();

        var propertyTypeBuilder = new PropertyTypeBuilder();
        var additionalPropertyType = propertyTypeBuilder
            .WithAlias("title")
            .Build();

        contentType.PropertyTypeCollection.Add(additionalPropertyType);

        var toUpdate = contentType.PropertyTypeCollection["myPropertyType2"];

        Assert.Throws<InvalidOperationException>(() => toUpdate.Alias = "myPropertyType");
    }

    /// <summary>
    /// Verifies that a <see cref="ContentTypeSort"/> instance can be deep cloned correctly.
    /// The test ensures that the cloned object is a different instance from the original,
    /// but has equal values for all relevant properties, including <c>Key</c>, <c>SortOrder</c>, and <c>Alias</c>.
    /// </summary>
    [Test]
    public void Can_Deep_Clone_Content_Type_Sort()
    {
        var contentType = BuildContentTypeSort();
        var clone = (ContentTypeSort)contentType.DeepClone();
        Assert.AreNotSame(clone, contentType);
        Assert.AreEqual(clone, contentType);
        Assert.AreEqual(clone.Key, contentType.Key);
        Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
        Assert.AreEqual(clone.Alias, contentType.Alias);
    }

    private ContentTypeSort BuildContentTypeSort()
    {
        ContentTypeBuilder contentTypeBuilder = new ContentTypeBuilder();
        var builder = new ContentTypeSortBuilder<ContentTypeBuilder>(contentTypeBuilder);
        return builder
            .WithKey(new Guid("4CAE063E-0BE1-4972-B10C-A3D9BB7DE856"))
            .WithSortOrder(4)
            .WithAlias("test")
            .Build();
    }

    /// <summary>
    /// Tests that a content type can be deep cloned with all identities reset.
    /// Ensures the clone has the specified new alias and that none of the cloned
    /// entities retain their original identities.
    /// </summary>
    [Test]
    public void Can_Deep_Clone_Content_Type_With_Reset_Identities()
    {
        var contentType = BuildContentType();

        var clone = (ContentType)contentType.DeepCloneWithResetIdentities("newAlias");

        Assert.AreEqual("newAlias", clone.Alias);
        Assert.AreNotEqual("newAlias", contentType.Alias);
        Assert.IsFalse(clone.HasIdentity);

        foreach (var propertyGroup in clone.PropertyGroups)
        {
            Assert.IsFalse(propertyGroup.HasIdentity);
            foreach (var propertyType in propertyGroup.PropertyTypes)
            {
                Assert.IsFalse(propertyType.HasIdentity);
            }
        }

        foreach (var propertyType in clone.PropertyTypes.Where(x => x.HasIdentity))
        {
            Assert.IsFalse(propertyType.HasIdentity);
        }
    }

    /// <summary>
    /// Tests that a <see cref="ContentType"/> instance can be deep cloned correctly, ensuring that all properties and collections are duplicated and not shared between the original and the clone.
    /// Verifies that the clone is equal to the original but is a separate instance, and that event handlers and property dirty tracking work as expected on the clone.
    /// </summary>
    [Test]
    public void Can_Deep_Clone_Content_Type()
    {
        // Arrange
        var contentType = BuildContentType();

        // Act
        var clone = (ContentType)contentType.DeepClone();

        // Assert
        Assert.AreNotSame(clone, contentType);
        Assert.AreEqual(clone, contentType);
        Assert.AreEqual(clone.Id, contentType.Id);
        Assert.AreEqual(clone.AllowedTemplates.Count(), contentType.AllowedTemplates.Count());
        for (var index = 0; index < contentType.AllowedTemplates.Count(); index++)
        {
            Assert.AreNotSame(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
            Assert.AreEqual(clone.AllowedTemplates.ElementAt(index), contentType.AllowedTemplates.ElementAt(index));
        }

        Assert.AreNotSame(clone.PropertyGroups, contentType.PropertyGroups);
        Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
        }

        Assert.AreNotSame(clone.PropertyTypes, contentType.PropertyTypes);
        Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
        Assert.AreEqual(0, clone.NoGroupPropertyTypes.Count());
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
        }

        Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
        Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
        Assert.AreEqual(clone.Key, contentType.Key);
        Assert.AreEqual(clone.Level, contentType.Level);
        Assert.AreEqual(clone.Path, contentType.Path);
        Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
        Assert.AreNotSame(clone.DefaultTemplate, contentType.DefaultTemplate);
        Assert.AreEqual(clone.DefaultTemplate, contentType.DefaultTemplate);
        Assert.AreEqual(clone.DefaultTemplateId, contentType.DefaultTemplateId);
        Assert.AreEqual(clone.Trashed, contentType.Trashed);
        Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
        Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
        Assert.AreEqual(clone.Icon, contentType.Icon);
        Assert.AreEqual(clone.ListView, contentType.ListView);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
        }

        // Need to ensure the event handlers are wired
        var asDirty = (ICanBeDirty)clone;

        Assert.IsFalse(asDirty.IsPropertyDirty("PropertyTypes"));

        var propertyTypeBuilder = new PropertyTypeBuilder();
        var additionalPropertyType = propertyTypeBuilder
            .WithAlias("blah")
            .Build();

        clone.AddPropertyType(additionalPropertyType);
        Assert.IsTrue(asDirty.IsPropertyDirty("PropertyTypes"));
        Assert.IsFalse(asDirty.IsPropertyDirty("PropertyGroups"));
        clone.AddPropertyGroup("hello", "hello");
        Assert.IsTrue(asDirty.IsPropertyDirty("PropertyGroups"));
    }

    /// <summary>
    /// Tests that a content type can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Content_Type_Without_Error()
    {
        // Arrange
        var contentType = BuildContentType();

        var json = JsonSerializer.Serialize(contentType);
        Debug.Print(json);
    }

    private static ContentType BuildContentType()
    {
        var builder = new ContentTypeBuilder();
        return builder.BuildSimpleContentType();
    }

    /// <summary>
    /// Tests that a MediaType content type can be deep cloned correctly.
    /// </summary>
    [Test]
    public void Can_Deep_Clone_Media_Type()
    {
        // Arrange
        var contentType = BuildMediaType();

        // Act
        var clone = (MediaType)contentType.DeepClone();

        // Assert
        Assert.AreNotSame(clone, contentType);
        Assert.AreEqual(clone, contentType);
        Assert.AreEqual(clone.Id, contentType.Id);
        Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
        }

        Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
        }

        Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
        Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
        Assert.AreEqual(clone.Key, contentType.Key);
        Assert.AreEqual(clone.Level, contentType.Level);
        Assert.AreEqual(clone.Path, contentType.Path);
        Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
        Assert.AreEqual(clone.Trashed, contentType.Trashed);
        Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
        Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
        Assert.AreEqual(clone.Icon, contentType.Icon);
        Assert.AreEqual(clone.ListView, contentType.ListView);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
        }
    }

    /// <summary>
    /// Tests that a media type content can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Media_Type_Without_Error()
    {
        // Arrange
        var contentType = BuildMediaType();

        var json = JsonSerializer.Serialize(contentType);
        Debug.Print(json);
    }

    private static MediaType BuildMediaType()
    {
        var builder = new MediaTypeBuilder();
        return builder.BuildImageMediaType();
    }

    /// <summary>
    /// Tests that a MemberType can be deeply cloned correctly.
    /// </summary>
    [Test]
    public void Can_Deep_Clone_Member_Type()
    {
        // Arrange
        var contentType = BuildMemberType();

        // Act
        var clone = (MemberType)contentType.DeepClone();

        // Assert
        Assert.AreNotSame(clone, contentType);
        Assert.AreEqual(clone, contentType);
        Assert.AreEqual(clone.Id, contentType.Id);
        Assert.AreEqual(clone.PropertyGroups.Count, contentType.PropertyGroups.Count);
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.AreNotSame(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
            Assert.AreEqual(clone.PropertyGroups[index], contentType.PropertyGroups[index]);
        }

        Assert.AreEqual(clone.PropertyTypes.Count(), contentType.PropertyTypes.Count());
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.AreNotSame(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
            Assert.AreEqual(clone.PropertyTypes.ElementAt(index), contentType.PropertyTypes.ElementAt(index));
        }

        Assert.AreEqual(clone.CreateDate, contentType.CreateDate);
        Assert.AreEqual(clone.CreatorId, contentType.CreatorId);
        Assert.AreEqual(clone.Key, contentType.Key);
        Assert.AreEqual(clone.Level, contentType.Level);
        Assert.AreEqual(clone.Path, contentType.Path);
        Assert.AreEqual(clone.SortOrder, contentType.SortOrder);
        Assert.AreEqual(clone.Trashed, contentType.Trashed);
        Assert.AreEqual(clone.UpdateDate, contentType.UpdateDate);
        Assert.AreEqual(clone.Thumbnail, contentType.Thumbnail);
        Assert.AreEqual(clone.Icon, contentType.Icon);
        Assert.AreEqual(clone.ListView, contentType.ListView);

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(contentType, null));
        }
    }

    /// <summary>
    /// Tests that a member type can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Member_Type_Without_Error()
    {
        // Arrange
        var contentType = BuildMemberType();

        var json = JsonSerializer.Serialize(contentType);
        Debug.Print(json);
    }

    /// <summary>
    /// Tests setting the member-specific property type options for sensitivity, view, and edit permissions.
    /// </summary>
    /// <param name="isSensitive">Indicates whether the property is sensitive.</param>
    /// <param name="canView">Indicates whether the member can view the property.</param>
    /// <param name="canEdit">Indicates whether the member can edit the property.</param>
    [Test]
    [TestCase(false, false, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(true, true, true)]
    public void Can_Set_Is_Member_Specific_Property_Type_Options(bool isSensitive, bool canView, bool canEdit)
    {
        var propertyTypeAlias = "testType";
        var memberType = BuildMemberType();
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("testType")
            .Build();

        memberType.AddPropertyType(propertyType);

        memberType.SetIsSensitiveProperty(propertyTypeAlias, isSensitive);
        memberType.SetMemberCanViewProperty(propertyTypeAlias, canView);
        memberType.SetMemberCanEditProperty(propertyTypeAlias, canEdit);

        Assert.Multiple(() =>
        {
            Assert.AreEqual(isSensitive, memberType.IsSensitiveProperty(propertyTypeAlias));
            Assert.AreEqual(canView, memberType.MemberCanViewProperty(propertyTypeAlias));
            Assert.AreEqual(canEdit, memberType.MemberCanEditProperty(propertyTypeAlias));
        });
    }

    private static MemberType BuildMemberType()
    {
        var builder = new MemberTypeBuilder();
        return builder.BuildSimpleMemberType();
    }
}
