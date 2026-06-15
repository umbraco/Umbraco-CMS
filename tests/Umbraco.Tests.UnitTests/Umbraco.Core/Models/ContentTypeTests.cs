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

[TestFixture]
public class ContentTypeTests
{
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

    [Test]
    public void Can_Deep_Clone_Content_Type_Sort()
    {
        var contentType = BuildContentTypeSort();
        var clone = (ContentTypeSort)contentType.DeepClone();
        Assert.That(contentType, Is.Not.SameAs(clone));
        Assert.That(contentType, Is.EqualTo(clone));
        Assert.That(contentType.Key, Is.EqualTo(clone.Key));
        Assert.That(contentType.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(contentType.Alias, Is.EqualTo(clone.Alias));
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

    [Test]
    public void Can_Deep_Clone_Content_Type_With_Reset_Identities()
    {
        var contentType = BuildContentType();

        var clone = (ContentType)contentType.DeepCloneWithResetIdentities("newAlias");

        Assert.That(clone.Alias, Is.EqualTo("newAlias"));
        Assert.That(contentType.Alias, Is.Not.EqualTo("newAlias"));
        Assert.That(clone.HasIdentity, Is.False);

        foreach (var propertyGroup in clone.PropertyGroups)
        {
            Assert.That(propertyGroup.HasIdentity, Is.False);
            foreach (var propertyType in propertyGroup.PropertyTypes)
            {
                Assert.That(propertyType.HasIdentity, Is.False);
            }
        }

        foreach (var propertyType in clone.PropertyTypes.Where(x => x.HasIdentity))
        {
            Assert.That(propertyType.HasIdentity, Is.False);
        }
    }

    [Test]
    public void Can_Deep_Clone_Content_Type()
    {
        // Arrange
        var contentType = BuildContentType();

        // Act
        var clone = (ContentType)contentType.DeepClone();

        // Assert
        Assert.That(contentType, Is.Not.SameAs(clone));
        Assert.That(contentType, Is.EqualTo(clone));
        Assert.That(contentType.Id, Is.EqualTo(clone.Id));
        Assert.That(contentType.AllowedTemplates.Count(), Is.EqualTo(clone.AllowedTemplates.Count()));
        for (var index = 0; index < contentType.AllowedTemplates.Count(); index++)
        {
            Assert.That(contentType.AllowedTemplates.ElementAt(index), Is.Not.SameAs(clone.AllowedTemplates.ElementAt(index)));
            Assert.That(contentType.AllowedTemplates.ElementAt(index), Is.EqualTo(clone.AllowedTemplates.ElementAt(index)));
        }

        Assert.That(contentType.PropertyGroups, Is.Not.SameAs(clone.PropertyGroups));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(clone.PropertyGroups.Count));
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.That(contentType.PropertyGroups[index], Is.Not.SameAs(clone.PropertyGroups[index]));
            Assert.That(contentType.PropertyGroups[index], Is.EqualTo(clone.PropertyGroups[index]));
        }

        Assert.That(contentType.PropertyTypes, Is.Not.SameAs(clone.PropertyTypes));
        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(clone.PropertyTypes.Count()));
        Assert.That(clone.NoGroupPropertyTypes.Count(), Is.EqualTo(0));
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.Not.SameAs(clone.PropertyTypes.ElementAt(index)));
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.EqualTo(clone.PropertyTypes.ElementAt(index)));
        }

        Assert.That(contentType.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(contentType.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(contentType.Key, Is.EqualTo(clone.Key));
        Assert.That(contentType.Level, Is.EqualTo(clone.Level));
        Assert.That(contentType.Path, Is.EqualTo(clone.Path));
        Assert.That(contentType.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(contentType.DefaultTemplate, Is.Not.SameAs(clone.DefaultTemplate));
        Assert.That(contentType.DefaultTemplate, Is.EqualTo(clone.DefaultTemplate));
        Assert.That(contentType.DefaultTemplateId, Is.EqualTo(clone.DefaultTemplateId));
        Assert.That(contentType.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(contentType.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(contentType.Thumbnail, Is.EqualTo(clone.Thumbnail));
        Assert.That(contentType.Icon, Is.EqualTo(clone.Icon));
        Assert.That(contentType.ListView, Is.EqualTo(clone.ListView));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(contentType, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }

        // Need to ensure the event handlers are wired
        var asDirty = (ICanBeDirty)clone;

        Assert.That(asDirty.IsPropertyDirty("PropertyTypes"), Is.False);

        var propertyTypeBuilder = new PropertyTypeBuilder();
        var additionalPropertyType = propertyTypeBuilder
            .WithAlias("blah")
            .Build();

        clone.AddPropertyType(additionalPropertyType);
        Assert.That(asDirty.IsPropertyDirty("PropertyTypes"), Is.True);
        Assert.That(asDirty.IsPropertyDirty("PropertyGroups"), Is.False);
        clone.AddPropertyGroup("hello", "hello");
        Assert.That(asDirty.IsPropertyDirty("PropertyGroups"), Is.True);
    }

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

    [Test]
    public void Can_Deep_Clone_Media_Type()
    {
        // Arrange
        var contentType = BuildMediaType();

        // Act
        var clone = (MediaType)contentType.DeepClone();

        // Assert
        Assert.That(contentType, Is.Not.SameAs(clone));
        Assert.That(contentType, Is.EqualTo(clone));
        Assert.That(contentType.Id, Is.EqualTo(clone.Id));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(clone.PropertyGroups.Count));
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.That(contentType.PropertyGroups[index], Is.Not.SameAs(clone.PropertyGroups[index]));
            Assert.That(contentType.PropertyGroups[index], Is.EqualTo(clone.PropertyGroups[index]));
        }

        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(clone.PropertyTypes.Count()));
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.Not.SameAs(clone.PropertyTypes.ElementAt(index)));
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.EqualTo(clone.PropertyTypes.ElementAt(index)));
        }

        Assert.That(contentType.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(contentType.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(contentType.Key, Is.EqualTo(clone.Key));
        Assert.That(contentType.Level, Is.EqualTo(clone.Level));
        Assert.That(contentType.Path, Is.EqualTo(clone.Path));
        Assert.That(contentType.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(contentType.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(contentType.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(contentType.Thumbnail, Is.EqualTo(clone.Thumbnail));
        Assert.That(contentType.Icon, Is.EqualTo(clone.Icon));
        Assert.That(contentType.ListView, Is.EqualTo(clone.ListView));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(contentType, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

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

    [Test]
    public void Can_Deep_Clone_Member_Type()
    {
        // Arrange
        var contentType = BuildMemberType();

        // Act
        var clone = (MemberType)contentType.DeepClone();

        // Assert
        Assert.That(contentType, Is.Not.SameAs(clone));
        Assert.That(contentType, Is.EqualTo(clone));
        Assert.That(contentType.Id, Is.EqualTo(clone.Id));
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(clone.PropertyGroups.Count));
        for (var index = 0; index < contentType.PropertyGroups.Count; index++)
        {
            Assert.That(contentType.PropertyGroups[index], Is.Not.SameAs(clone.PropertyGroups[index]));
            Assert.That(contentType.PropertyGroups[index], Is.EqualTo(clone.PropertyGroups[index]));
        }

        Assert.That(contentType.PropertyTypes.Count(), Is.EqualTo(clone.PropertyTypes.Count()));
        for (var index = 0; index < contentType.PropertyTypes.Count(); index++)
        {
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.Not.SameAs(clone.PropertyTypes.ElementAt(index)));
            Assert.That(contentType.PropertyTypes.ElementAt(index), Is.EqualTo(clone.PropertyTypes.ElementAt(index)));
        }

        Assert.That(contentType.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(contentType.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(contentType.Key, Is.EqualTo(clone.Key));
        Assert.That(contentType.Level, Is.EqualTo(clone.Level));
        Assert.That(contentType.Path, Is.EqualTo(clone.Path));
        Assert.That(contentType.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(contentType.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(contentType.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(contentType.Thumbnail, Is.EqualTo(clone.Thumbnail));
        Assert.That(contentType.Icon, Is.EqualTo(clone.Icon));
        Assert.That(contentType.ListView, Is.EqualTo(clone.ListView));

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(contentType, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }
    }

    [Test]
    public void Can_Serialize_Member_Type_Without_Error()
    {
        // Arrange
        var contentType = BuildMemberType();

        var json = JsonSerializer.Serialize(contentType);
        Debug.Print(json);
    }

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
            Assert.That(memberType.IsSensitiveProperty(propertyTypeAlias), Is.EqualTo(isSensitive));
            Assert.That(memberType.MemberCanViewProperty(propertyTypeAlias), Is.EqualTo(canView));
            Assert.That(memberType.MemberCanEditProperty(propertyTypeAlias), Is.EqualTo(canEdit));
        });
    }

    private static MemberType BuildMemberType()
    {
        var builder = new MemberTypeBuilder();
        return builder.BuildSimpleMemberType();
    }
}
