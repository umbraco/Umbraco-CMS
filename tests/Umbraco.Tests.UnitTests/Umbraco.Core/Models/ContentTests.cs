// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Extensions;
using Umbraco.Cms.Tests.Common.TestHelpers.Stubs;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models;

/// <summary>
/// Contains unit tests for the <see cref="Content"/> model in Umbraco CMS, verifying its behavior and functionality.
/// </summary>
[TestFixture]
public class ContentTests
{
    private readonly IContentTypeService _contentTypeService = Mock.Of<IContentTypeService>();

    private readonly PropertyEditorCollection _propertyEditorCollection = new (new DataEditorCollection(() => []));

    /// <summary>
    /// Verifies that updating the culture-specific name of a content variant correctly tracks dirty changes
    /// on both the culture info and the parent content's culture info collection.
    /// </summary>
    /// <param name="newName">The new culture-specific name to assign to the variant.</param>
    /// <param name="expectedDirty">True if the property is expected to be marked as dirty after the update; otherwise, false.</param>
    [TestCase("name-fr", false)]
    [TestCase("name-fr-updated", true)]
    public void Variant_Culture_Names_Track_Dirty_Changes(string newName, bool expectedDirty)
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = new ContentBuilder()
            .WithId(1)
            .WithVersionId(1)
            .WithName("content")
            .WithContentType(contentType)
            .Build();

        const string langFr = "fr-FR";

