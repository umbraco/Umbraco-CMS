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

[TestFixture]
public class ContentTests
{
    private readonly IContentTypeService _contentTypeService = Mock.Of<IContentTypeService>();

    private readonly PropertyEditorCollection _propertyEditorCollection = new (new DataEditorCollection(() => []));

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

        Assert.That(content.IsPropertyDirty("CultureInfos"), Is.False); // hasn't been changed

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        Assert.That(
            content.IsPropertyDirty("CultureInfos"), Is.True); // now it will be changed since the collection has changed
        var frCultureName = content.CultureInfos[langFr];
        Assert.That(frCultureName.IsPropertyDirty("Date"), Is.True);

        content.ResetDirtyProperties();
        frCultureName.ResetDirtyProperties();

        Assert.That(content.IsPropertyDirty("CultureInfos"), Is.False); // it's been reset
        Assert.That(content.WasPropertyDirty("CultureInfos"), Is.True);

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName(newName, langFr);

        // dirty is only true if we updated the name
        Assert.That(frCultureName.IsPropertyDirty("Date"), Is.EqualTo(expectedDirty));
        Assert.That(content.IsPropertyDirty("CultureInfos"), Is.EqualTo(expectedDirty));
    }

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

        Assert.That(content.IsPropertyDirty("PublishCultureInfos"), Is.False); // hasn't been changed

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection); // we've set the name, now we're publishing it
        Assert.That(
            content.IsPropertyDirty("PublishCultureInfos"), Is.True); // now it will be changed since the collection has changed
        var frCultureName = content.PublishCultureInfos[langFr];
        Assert.That(frCultureName.IsPropertyDirty("Date"), Is.True);

        content.ResetDirtyProperties();
        frCultureName.ResetDirtyProperties();

        Assert.That(content.IsPropertyDirty("PublishCultureInfos"), Is.False); // it's been reset
        Assert.That(content.WasPropertyDirty("PublishCultureInfos"), Is.True);

        Thread.Sleep(500); // The "Date" wont be dirty if the test runs too fast since it will be the same date
        content.SetCultureName("name-fr", langFr);
        content.PublishCulture(CultureImpact.Explicit(langFr, false), DateTime.UtcNow, _propertyEditorCollection); // we've set the name, now we're publishing it
        Assert.That(frCultureName.IsPropertyDirty("Date"), Is.True);
        Assert.That(content.IsPropertyDirty("PublishCultureInfos"), Is.True); // it's true now since we've updated a name
    }

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

        Assert.That(nonGrouped.Count(), Is.EqualTo(2));
        Assert.That(content.Properties.Count(), Is.EqualTo(5));
    }

    [Test]
    public void All_Dirty_Properties_Get_Reset()
    {
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        Mock.Get(_contentTypeService).As<IContentTypeBaseService>().Setup(x => x.Get(It.IsAny<int>()))
            .Returns(contentType);

        var content = ContentBuilder.CreateTextpageContent(contentType, "Textpage", -1);

        content.ResetDirtyProperties(false);

        Assert.That(content.IsDirty(), Is.False);
        foreach (var prop in content.Properties)
        {
            Assert.That(prop.IsDirty(), Is.False);
        }
    }

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
        Assert.That(content, Is.Not.SameAs(clone));
        Assert.That(content.Id, Is.Not.EqualTo(clone.Id));
        Assert.That(content.VersionId, Is.Not.EqualTo(clone.VersionId));
        Assert.That(clone.HasIdentity, Is.False);

        Assert.That(clone.Properties, Is.Not.SameAs(content.Properties));
    }

    private static IProfilingLogger GetTestProfilingLogger()
    {
        var logger = NullLoggerFactory.Instance.CreateLogger<ProfilingLogger>();
        var profiler = new TestProfiler();
        return new ProfilingLogger(logger, profiler);
    }

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
        Assert.That(content, Is.Not.SameAs(clone));
        Assert.That(content, Is.EqualTo(clone));
        Assert.That(content.Id, Is.EqualTo(clone.Id));
        Assert.That(content.VersionId, Is.EqualTo(clone.VersionId));
        Assert.That(content.ContentType, Is.EqualTo(clone.ContentType));
        Assert.That(content.ContentTypeId, Is.EqualTo(clone.ContentTypeId));
        Assert.That(content.CreateDate, Is.EqualTo(clone.CreateDate));
        Assert.That(content.CreatorId, Is.EqualTo(clone.CreatorId));
        Assert.That(content.Key, Is.EqualTo(clone.Key));
        Assert.That(content.Level, Is.EqualTo(clone.Level));
        Assert.That(content.Path, Is.EqualTo(clone.Path));
        Assert.That(content.Published, Is.EqualTo(clone.Published));
        Assert.That(content.PublishedState, Is.EqualTo(clone.PublishedState));
        Assert.That(content.SortOrder, Is.EqualTo(clone.SortOrder));
        Assert.That(content.PublishedState, Is.EqualTo(clone.PublishedState));
        Assert.That(content.TemplateId, Is.Not.EqualTo(clone.TemplateId));
        Assert.That(content.TemplateId, Is.EqualTo(clone.TemplateId));
        Assert.That(content.Trashed, Is.EqualTo(clone.Trashed));
        Assert.That(content.UpdateDate, Is.EqualTo(clone.UpdateDate));
        Assert.That(content.VersionId, Is.EqualTo(clone.VersionId));
        Assert.That(content.WriterId, Is.EqualTo(clone.WriterId));
        Assert.That(content.Properties, Is.Not.SameAs(clone.Properties));
        Assert.That(content.Properties.Count(), Is.EqualTo(clone.Properties.Count()));
        for (var index = 0; index < content.Properties.Count; index++)
        {
            Assert.That(content.Properties[index], Is.Not.SameAs(clone.Properties[index]));
            Assert.That(content.Properties[index], Is.EqualTo(clone.Properties[index]));
        }

        Assert.That(content.PublishCultureInfos, Is.Not.SameAs(clone.PublishCultureInfos));
        Assert.That(content.PublishCultureInfos, Has.Count.EqualTo(clone.PublishCultureInfos.Count));
        foreach (var key in content.PublishCultureInfos.Keys)
        {
            Assert.That(content.PublishCultureInfos[key], Is.Not.SameAs(clone.PublishCultureInfos[key]));
            Assert.That(content.PublishCultureInfos[key], Is.EqualTo(clone.PublishCultureInfos[key]));
        }

        Assert.That(content.CultureInfos, Is.Not.SameAs(clone.CultureInfos));
        Assert.That(content.CultureInfos, Has.Count.EqualTo(clone.CultureInfos.Count));
        foreach (var key in content.CultureInfos.Keys)
        {
            Assert.That(content.CultureInfos[key], Is.Not.SameAs(clone.CultureInfos[key]));
            Assert.That(content.CultureInfos[key], Is.EqualTo(clone.CultureInfos[key]));
        }

        // This double verifies by reflection
        var allProps = clone.GetType().GetProperties();
        foreach (var propertyInfo in allProps)
        {
            Assert.That(propertyInfo.GetValue(content, null), Is.EqualTo(propertyInfo.GetValue(clone, null)));
        }

        // Need to ensure the event handlers are wired
        var asDirty = (ICanBeDirty)clone;

        Assert.That(asDirty.IsPropertyDirty("Properties"), Is.False);
        var propertyType = new PropertyTypeBuilder()
            .WithAlias("blah")
            .Build();
        var newProperty = new PropertyBuilder()
            .WithId(1)
            .WithPropertyType(propertyType)
            .Build();
        newProperty.SetValue("blah");
        clone.Properties.Add(newProperty);

        Assert.That(asDirty.IsPropertyDirty("Properties"), Is.True);
    }

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
        Assert.That(content.WasDirty(), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.Id)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.CreateDate)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.CreatorId)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.Key)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.Level)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.Path)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.SortOrder)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.TemplateId)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.Trashed)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.UpdateDate)), Is.True);
        Assert.That(content.WasPropertyDirty(nameof(Content.WriterId)), Is.True);
        foreach (var prop in content.Properties)
        {
            Assert.That(prop.WasDirty(), Is.True);
            Assert.That(prop.WasPropertyDirty("Id"), Is.True);
        }

        Assert.That(content.WasPropertyDirty("CultureInfos"), Is.True);
        foreach (var culture in content.CultureInfos)
        {
            Assert.That(culture.WasDirty(), Is.True);
            Assert.That(culture.WasPropertyDirty("Name"), Is.True);
            Assert.That(culture.WasPropertyDirty("Date"), Is.True);
        }

        Assert.That(content.WasPropertyDirty("PublishCultureInfos"), Is.True);
        foreach (var culture in content.PublishCultureInfos)
        {
            Assert.That(culture.WasDirty(), Is.True);
            Assert.That(culture.WasPropertyDirty("Name"), Is.True);
            Assert.That(culture.WasPropertyDirty("Date"), Is.True);
        }
    }

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

    [Test]
    public void Can_Add_PropertyGroup_On_ContentType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();

        // Act
        contentType.PropertyGroups.Add(
            new PropertyGroup(true) { Alias = "testGroup", Name = "Test Group", SortOrder = 3 });

        // Assert
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(3));
    }

    [Test]
    public void Can_Remove_PropertyGroup_From_ContentType()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateTextPageContentType();
        contentType.ResetDirtyProperties();

        // Act
        contentType.PropertyGroups.Remove("content");

        // Assert
        Assert.That(contentType.PropertyGroups, Has.Count.EqualTo(1));
        //// Assert.That(contentType.IsPropertyDirty("PropertyGroups"), Is.True);
    }

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
        Assert.That(contentType.PropertyGroups["content"].PropertyTypes, Has.Count.EqualTo(3));
    }

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
        Assert.That(content.Properties, Has.Count.EqualTo(5));
        Assert.That(content.Properties["subtitle"].GetValue(), Is.EqualTo("Subtitle Test"));
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
    }

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
        Assert.That(content.Properties, Has.Count.EqualTo(4));
        Assert.That(contentType.PropertyTypes.First(x => x.Alias == "title").SortOrder, Is.EqualTo(1));
        Assert.That(content.Properties["title"].GetValue(), Is.EqualTo("Textpage textpage"));
    }

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
        Assert.That(content.Properties, Has.Count.EqualTo(5));
    }

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

        Assert.That(content.IsPropertyDirty("Published"), Is.False);
        Assert.That(content.Published, Is.False);
        Assert.That(content.IsPropertyDirty("Name"), Is.False);
        Assert.That(content.PublishedState, Is.EqualTo(PublishedState.Publishing));

        // the repo would do
        content.Published = true;

        // and then
        Assert.That(content.IsPropertyDirty("Published"), Is.True);
        Assert.That(content.Published, Is.True);
        Assert.That(content.IsPropertyDirty("Name"), Is.False);
        Assert.That(content.PublishedState, Is.EqualTo(PublishedState.Published));

        // and before returning,
        content.ResetDirtyProperties();

        // and then
        Assert.That(content.IsPropertyDirty("Published"), Is.False);
        Assert.That(content.Published, Is.True);
        Assert.That(content.IsPropertyDirty("Name"), Is.False);
        Assert.That(content.PublishedState, Is.EqualTo(PublishedState.Published));
    }

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
