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
    public void Can_Reset_Dirty_Properties_Cascades_Into_Property_Types()
    {
        var contentType = BuildContentType();

        // Add an un-grouped property type so both cascade paths are covered: property types held
        // on a property group, and property types held directly on the content type (no group).
        contentType.AddPropertyType(new PropertyTypeBuilder().WithAlias("noGroup").WithName("No Group").Build());

        var groupedPropertyTypes = contentType.PropertyGroups
            .Where(g => g.PropertyTypes is not null)
            .SelectMany(g => g.PropertyTypes!)
            .ToList();
        var noGroupPropertyTypes = contentType.NoGroupPropertyTypes.ToList();
        Assert.Multiple(() =>
        {
            Assert.That(groupedPropertyTypes, Is.Not.Empty, "expected at least one grouped property type");
            Assert.That(noGroupPropertyTypes, Is.Not.Empty, "expected at least one no-group property type");
        });

        var allPropertyTypes = groupedPropertyTypes.Concat(noGroupPropertyTypes).ToList();

        // Establish a clean baseline on each property type directly, not via the content type,
        // so the arrange step does not depend on the behaviour under test. Then dirty every one.
        foreach (var propertyType in allPropertyTypes)
        {
            propertyType.ResetDirtyProperties(false);
            propertyType.Variations = ContentVariation.Culture;
            Assert.That(propertyType.IsDirty(), Is.True, $"'{propertyType.Alias}' should be dirty before the reset");
        }

        // The (bool) overload is the one the repository cache calls on every round-trip.
        contentType.ResetDirtyProperties(false);

        Assert.Multiple(() =>
        {
            foreach (var propertyType in allPropertyTypes)
            {
                Assert.That(propertyType.IsDirty(), Is.False, $"'{propertyType.Alias}' should not be dirty after the reset");
            }
        });
    }

    [Test]
    public void Can_Reset_Dirty_Properties_Cascades_And_Remembers_When_Requested()
    {
        var contentType = BuildContentType();
        var property = contentType.PropertyGroups.First().PropertyTypes!.First();

        property.ResetDirtyProperties(false);
        property.Variations = ContentVariation.Culture;
        Assert.That(property.IsDirty(), Is.True);

        contentType.ResetDirtyProperties(true);

        Assert.Multiple(() =>
        {
            Assert.That(property.IsDirty(), Is.False);
            Assert.That(property.WasDirty(), Is.True);
            Assert.That(property.WasPropertyDirty("Variations"), Is.True);
        });
    }

    [Test]
    public void Can_Reset_Dirty_Properties_Cascades_Via_Parameterless_Overload()
    {
        var contentType = BuildContentType();
        var property = contentType.PropertyGroups.First().PropertyTypes!.First();

        property.ResetDirtyProperties(false);
        property.Variations = ContentVariation.Culture;
        Assert.That(property.IsDirty(), Is.True);

        // The parameterless overload delegates to ResetDirtyProperties(true) and must still cascade.
        contentType.ResetDirtyProperties();

        Assert.That(property.IsDirty(), Is.False);
    }

    [Test]
    public void Can_Move_PropertyType_To_No_Group()
    {
        var contentType = BuildContentTypeWithSingleGroup();

        Assert.That(contentType.MovePropertyType("title", null), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.PropertyTypes.Select(x => x.Alias), Is.EquivalentTo(new[] { "title" }));
            Assert.That(contentType.NoGroupPropertyTypes.Select(x => x.Alias), Is.EquivalentTo(new[] { "title" }));
            Assert.That(contentType.PropertyGroups["content"].PropertyTypes, Is.Empty);
        });
    }

    [Test]
    public void Can_Move_PropertyType_From_No_Group_Into_Group()
    {
        var contentType = BuildContentTypeWithSingleGroup();
        contentType.AddPropertyType(new PropertyTypeBuilder().WithAlias("noGroup").WithName("No Group").Build());

        Assert.That(contentType.MovePropertyType("noGroup", "content"), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);
            Assert.That(
                contentType.PropertyGroups["content"].PropertyTypes!.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title", "noGroup" }));
        });
    }

    [Test]
    public void Can_Move_PropertyType_Between_Groups()
    {
        var contentType = BuildContentTypeWithSingleGroup();
        contentType.AddPropertyGroup("meta", "Meta");

        Assert.That(contentType.MovePropertyType("title", "meta"), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);
            Assert.That(contentType.PropertyGroups["content"].PropertyTypes, Is.Empty);
            Assert.That(
                contentType.PropertyGroups["meta"].PropertyTypes!.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title" }));
        });
    }

    [Test]
    public void Can_Move_PropertyType_Into_Group_With_Uninitialised_PropertyTypes()
    {
        var contentType = BuildContentTypeWithSingleGroup();
        contentType.AddPropertyGroup("meta", "Meta");
        contentType.PropertyGroups["meta"].PropertyTypes = null;

        Assert.That(contentType.MovePropertyType("title", "meta"), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(
                contentType.PropertyTypes.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title" }),
                "the property type should not be orphaned");
            Assert.That(
                contentType.PropertyGroups["meta"].PropertyTypes?.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title" }));
            Assert.That(contentType.PropertyGroups["content"].PropertyTypes, Is.Empty);
        });
    }

    [Test]
    public void Can_Move_Already_Ungrouped_PropertyType_To_No_Group()
    {
        var contentType = BuildContentTypeWithUngroupedProperty();
        Assert.That(contentType.NoGroupPropertyTypes.Select(x => x.Alias), Is.EquivalentTo(new[] { "title" }));

        // Moving a property that is already un-grouped is a no-op, and must not duplicate it.
        Assert.That(contentType.MovePropertyType("title", null), Is.True);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.PropertyTypes.Select(x => x.Alias), Is.EquivalentTo(new[] { "title" }));
            Assert.That(contentType.NoGroupPropertyTypes.Select(x => x.Alias), Is.EquivalentTo(new[] { "title" }));
            Assert.That(contentType.PropertyGroups["content"].PropertyTypes, Is.Empty);
        });
    }

    [Test]
    public void Cannot_Move_PropertyType_To_Unknown_Group()
    {
        var contentType = BuildContentTypeWithSingleGroup();

        Assert.That(contentType.MovePropertyType("title", "noSuchGroup"), Is.False);

        Assert.Multiple(() =>
        {
            Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);
            Assert.That(
                contentType.PropertyGroups["content"].PropertyTypes!.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title" }));
        });
    }

    [Test]
    public void Cannot_Move_Unknown_PropertyType()
    {
        var contentType = BuildContentTypeWithSingleGroup();

        Assert.Multiple(() =>
        {
            Assert.That(contentType.MovePropertyType("noSuchProperty", null), Is.False);
            Assert.That(contentType.MovePropertyType("noSuchProperty", "content"), Is.False);
        });

        Assert.Multiple(() =>
        {
            Assert.That(contentType.NoGroupPropertyTypes, Is.Empty);
            Assert.That(
                contentType.PropertyGroups["content"].PropertyTypes!.Select(x => x.Alias),
                Is.EquivalentTo(new[] { "title" }));
        });
    }

    private static ContentType BuildContentTypeWithSingleGroup() =>
        (ContentType)new ContentTypeBuilder()
            .WithAlias("textPage")
            .WithName("Text Page")
            .WithPropertyTypeIdsIncrementingFrom(200)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .AddPropertyType()
                    .WithAlias("title")
                    .WithName("Title")
                    .WithSortOrder(1)
                    .Done()
                .Done()
            .Build();

    /// <remarks>
    ///     A property type added at the builder root belongs to no group, so this yields an empty
    ///     property group alongside an un-grouped property type.
    /// </remarks>
    private static ContentType BuildContentTypeWithUngroupedProperty() =>
        (ContentType)new ContentTypeBuilder()
            .WithAlias("textPage")
            .WithName("Text Page")
            .WithPropertyTypeIdsIncrementingFrom(200)
            .AddPropertyGroup()
                .WithAlias("content")
                .WithName("Content")
                .WithSortOrder(1)
                .Done()
            .AddPropertyType()
                .WithAlias("title")
                .WithName("Title")
                .WithSortOrder(1)
                .Done()
            .Build();

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