        Assert.IsFalse(content.IsPropertyDirty("CultureInfos")); // hasn't been changed

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        Assert.IsTrue(
            content.IsPropertyDirty("CultureInfos")); // now it will be changed since the collection has changed
        var frCultureName = content.CultureInfos[langFr];
        Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));

        content.ResetDirtyProperties();
        frCultureName.ResetDirtyProperties();

        Assert.IsFalse(content.IsPropertyDirty("CultureInfos")); // it's been reset
        Assert.IsTrue(content.WasPropertyDirty("CultureInfos"));

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName(newName, langFr);

        // dirty is only true if we updated the name
        Assert.AreEqual(expectedDirty, frCultureName.IsPropertyDirty("Date"));
        Assert.AreEqual(expectedDirty, content.IsPropertyDirty("CultureInfos"));
    }

    /// <summary>
    /// Verifies that changes to published culture names in content variants correctly track and reset dirty states.
    /// Ensures that modifying culture names and publishing updates mark the relevant properties as dirty, and that resetting these properties clears the dirty state as expected.
    /// </summary>
    [Test]
    public void Variant_Published_Culture_Names_Track_Dirty_Changes()
    {
        var contentType = new ContentTypeBuilder()
            .WithAlias("contentType")
            .WithContentVariation(ContentVariation.Culture)
            .Build();
        var content = new ContentBuilder()
            .WithId(1)
            .WithVersionId(1)
            .WithName("content")
            .WithContentType(contentType)
            .Build();

        const string langFr = "fr-FR";

        content.ChangeContentType(contentType);

        Assert.IsFalse(content.IsPropertyDirty("PublishCultureInfos")); // hasn't been changed

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection); // we've set the name, now we're publishing it
        Assert.IsTrue(
            content.IsPropertyDirty("PublishCultureInfos")); // now it will be changed since the collection has changed
        var frCultureName = content.PublishCultureInfos[langFr];
        Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));

        content.ResetDirtyProperties();
        frCultureName.ResetDirtyProperties();

        Assert.IsFalse(content.IsPropertyDirty("PublishCultureInfos")); // it's been reset
        Assert.IsTrue(content.WasPropertyDirty("PublishCultureInfos"));

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection); // we've set the name, now we're publishing it
        Assert.IsTrue(frCultureName.IsPropertyDirty("Date"));
        Assert.IsTrue(content.IsPropertyDirty("PublishCultureInfos")); // it's true now since we've updated a name
    }

    /// <summary>
    /// Tests that the GetNonGroupedProperties method returns only properties that are not grouped.
    /// </summary>
    [Test]
    public void Get_Non_Grouped_Properties()
    {
        var contentType = ContentTypeBuilder.CreateSimpleContentType();

        // Add non-grouped properties
        var pt1 = new PropertyTypeBuilder()
            .WithAlias("nonGrouped1")
            .WithName("Non Grouped 1")
            .Build();
        var pt2 = new PropertyTypeBuilder()
            .WithAlias("nonGrouped2")
            .WithName("Non Grouped 2")
            .Build();
        contentType.AddPropertyType(pt1);
        contentType.AddPropertyType(pt2);

        // Ensure that nothing is marked as dirty
        contentType.ResetDirtyProperties(false);
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateSimpleContent(contentType);

        var nonGrouped = content.GetNonGroupedProperties();

        Assert.AreEqual(2, nonGrouped.Count());
        Assert.AreEqual(5, content.Properties.Count());
    }

    /// <summary>
    /// Tests that all dirty properties of a content instance get reset correctly.
    /// </summary>
    [Test]
    public void All_Dirty_Properties_Get_Reset()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.ResetDirtyProperties(false);

        Assert.IsFalse(content.IsDirty());
        foreach (var prop in content.Properties)
        {
            Assert.IsFalse(prop.IsDirty());
        }
    }

    /// <summary>
    /// Verifies that a mocked content instance is created and is not null.
    /// This test ensures that the content type service can be mocked and used to generate content for testing purposes.
    /// </summary>
    [Test]
    public void Can_Verify_Mocked_Content()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act

        // Assert
        Assert.That(content, Is.Not.Null);
    }

    /// <summary>
    /// Tests that the value of a property can be changed successfully.
    /// </summary>
    [Test]
    public void Can_Change_Property_Value()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.Properties["title"].SetValue("This is the new title");

        // Assert
        Assert.That(content.Properties.Any(), Is.True);
        Assert.That(content.Properties["title"], Is.Not.Null);
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
    }

    /// <summary>
    /// Tests that a property value can be set as a string on the content.
    /// </summary>
    [Test]
    public void Can_Set_Property_Value_As_String()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.SetValue("title", "This is the new title");

        // Assert
        Assert.That(content.Properties.Any(), Is.True);
        Assert.That(content.Properties["title"], Is.Not.Null);
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
    }

    /// <summary>
    /// Tests that a content item can be cloned with its identities reset.
    /// Ensures the cloned content has different Id and VersionId and that it does not have an identity.
    /// </summary>
    [Test]
    public void Can_Clone_Content_With_Reset_Identity()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        content.Id = 10;
        content.Key = new Guid("29181B97-CB8F-403F-86DE-5FEB497F4800");

        // Act
        var clone = content.DeepCloneWithResetIdentities();

        // Assert
        Assert.AreNotSame(clone, content);
        Assert.AreNotSame(clone.Id, content.Id);
        Assert.AreNotSame(clone.VersionId, content.VersionId);
        Assert.That(clone.HasIdentity, Is.False);

        Assert.AreNotSame(content.Properties, clone.Properties);
    }

    private static IProfilingLogger GetTestProfilingLogger()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<ProfilingLogger>();
        var profiler = new TestProfiler();
        return new ProfilingLogger(logger, profiler);
    }

    /// <summary>
    /// Tests the performance of deep cloning operations on content types, both with and without using the runtime cache.
    /// This test helps to evaluate the efficiency of the deep clone implementation under different caching scenarios.
    /// </summary>
    [Ignore("TODO - ignored test")]
    [Test]
    public void Can_Deep_Clone_Perf_Test()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.Id = 99;
        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        var i = 200;
        foreach (var property in content.Properties)
        {
            property.Id = ++i;
        }

        content.Id = 10;
        content.CreateDate = DateTime.UtcNow;
        content.CreatorId = 22;
        content.Key = Guid.NewGuid();
        content.Level = 3;
        content.Path = "-1,4,10";
        content.SortOrder = 5;
        content.TemplateId = 88;
        content.Trashed = false;
        content.UpdateDate = DateTime.UtcNow;
        content.WriterId = 23;

        var runtimeCache = new ObjectCacheAppCache();
        runtimeCache.Insert(content.Id.ToString(CultureInfo.InvariantCulture), () => content);

        var proflog = GetTestProfilingLogger();

        using (proflog.DebugDuration<ContentTests>("STARTING PERF TEST WITH RUNTIME CACHE"))
        {
            for (var j = 0; j < 1000; j++)
            {
                var clone = runtimeCache.Get(content.Id.ToString(CultureInfo.InvariantCulture));
            }
        }

        using (proflog.DebugDuration<ContentTests>("STARTING PERF TEST WITHOUT RUNTIME CACHE"))
        {
            for (var j = 0; j < 1000; j++)
            {
                var clone = (ContentType)contentType.DeepClone();
            }
        }
    }

    /// <summary>
    /// Tests that the Content object can be deeply cloned correctly.
    /// Ensures all properties and collections are cloned and not referenced.
    /// </summary>
    [Test]
    public void Can_Deep_Clone()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.Id = 99;
        contentType.Variations = ContentVariation.Culture;
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.SetCultureName("Hello", "en-US");
        content.SetCultureName("World", "es-ES");
        content.PublishCulture(CultureImpact.All, DateTime.UtcNow, _propertyEditorCollection);

        // should not try to clone something that's not Published or Unpublished
        // (and in fact it will not work)
        // but we cannot directly set the state to Published - hence this trick
        // content.ChangePublishedState(PublishedState.Publishing);
        content.ResetDirtyProperties(false); // => .Published

        var i = 200;
        foreach (var property in content.Properties)
        {
            property.Id = ++i;
        }

        content.Id = 10;
        content.CreateDate = DateTime.UtcNow;
        content.CreatorId = 22;
        content.Key = Guid.NewGuid();
        content.Level = 3;
        content.Path = "-1,4,10";
        content.SortOrder = 5;
        content.TemplateId = 88;
        content.Trashed = false;
        content.UpdateDate = DateTime.UtcNow;
        content.WriterId = 23;

        // Act
        var clone = (Content)content.DeepClone();

        // Assert
        Assert.AreNotSame(clone, content);
        Assert.AreEqual(clone, content);
        Assert.AreEqual(clone.Id, content.Id);
        Assert.AreEqual(clone.VersionId, content.VersionId);
        Assert.AreEqual(clone.ContentType, content.ContentType);
        Assert.AreEqual(clone.ContentTypeId, content.ContentTypeId);
        Assert.AreEqual(clone.CreateDate, content.CreateDate);
        Assert.AreEqual(clone.CreatorId, content.CreatorId);
        Assert.AreEqual(clone.Key, content.Key);
        Assert.AreEqual(clone.Level, content.Level);
        Assert.AreEqual(clone.Path, content.Path);
        Assert.AreEqual(clone.Published, content.Published);
        Assert.AreEqual(clone.PublishedState, content.PublishedState);
        Assert.AreEqual(clone.SortOrder, content.SortOrder);
        Assert.AreEqual(clone.PublishedState, content.PublishedState);
        Assert.AreNotSame(clone.TemplateId, content.TemplateId);
        Assert.AreEqual(clone.TemplateId, content.TemplateId);
        Assert.AreEqual(clone.Trashed, content.Trashed);
        Assert.AreEqual(clone.UpdateDate, content.UpdateDate);
        Assert.AreEqual(clone.VersionId, content.VersionId);
        Assert.AreEqual(clone.WriterId, content.WriterId);
        Assert.AreNotSame(clone.Properties, content.Properties);
        Assert.AreEqual(clone.Properties.Count(), content.Properties.Count());
        for (var index = 0; index < content.Properties.Count; index++)
        {
            Assert.AreNotSame(clone.Properties[index], content.Properties[index]);
            Assert.AreEqual(clone.Properties[index], content.Properties[index]);
        }

        Assert.AreNotSame(clone.PublishCultureInfos, content.PublishCultureInfos);
        Assert.AreEqual(clone.PublishCultureInfos.Count, content.PublishCultureInfos.Count);
        foreach (var key in content.PublishCultureInfos.Keys)
        {
            Assert.AreNotSame(clone.PublishCultureInfos[key], content.PublishCultureInfos[key]);
            Assert.AreEqual(clone.PublishCultureInfos[key], content.PublishCultureInfos[key]);
        }

        Assert.AreNotSame(clone.CultureInfos, content.CultureInfos);
        Assert.AreEqual(clone.CultureInfos.Count, content.CultureInfos.Count);
        foreach (var key in content.CultureInfos.Keys)
        {
            Assert.AreNotSame(clone.CultureInfos[key], content.CultureInfos[key]);
            Assert.AreEqual(clone.CultureInfos[key], content.CultureInfos[key]);
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.AreEqual(propertyInfo.GetValue(clone, null), propertyInfo.GetValue(content, null));
        }

        // Need to ensure the event handlers are wired
        var asDirty = (ICanBeDirty)clone;

        Assert.IsFalse(asDirty.IsPropertyDirty("Properties"));
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("blah")
            .Build();
        var newProperty = new PropertyBuilder()
            .WithId(1)
            .WithPropertyType(propertyType)
            .Build();
        newProperty.SetValue("blah");
        clone.Properties.Add(newProperty);

        Assert.IsTrue(asDirty.IsPropertyDirty("Properties"));
    }

    /// <summary>
    /// Tests that the content correctly remembers which properties are dirty after resetting dirty properties.
    /// </summary>
    [Test]
    public void Remember_Dirty_Properties()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.Id = 99;
        contentType.Variations = ContentVariation.Culture;
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.SetCultureName("Hello", "en-US");
        content.SetCultureName("World", "es-ES");
        content.PublishCulture(CultureImpact.All, DateTime.UtcNow, _propertyEditorCollection);

        var i = 200;
        foreach (var property in content.Properties)
        {
            property.Id = ++i;
        }

        content.Id = 10;
        content.CreateDate = DateTime.UtcNow;
        content.CreatorId = 22;
        content.Key = Guid.NewGuid();
        content.Level = 3;
        content.Path = "-1,4,10";
        content.SortOrder = 5;
        content.TemplateId = 88;
        content.Trashed = true;
        content.UpdateDate = DateTime.UtcNow;
        content.WriterId = 23;

        // Act
        content.ResetDirtyProperties();

        // Assert
        Assert.IsTrue(content.WasDirty());
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Id)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.CreateDate)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.CreatorId)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Key)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Level)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Path)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.SortOrder)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.TemplateId)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.Trashed)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.UpdateDate)));
        Assert.IsTrue(content.WasPropertyDirty(nameof(Content.WriterId)));
        foreach (var prop in content.Properties)
        {
            Assert.IsTrue(prop.WasDirty());
            Assert.IsTrue(prop.WasPropertyDirty("Id"));
        }

        Assert.IsTrue(content.WasPropertyDirty("CultureInfos"));
        foreach (var culture in content.CultureInfos)
        {
            Assert.IsTrue(culture.WasDirty());
            Assert.IsTrue(culture.WasPropertyDirty("Name"));
            Assert.IsTrue(culture.WasPropertyDirty("Date"));
        }

        Assert.IsTrue(content.WasPropertyDirty("PublishCultureInfos"));
        foreach (var culture in content.PublishCultureInfos)
        {
            Assert.IsTrue(culture.WasDirty());
            Assert.IsTrue(culture.WasPropertyDirty("Name"));
            Assert.IsTrue(culture.WasPropertyDirty("Date"));
        }
    }

    /// <summary>
    /// Tests that the Content object can be serialized to JSON without throwing an error.
    /// </summary>
    [Test]
    public void Can_Serialize_Without_Error()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.Id = 99;
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
        var i = 200;
        foreach (var property in content.Properties)
        {
            property.Id = ++i;
        }

        content.Id = 10;
        content.CreateDate = DateTime.UtcNow;
        content.CreatorId = 22;
        content.Key = Guid.NewGuid();
        content.Level = 3;
        content.Path = "-1,4,10";
        content.SortOrder = 5;
        content.TemplateId = 88;
        content.Trashed = false;
        content.UpdateDate = DateTime.UtcNow;
        content.WriterId = 23;

        var json = JsonSerializer.Serialize(content);
        Debug.Print(json);
    }

    /// <summary>
    /// Tests that the property value of a content item can be changed using an anonymous object.
    /// </summary>
    [Test]
    public void Can_Change_Property_Value_Through_Anonymous_Object()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.PropertyValues(new { title = "This is the new title" });

        // Assert
        Assert.That(content.Properties.Any(), Is.True);
        Assert.That(content.Properties["title"], Is.Not.Null);
        Assert.That(content.Properties["title"].Alias, Is.EqualTo("title"));
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("This is the new title"));
        Assert.That(content.Properties["description"].GetValue(), Is.EqualTo("This is the meta description for a textpage"));
    }

    /// <summary>
    /// Tests that the dirty property tracking on content works correctly.
    /// It verifies that after resetting dirty properties, changing a property marks it as dirty.
    /// </summary>
    [Test]
    public void Can_Verify_Dirty_Property_On_Content()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.ResetDirtyProperties();
        content.Name = "New Home";

        // Assert
        Assert.That(content.Name, Is.EqualTo("New Home"));
        Assert.That(content.IsPropertyDirty("Name"), Is.True);
    }

    /// <summary>
    /// Tests that a property group can be added to a content type.
    /// </summary>
    [Test]
    public void Can_Add_PropertyGroup_On_ContentType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        // Act
        contentType.PropertyGroups.Add(
            new PropertyGroup(true) { Alias = "testGroup", Name = "Test Group", SortOrder = 3 });

        // Assert
        Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(3));
    }

    /// <summary>
    /// Tests that a property group can be removed from a content type.
    /// </summary>
    [Test]
    public void Can_Remove_PropertyGroup_From_ContentType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties();

        // Act
        contentType.PropertyGroups.Remove("content");

        // Assert
        Assert.That(contentType.PropertyGroups.Count, Is.EqualTo(1));
        //// Assert.That(contentType.IsPropertyDirty("PropertyGroups"), Is.True);
    }

    /// <summary>
    /// Tests that a PropertyType can be added to a PropertyGroup on a ContentType.
    /// </summary>
    [Test]
    public void Can_Add_PropertyType_To_Group_On_ContentType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        // Act
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("subtitle")
            .WithName("Subtitle")
            .Build();
        contentType.PropertyGroups["content"].PropertyTypes.Add(propertyType);

        // Assert
        Assert.That(contentType.PropertyGroups["content"].PropertyTypes.Count, Is.EqualTo(3));
    }

    /// <summary>
    /// Tests that a new property can be added to a content instance using a new property type.
    /// </summary>
    [Test]
    public void Can_Add_New_Property_To_New_PropertyType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("subtitle")
            .WithName("Subtitle")
            .Build();
        contentType.PropertyGroups["content"].PropertyTypes.Add(propertyType);
        var newProperty = new Property(propertyType);
        newProperty.SetValue("This is a subtitle Test");
        content.Properties.Add(newProperty);

        // Assert
        Assert.That(content.Properties.Contains("subtitle"), Is.True);
        Assert.That(content.Properties["subtitle"].GetValue(), Is.EqualTo("This is a subtitle Test"));
    }

    /// <summary>
    /// Tests that a new property can be added to a new property type within a new property group.
    /// </summary>
    [Test]
    public void Can_Add_New_Property_To_New_PropertyType_In_New_PropertyGroup()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("subtitle")
            .WithName("Subtitle")
            .Build();
        var propertyGroup = new PropertyGroup(true) { Alias = "testGroup", Name = "Test Group", SortOrder = 3 };
        propertyGroup.PropertyTypes.Add(propertyType);
        contentType.PropertyGroups.Add(propertyGroup);
        var newProperty = new Property(propertyType);
        newProperty.SetValue("Subtitle Test");
        content.Properties.Add(newProperty);

        // Assert
        Assert.That(content.Properties.Count, Is.EqualTo(5));
        Assert.That(content.Properties["subtitle"].GetValue(), Is.EqualTo("Subtitle Test"));
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
    }

    /// <summary>
    /// Verifies that a <see cref="PropertyType"/> can be added and updated via the <c>Properties</c> collection of a <see cref="Content"/> object.
    /// Ensures that changes to the <see cref="PropertyType"/> are reflected in the content's properties, but not all <see cref="PropertyType"/> properties (such as <c>SortOrder</c>) are updated through the content object.
    /// </summary>
    [Test]
    public void Can_Update_PropertyType_Through_Content_Properties()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act - note that the PropertyType's properties like SortOrder is not updated through the Content object
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("title")
            .WithName("Title")
            .Build();
        content.Properties.Add(new Property(propertyType));

        // Assert
        Assert.That(content.Properties.Count, Is.EqualTo(4));
        Assert.That(contentType.PropertyTypes.First(x => x.Alias == "title").SortOrder, Is.EqualTo(1));
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
    }

    /// <summary>
    /// Tests that the content type of a content item can be changed and verifies the properties are updated accordingly.
    /// </summary>
    [Test]
    public void Can_Change_ContentType_On_Content()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.ChangeContentType(simpleContentType);

        // Assert
        Assert.That(content.Properties.Contains("author"), Is.True);

        // Note: There were 4 properties, after changing ContentType 1 has been added (no properties are deleted).
        Assert.That(content.Properties.Count, Is.EqualTo(5));
    }

    /// <summary>
    /// Tests that the content type of a content item can be changed and a property value can be set on the new content type.
    /// </summary>
    [Test]
    public void Can_Change_ContentType_On_Content_And_Set_Property_Value()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.ChangeContentType(simpleContentType);
        content.SetValue("author", "John Doe");

        // Assert
        Assert.That(content.Properties.Contains("author"), Is.True);
        Assert.That(content.Properties["author"].GetValue(), Is.EqualTo("John Doe"));
    }

    /// <summary>
    /// Tests that changing the content type on a content instance preserves the old properties and their values.
    /// </summary>
    [Test]
    public void Can_Change_ContentType_On_Content_And_Still_Get_Old_Properties()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        // Act
        content.ChangeContentType(simpleContentType);

        // Assert
        Assert.That(content.Properties.Contains("author"), Is.True);
        Assert.That(content.Properties.Contains("keywords"), Is.True);
        Assert.That(content.Properties.Contains("description"), Is.True);
        Assert.That(content.Properties["keywords"].GetValue(), Is.EqualTo("text,page,meta"));
        Assert.That(content.Properties["description"].GetValue(), Is.EqualTo("This is the meta description for a textpage"));
    }

    /// <summary>
    /// Tests that the content type can be changed on content and that old property types are cleared.
    /// </summary>
    [Test]
    [Ignore("Need to reimplement this logic for v8")]
    public void Can_Change_ContentType_On_Content_And_Clear_Old_PropertyTypes() => throw new NotImplementedException();

    //// Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>())).Returns(contentType);
    ////// Arrange
    //// var contentType = ContentTypeBuilder.CreateTextPageContentType();
    //// var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
    //// var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);
    ////// Act
    //// content.ChangeContentType(simpleContentType, true);
    ////// Assert
    //// Assert.That(content.Properties.Contains("author"), Is.True);
    //// Assert.That(content.Properties.Contains("keywords"), Is.False);
    //// Assert.That(content.Properties.Contains("description"), Is.False);
    [Test]
    public void Can_Verify_Content_Is_Published()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.ResetDirtyProperties();
        content.PublishedState = PublishedState.Publishing;

        Assert.IsFalse(content.IsPropertyDirty("Published"));
        Assert.IsFalse(content.Published);
        Assert.IsFalse(content.IsPropertyDirty("Name"));
        Assert.AreEqual(PublishedState.Publishing, content.PublishedState);

        // the repo would do
        content.Published = true;

        // and then
        Assert.IsTrue(content.IsPropertyDirty("Published"));
        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.IsPropertyDirty("Name"));
        Assert.AreEqual(PublishedState.Published, content.PublishedState);

        // and before returning,
        content.ResetDirtyProperties();

        // and then
        Assert.IsFalse(content.IsPropertyDirty("Published"));
        Assert.IsTrue(content.Published);
        Assert.IsFalse(content.IsPropertyDirty("Name"));
        Assert.AreEqual(PublishedState.Published, content.PublishedState);
    }

    /// <summary>
    /// Tests that adding a property group to a content type marks the entity as dirty.
    /// </summary>
    [Test]
    public void Adding_PropertyGroup_To_ContentType_Results_In_Dirty_Entity()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties();

        // Act
        var propertyGroup = new PropertyGroup(true) { Alias = "testGroup", Name = "Test Group", SortOrder = 3 };
        contentType.PropertyGroups.Add(propertyGroup);

        // Assert
        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.PropertyGroups.Any(x => x.Name == "Test Group"), Is.True);
        //// Assert.That(contentType.IsPropertyDirty("PropertyGroups"), Is.True);
    }

    /// <summary>
    /// Tests that after committing changes, the WasDirty flag is set to true while IsDirty is false.
    /// </summary>
    [Test]
    public void After_Committing_Changes_Was_Dirty_Is_True()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties(); // reset

        // Act
        contentType.Alias = "newAlias";
        contentType.ResetDirtyProperties(); // this would be like committing the entity

        // Assert
        Assert.That(contentType.IsDirty(), Is.False);
        Assert.That(contentType.WasDirty(), Is.True);
        Assert.That(contentType.WasPropertyDirty("Alias"), Is.True);
    }

    /// <summary>
    /// Verifies that after making a change to a property and committing (resetting dirty properties),
    /// the <c>WasDirty</c> flag is set to true on the changed property, indicating it was modified
    /// before the commit.
    /// </summary>
    [Test]
    public void After_Committing_Changes_Was_Dirty_Is_True_On_Changed_Property()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties(); // reset
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "test", -1);
        content.ResetDirtyProperties();

        // Act
        content.SetValue("title", "new title");
        Assert.That(content.IsEntityDirty(), Is.False);
        Assert.That(content.IsDirty(), Is.True);
        Assert.That(content.IsPropertyDirty("title"), Is.True);
        Assert.That(content.IsAnyUserPropertyDirty(), Is.True);
        Assert.That(content.GetDirtyUserProperties().Count(), Is.EqualTo(1));
        Assert.That(content.Properties[0].IsDirty(), Is.True);
        Assert.That(content.Properties["title"].IsDirty(), Is.True);

        content.ResetDirtyProperties(); // this would be like committing the entity

        // Assert
        Assert.That(content.WasDirty(), Is.True);
        Assert.That(content.Properties[0].WasDirty(), Is.True);

        Assert.That(content.WasPropertyDirty("title"), Is.True);
        Assert.That(content.Properties["title"].IsDirty(), Is.False);
        Assert.That(content.Properties["title"].WasDirty(), Is.True);
    }

    /// <summary>
    /// Tests that if the content has not been committed, the WasDirty flag is false.
    /// </summary>
    [Test]
    public void If_Not_Committed_Was_Dirty_Is_False()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        // Act
        contentType.Alias = "newAlias";

        // Assert
        Assert.That(contentType.IsDirty(), Is.True);
        Assert.That(contentType.WasDirty(), Is.False);
    }

    /// <summary>
    /// Tests that the content type correctly detects when a property has been removed.
    /// </summary>
    [Test]
    public void Detect_That_A_Property_Is_Removed()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Assert.That(contentType.WasPropertyDirty("HasPropertyTypeBeenRemoved"), Is.False);

        // Act
        contentType.RemovePropertyType("title");

        // Assert
        Assert.That(contentType.IsPropertyDirty("HasPropertyTypeBeenRemoved"), Is.True);
    }

    /// <summary>
    /// Tests that adding a PropertyType to a PropertyGroup on a ContentType marks the PropertyGroup as dirty.
    /// </summary>
    [Test]
    public void Adding_PropertyType_To_PropertyGroup_On_ContentType_Results_In_Dirty_Entity()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties();

        // Act
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("subtitle")
            .WithName("Subtitle")
            .Build();
        contentType.PropertyGroups["content"].PropertyTypes.Add(propertyType);

        // Assert
        Assert.That(contentType.PropertyGroups["content"].IsDirty(), Is.True);
        Assert.That(contentType.PropertyGroups["content"].IsPropertyDirty("PropertyTypes"), Is.True);
        Assert.That(contentType.PropertyGroups.Any(x => x.IsDirty()), Is.True);
    }

    /// <summary>
    /// Tests that a composite content type collection can be composed correctly by adding another content type.
    /// </summary>
    [Test]
    public void Can_Compose_Composite_ContentType_Collection()
    {
        // Arrange
        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("coauthor")
            .WithName("Co-author")
            .Build();
        var simple2ContentType = ContentTypeBuilder.CreateSimpleContentType(
            "anotherSimple",
            "Another Simple Page",
            propertyTypeCollection: new PropertyTypeCollection(true, new List<PropertyType> { propertyType }));

        // Act
        var added = simpleContentType.AddContentType(simple2ContentType);
        var compositionPropertyGroups = simpleContentType.CompositionPropertyGroups;
        var compositionPropertyTypes = simpleContentType.CompositionPropertyTypes;

        // Assert
        Assert.That(added, Is.True);
        Assert.That(compositionPropertyGroups.Count(), Is.EqualTo(1));
        Assert.That(compositionPropertyTypes.Count(), Is.EqualTo(4));
    }

    /// <summary>
    /// Tests that nested composite content types can be composed correctly,
    /// ensuring property groups and types are aggregated as expected.
    /// </summary>
    [Test]
    public void Can_Compose_Nested_Composite_ContentType_Collection()
    {
        // Arrange
        var metaContentType = ContentTypeBuilder.CreateMetaContentType();
        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType();
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("coauthor")
            .WithName("Co-author")
            .Build();
        var simple2ContentType = ContentTypeBuilder.CreateSimpleContentType(
            "anotherSimple",
            "Another Simple Page",
            propertyTypeCollection: new PropertyTypeCollection(true, new List<PropertyType> { propertyType }));

        // Act
        var addedMeta = simple2ContentType.AddContentType(metaContentType);
        var added = simpleContentType.AddContentType(simple2ContentType);
        var compositionPropertyGroups = simpleContentType.CompositionPropertyGroups;
        var compositionPropertyTypes = simpleContentType.CompositionPropertyTypes;

        // Assert
        Assert.That(addedMeta, Is.True);
        Assert.That(added, Is.True);
        Assert.That(compositionPropertyGroups.Count(), Is.EqualTo(2));
        Assert.That(compositionPropertyTypes.Count(), Is.EqualTo(6));
        Assert.That(simpleContentType.ContentTypeCompositionExists("meta"), Is.True);
    }

    /// <summary>
    /// Verifies that the content type composition logic correctly prevents the creation of circular dependencies
    /// when adding content types as compositions, ensuring the integrity of the content type hierarchy.
    /// This test checks that attempts to introduce cycles are detected and rejected, while valid compositions succeed.
    /// </summary>
    [Test]
    public void Can_Avoid_Circular_Dependencies_In_Composition()
    {
        var textPage = ContentTypeBuilder.CreateTextPageContentType();
        var parent = ContentTypeBuilder.CreateSimpleContentType("parent", "Parent", null, randomizeAliases: true);
        var meta = ContentTypeBuilder.CreateMetaContentType();
        var propertyType1 = new PropertyTypeBuilder()
            .WithAlias("coauthor")
            .WithName("Co-author")
            .Build();
        var mixin1 = ContentTypeBuilder.CreateSimpleContentType(
            "mixin1",
            "Mixin1",
            propertyTypeCollection: new PropertyTypeCollection(true, new List<PropertyType> { propertyType1 }));
        var propertyType2 = new PropertyTypeBuilder()
            .WithAlias("author")
            .WithName("Author")
            .Build();
        var mixin2 = ContentTypeBuilder.CreateSimpleContentType(
            "mixin2",
            "Mixin2",
            propertyTypeCollection: new PropertyTypeCollection(true, new List<PropertyType> { propertyType2 }));

        // Act
        var addedMetaMixin2 = mixin2.AddContentType(meta);
        var addedMixin2 = mixin1.AddContentType(mixin2);
        var addedMeta = parent.AddContentType(meta);

        var addedMixin1 = parent.AddContentType(mixin1);

        var addedMixin1Textpage = textPage.AddContentType(mixin1);
        var addedTextpageParent = parent.AddContentType(textPage);

        var aliases = textPage.CompositionAliases();
        var propertyTypes = textPage.CompositionPropertyTypes;
        var propertyGroups = textPage.CompositionPropertyGroups;

        // Assert
        Assert.That(mixin2.ContentTypeCompositionExists("meta"), Is.True);
        Assert.That(mixin1.ContentTypeCompositionExists("meta"), Is.True);
        Assert.That(parent.ContentTypeCompositionExists("meta"), Is.True);
        Assert.That(textPage.ContentTypeCompositionExists("meta"), Is.True);

        Assert.That(aliases.Count(), Is.EqualTo(3));
        Assert.That(propertyTypes.Count(), Is.EqualTo(8));
        Assert.That(propertyGroups.Count(), Is.EqualTo(2));

        Assert.That(addedMeta, Is.True);
        Assert.That(addedMetaMixin2, Is.True);
        Assert.That(addedMixin2, Is.True);
        Assert.That(addedMixin1, Is.False);
        Assert.That(addedMixin1Textpage, Is.True);
        Assert.That(addedTextpageParent, Is.False);
    }
}
