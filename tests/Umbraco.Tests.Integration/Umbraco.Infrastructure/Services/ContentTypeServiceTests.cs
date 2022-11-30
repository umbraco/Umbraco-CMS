// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
public class ContentTypeServiceTests : UmbracoIntegrationTest
{
    private IFileService FileService => GetRequiredService<IFileService>();

    private ContentService ContentService => (ContentService)GetRequiredService<IContentService>();

    private IDataTypeService DataTypeService => GetRequiredService<IDataTypeService>();

    private ContentTypeService ContentTypeService => (ContentTypeService)GetRequiredService<IContentTypeService>();

    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.AddNotificationHandler<ContentMovedToRecycleBinNotification, ContentNotificationHandler>();
        builder.AddNotificationHandler<ContentTypeDeletedNotification, ContentTypeNotificationHandler>();
    }

    [Test]
    public void CanSaveAndGetIsElement()
    {
        // create content type with a property type that varies by culture
        IContentType contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Variations = ContentVariation.Nothing;
        var contentCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext)
            {
                Alias = "title",
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88,
                Variations = ContentVariation.Nothing
            }
        };
        contentType.PropertyGroups.Add(new PropertyGroup(contentCollection)
        {
            Alias = "content",
            Name = "Content",
            SortOrder = 1
        });
        ContentTypeService.Save(contentType);

        contentType = ContentTypeService.Get(contentType.Id);
        Assert.IsFalse(contentType.IsElement);

        contentType.IsElement = true;
        ContentTypeService.Save(contentType);

        contentType = ContentTypeService.Get(contentType.Id);
        Assert.IsTrue(contentType.IsElement);
    }

    [Test]
    public void Deleting_Content_Type_With_Hierarchy_Of_Content_Items_Moves_Orphaned_Content_To_Recycle_Bin()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        IContentType contentType1 =
            ContentTypeBuilder.CreateSimpleContentType("test1", "Test1", defaultTemplateId: template.Id);
        FileService.SaveTemplate(contentType1.DefaultTemplate);
        ContentTypeService.Save(contentType1);
        IContentType contentType2 =
            ContentTypeBuilder.CreateSimpleContentType("test2", "Test2", defaultTemplateId: template.Id);
        FileService.SaveTemplate(contentType2.DefaultTemplate);
        ContentTypeService.Save(contentType2);
        IContentType contentType3 =
            ContentTypeBuilder.CreateSimpleContentType("test3", "Test3", defaultTemplateId: template.Id);
        FileService.SaveTemplate(contentType3.DefaultTemplate);
        ContentTypeService.Save(contentType3);

        IContentType[] contentTypes = { contentType1, contentType2, contentType3 };
        var parentId = -1;

        var ids = new List<int>();

        for (var i = 0; i < 2; i++)
        {
            for (var index = 0; index < contentTypes.Length; index++)
            {
                var contentType = contentTypes[index];
                var contentItem =
                    ContentBuilder.CreateSimpleContent(contentType, "MyName_" + index + "_" + i, parentId);
                ContentService.Save(contentItem);
                ContentService.SaveAndPublish(contentItem);
                parentId = contentItem.Id;

                ids.Add(contentItem.Id);
            }
        }

        // delete the first content type, all other content of different content types should be in the recycle bin
        ContentTypeService.Delete(contentTypes[0]);

        var found = ContentService.GetByIds(ids);

        Assert.AreEqual(4, found.Count());
        foreach (var content in found)
        {
            Assert.IsTrue(content.Trashed);
        }
    }

    [Test]
    public void Deleting_Content_Types_With_Hierarchy_Of_Content_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items_1()
    {
        ContentNotificationHandler.MovedContentToRecycleBin = MovedContentToRecycleBin;

        try
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            IContentType contentType1 =
                ContentTypeBuilder.CreateSimpleContentType("test1", "Test1", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType1.DefaultTemplate);
            ContentTypeService.Save(contentType1);
            IContentType contentType2 =
                ContentTypeBuilder.CreateSimpleContentType("test2", "Test2", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType2.DefaultTemplate);
            ContentTypeService.Save(contentType2);
            IContentType contentType3 =
                ContentTypeBuilder.CreateSimpleContentType("test3", "Test3", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType3.DefaultTemplate);
            ContentTypeService.Save(contentType3);

            IContentType[] contentTypes = { contentType1, contentType2, contentType3 };
            var parentId = -1;

            for (var i = 0; i < 2; i++)
            {
                for (var index = 0; index < contentTypes.Length; index++)
                {
                    var contentType = contentTypes[index];
                    var contentItem =
                        ContentBuilder.CreateSimpleContent(contentType, "MyName_" + index + "_" + i, parentId);
                    ContentService.Save(contentItem);
                    ContentService.SaveAndPublish(contentItem);
                    parentId = contentItem.Id;
                }
            }

            foreach (var contentType in contentTypes.Reverse())
            {
                ContentTypeService.Delete(contentType);
            }
        }
        finally
        {
            ContentNotificationHandler.MovedContentToRecycleBin = null;
        }
    }

    [Test]
    public void Deleting_Content_Types_With_Hierarchy_Of_Content_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items_2()
    {
        ContentNotificationHandler.MovedContentToRecycleBin = MovedContentToRecycleBin;

        try
        {
            var template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);

            IContentType contentType1 =
                ContentTypeBuilder.CreateSimpleContentType("test1", "Test1", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType1.DefaultTemplate);
            ContentTypeService.Save(contentType1);
            IContentType contentType2 =
                ContentTypeBuilder.CreateSimpleContentType("test2", "Test2", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType2.DefaultTemplate);
            ContentTypeService.Save(contentType2);
            IContentType contentType3 =
                ContentTypeBuilder.CreateSimpleContentType("test3", "Test3", defaultTemplateId: template.Id);
            FileService.SaveTemplate(contentType3.DefaultTemplate);
            ContentTypeService.Save(contentType3);

            var root = ContentBuilder.CreateSimpleContent(contentType1, "Root");
            ContentService.Save(root);
            ContentService.SaveAndPublish(root);

            var level1 = ContentBuilder.CreateSimpleContent(contentType2, "L1", root.Id);
            ContentService.Save(level1);
            ContentService.SaveAndPublish(level1);

            for (var i = 0; i < 2; i++)
            {
                var level3 = ContentBuilder.CreateSimpleContent(contentType3, "L2" + i, level1.Id);
                ContentService.Save(level3);
                ContentService.SaveAndPublish(level3);
            }

            ContentTypeService.Delete(contentType1);
        }
        finally
        {
            ContentNotificationHandler.MovedContentToRecycleBin = null;
        }
    }

    private void MovedContentToRecycleBin(ContentMovedToRecycleBinNotification notification)
    {
        foreach (var item in notification.MoveInfoCollection)
        {
            // if this item doesn't exist then Fail!
            var exists = ContentService.GetById(item.Entity.Id);
            if (exists == null)
            {
                Assert.Fail("The item doesn't exist");
            }
        }
    }

    [Test]
    public void Deleting_PropertyType_Removes_The_Property_From_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        IContentType contentType1 = ContentTypeBuilder.CreateTextPageContentType("test1", "Test1", template.Id);
        FileService.SaveTemplate(contentType1.DefaultTemplate);
        ContentTypeService.Save(contentType1);
        IContent contentItem = ContentBuilder.CreateTextpageContent(contentType1, "Testing", -1);
        ContentService.SaveAndPublish(contentItem);
        var initProps = contentItem.Properties.Count;

        // remove a property
        contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
        ContentTypeService.Save(contentType1);

        // re-load it from the db
        contentItem = ContentService.GetById(contentItem.Id);

        Assert.AreEqual(initProps - 1, contentItem.Properties.Count);
    }

    [Test]
    public void Get_Descendants()
    {
        // Arrange
        var contentTypeService = ContentTypeService;
        var hierarchy = CreateContentTypeHierarchy();
        contentTypeService.Save(hierarchy, 0); // ensure they are saved!
        var master = hierarchy.First();

        // Act
        var descendants = contentTypeService.GetDescendants(master.Id, false);

        // Assert
        Assert.AreEqual(10, descendants.Count());
    }

    [Test]
    public void Get_Descendants_And_Self()
    {
        // Arrange
        var contentTypeService = ContentTypeService;
        var hierarchy = CreateContentTypeHierarchy();
        contentTypeService.Save(hierarchy, 0); // ensure they are saved!
        var master = hierarchy.First();

        // Act
        var descendants = contentTypeService.GetDescendants(master.Id, true);

        // Assert
        Assert.AreEqual(11, descendants.Count());
    }

    [Test]
    public void Can_Bulk_Save_New_Hierarchy_Content_Types()
    {
        // Arrange
        var contentTypeService = ContentTypeService;
        var hierarchy = CreateContentTypeHierarchy();

        // Act
        contentTypeService.Save(hierarchy, 0);

        Assert.That(hierarchy.Any(), Is.True);
        Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);

        // all parent ids should be ok, they are lazy and if they equal zero an exception will be thrown
        Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));
        for (var i = 0; i < hierarchy.Count(); i++)
        {
            if (i == 0)
            {
                continue;
            }

            Assert.AreEqual(hierarchy.ElementAt(i).ParentId, hierarchy.ElementAt(i - 1).Id);
        }
    }

    [Test]
    public void Can_Save_ContentType_Structure_And_Create_Content_Based_On_It()
    {
        // Arrange
        var cs = ContentService;
        var cts = ContentTypeService;
        var dtdYesNo = DataTypeService.GetDataType(-49);
        var ctBase = new ContentType(ShortStringHelper, -1)
        {
            Name = "Base",
            Alias = "Base",
            Icon = "folder.gif",
            Thumbnail = "folder.png"
        };
        ctBase.AddPropertyType(new PropertyType(ShortStringHelper, dtdYesNo, Constants.Conventions.Content.NaviHide)
        {
            Name = "Hide From Navigation"
        });
        /*,"Navigation"*/
        cts.Save(ctBase);

        const string contentTypeAlias = "HomePage";
        var ctHomePage = new ContentType(ShortStringHelper, ctBase, contentTypeAlias)
        {
            Name = "Home Page",
            Alias = contentTypeAlias,
            Icon = "settingDomain.gif",
            Thumbnail = "folder.png",
            AllowedAsRoot = true
        };
        ctHomePage.AddPropertyType(
            new PropertyType(ShortStringHelper, dtdYesNo, "someProperty") { Name = "Some property" });
        /*,"Navigation"*/
        cts.Save(ctHomePage);

        // Act
        var homeDoc = cs.Create("Home Page", -1, contentTypeAlias);
        cs.SaveAndPublish(homeDoc);

        // Assert
        Assert.That(ctBase.HasIdentity, Is.True);
        Assert.That(ctHomePage.HasIdentity, Is.True);
        Assert.That(homeDoc.HasIdentity, Is.True);
        Assert.That(homeDoc.ContentTypeId, Is.EqualTo(ctHomePage.Id));
    }

    [Test]
    public void Create_Content_Type_Ensures_Sort_Orders()
    {
        var contentType = new ContentType(ShortStringHelper, -1)
        {
            Alias = "test",
            Name = "Test",
            Description = "ContentType used for simple text pages",
            Icon = ".sprTreeDoc3",
            Thumbnail = "doc2.png",
            SortOrder = 1,
            CreatorId = 0,
            Trashed = false
        };

        contentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            { Name = "Title", Description = string.Empty, Mandatory = false, DataTypeId = -88 });
        contentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext,
                "bodyText")
            { Name = "Body Text", Description = string.Empty, Mandatory = false, DataTypeId = -87 });
        contentType.AddPropertyType(
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            { Name = "Author", Description = "Name of the author", Mandatory = false, DataTypeId = -88 });

        ContentTypeService.Save(contentType);

        var sortOrders = contentType.PropertyTypes.Select(x => x.SortOrder).ToArray();

        Assert.AreEqual(1, sortOrders.Count(x => x == 0));
        Assert.AreEqual(1, sortOrders.Count(x => x == 1));
        Assert.AreEqual(1, sortOrders.Count(x => x == 2));
    }

    [Test]
    public void Can_Create_And_Save_ContentType_Composition()
    {
        /*
         * Global
         * - Components
         * - Category
         */
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var global = ContentTypeBuilder.CreateSimpleContentType("global", "Global", defaultTemplateId: template.Id);
        ContentTypeService.Save(global);

        var components = ContentTypeBuilder.CreateSimpleContentType("components", "Components", global,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(components);

        var component = ContentTypeBuilder.CreateSimpleContentType("component", "Component", components,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(component);

        var category = ContentTypeBuilder.CreateSimpleContentType("category", "Category", global,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(category);

        var success = category.AddContentType(component);

        Assert.That(success, Is.False);
    }

    [Test]
    public void Can_Delete_Parent_ContentType_When_Child_Has_Content()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page", randomizeAliases: true,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        var childContentType = ContentTypeBuilder.CreateSimpleContentType("childPage", "Child Page", contentType,
            randomizeAliases: true, propertyGroupAlias: "childContent", propertyGroupName: "Child Content",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(childContentType);
        var content = ContentService.Create("Page 1", -1, childContentType.Alias);
        ContentService.Save(content);

        ContentTypeService.Delete(contentType);

        Assert.IsNotNull(content.Id);
        Assert.AreNotEqual(0, content.Id);
        Assert.IsNotNull(childContentType.Id);
        Assert.AreNotEqual(0, childContentType.Id);
        Assert.IsNotNull(contentType.Id);
        Assert.AreNotEqual(0, contentType.Id);
        var deletedContent = ContentService.GetById(content.Id);
        var deletedChildContentType = ContentTypeService.Get(childContentType.Id);
        var deletedContentType = ContentTypeService.Get(contentType.Id);

        Assert.IsNull(deletedChildContentType);
        Assert.IsNull(deletedContent);
        Assert.IsNull(deletedContentType);
    }

    [Test]
    public void Can_Create_Container()
    {
        // Arrange
        var cts = ContentTypeService;

        // Act
        var container = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container1" };
        cts.SaveContainer(container);

        // Assert
        var createdContainer = cts.GetContainer(container.Id);
        Assert.IsNotNull(createdContainer);
    }

    [Test]
    public void Can_Get_All_Containers()
    {
        // Arrange
        var cts = ContentTypeService;

        // Act
        var container1 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container1" };
        cts.SaveContainer(container1);

        var container2 = new EntityContainer(Constants.ObjectTypes.DocumentType) { Name = "container2" };
        cts.SaveContainer(container2);

        // Assert
        var containers = cts.GetContainers(new int[0]);
        Assert.AreEqual(2, containers.Count());
    }

    [Test]
    public void Deleting_ContentType_Sends_Correct_Number_Of_DeletedEntities_In_Events()
    {
        var deletedEntities = 0;

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);

        ContentTypeNotificationHandler.Deleted +=
            notification => deletedEntities += notification.DeletedEntities.Count();

        ContentTypeService.Delete(contentType);

        Assert.AreEqual(deletedEntities, 1);
    }

    [Test]
    public void Deleting_Multiple_ContentTypes_Sends_Correct_Number_Of_DeletedEntities_In_Events()
    {
        var deletedEntities = 0;

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        var contentType2 =
            ContentTypeBuilder.CreateSimpleContentType("otherPage", "Other page", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType2);

        ContentTypeNotificationHandler.Deleted +=
            notification => deletedEntities += notification.DeletedEntities.Count();

        ContentTypeService.Delete(contentType);
        ContentTypeService.Delete(contentType2);

        Assert.AreEqual(2, deletedEntities);
    }

    [Test]
    public void Deleting_ContentType_With_Child_Sends_Correct_Number_Of_DeletedEntities_In_Events()
    {
        var deletedEntities = 0;

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var contentType = ContentTypeBuilder.CreateSimpleContentType("page", "Page", defaultTemplateId: template.Id);
        ContentTypeService.Save(contentType);
        var contentType2 =
            ContentTypeBuilder.CreateSimpleContentType("subPage", "Sub page", defaultTemplateId: template.Id);
        contentType2.ParentId = contentType.Id;
        ContentTypeService.Save(contentType2);

        ContentTypeNotificationHandler.Deleted +=
            notification => deletedEntities += notification.DeletedEntities.Count();

        ContentTypeService.Delete(contentType);

        Assert.AreEqual(2, deletedEntities);
    }

    [Test]
    public void Can_Remove_ContentType_Composition_From_ContentType()
    {
        // Test for U4-2234
        var cts = ContentTypeService;

        // Arrange
        var component = CreateComponent();
        cts.Save(component);
        var banner = CreateBannerComponent(component);
        cts.Save(banner);
        var site = CreateSite();
        cts.Save(site);
        var homepage = CreateHomepage(site);
        cts.Save(homepage);

        // Add banner to homepage
        var added = homepage.AddContentType(banner);
        cts.Save(homepage);

        // Assert composition
        var bannerExists = homepage.ContentTypeCompositionExists(banner.Alias);
        var bannerPropertyExists = homepage.CompositionPropertyTypes.Any(x => x.Alias.Equals("bannerName"));
        Assert.That(added, Is.True);
        Assert.That(bannerExists, Is.True);
        Assert.That(bannerPropertyExists, Is.True);
        Assert.That(homepage.CompositionPropertyTypes.Count(), Is.EqualTo(6));

        // Remove banner from homepage
        var removed = homepage.RemoveContentType(banner.Alias);
        cts.Save(homepage);

        // Assert composition
        var bannerStillExists = homepage.ContentTypeCompositionExists(banner.Alias);
        var bannerPropertyStillExists = homepage.CompositionPropertyTypes.Any(x => x.Alias.Equals("bannerName"));
        Assert.That(removed, Is.True);
        Assert.That(bannerStillExists, Is.False);
        Assert.That(bannerPropertyStillExists, Is.False);
        Assert.That(homepage.CompositionPropertyTypes.Count(), Is.EqualTo(4));
    }

    [Test]
    public void Can_Copy_ContentType_By_Performing_Clone()
    {
        // Arrange
        var metaContentType = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaContentType);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var simpleContentType =
            ContentTypeBuilder.CreateSimpleContentType("category", "Category", metaContentType,
                defaultTemplateId: template.Id) as IContentType;
        ContentTypeService.Save(simpleContentType);
        var categoryId = simpleContentType.Id;

        // Act
        var sut = simpleContentType.DeepCloneWithResetIdentities("newcategory");
        Assert.IsNotNull(sut);
        ContentTypeService.Save(sut);

        // Assert
        Assert.That(sut.HasIdentity, Is.True);

        var contentType = ContentTypeService.Get(sut.Id);
        var category = ContentTypeService.Get(categoryId);

        Assert.That(contentType.CompositionAliases().Any(x => x.Equals("meta")), Is.True);
        Assert.AreEqual(contentType.ParentId, category.ParentId);
        Assert.AreEqual(contentType.Level, category.Level);
        Assert.AreEqual(contentType.PropertyTypes.Count(), category.PropertyTypes.Count());
        Assert.AreNotEqual(contentType.Id, category.Id);
        Assert.AreNotEqual(contentType.Key, category.Key);
        Assert.AreNotEqual(contentType.Path, category.Path);
        Assert.AreNotEqual(contentType.SortOrder, category.SortOrder);
        Assert.AreNotEqual(contentType.PropertyTypes.First(x => x.Alias.Equals("title")).Id,
            category.PropertyTypes.First(x => x.Alias.Equals("title")).Id);
        Assert.AreNotEqual(contentType.PropertyGroups.First(x => x.Name.Equals("Content")).Id,
            category.PropertyGroups.First(x => x.Name.Equals("Content")).Id);
    }

    [Test]
    public void Can_Copy_ContentType_To_New_Parent_By_Performing_Clone()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var parentContentType1 =
            ContentTypeBuilder.CreateSimpleContentType("parent1", "Parent1", defaultTemplateId: template.Id);
        ContentTypeService.Save(parentContentType1);
        var parentContentType2 = ContentTypeBuilder.CreateSimpleContentType("parent2", "Parent2",
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(parentContentType2);

        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType("category", "Category", parentContentType1,
            randomizeAliases: true, defaultTemplateId: template.Id) as IContentType;
        ContentTypeService.Save(simpleContentType);

        // Act
        var clone = simpleContentType.DeepCloneWithResetIdentities("newcategory");
        Assert.IsNotNull(clone);
        clone.RemoveContentType("parent1");
        clone.AddContentType(parentContentType2);
        clone.ParentId = parentContentType2.Id;
        ContentTypeService.Save(clone);

        // Assert
        Assert.That(clone.HasIdentity, Is.True);

        var clonedContentType = ContentTypeService.Get(clone.Id);
        var originalContentType = ContentTypeService.Get(simpleContentType.Id);

        Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
        Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

        Assert.AreEqual(clonedContentType.Path, "-1," + parentContentType2.Id + "," + clonedContentType.Id);
        Assert.AreEqual(clonedContentType.PropertyTypes.Count(), originalContentType.PropertyTypes.Count());

        Assert.AreNotEqual(clonedContentType.ParentId, originalContentType.ParentId);
        Assert.AreEqual(clonedContentType.ParentId, parentContentType2.Id);

        Assert.AreNotEqual(clonedContentType.Id, originalContentType.Id);
        Assert.AreNotEqual(clonedContentType.Key, originalContentType.Key);
        Assert.AreNotEqual(clonedContentType.Path, originalContentType.Path);

        Assert.AreNotEqual(clonedContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id,
            originalContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id);
        Assert.AreNotEqual(clonedContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id,
            originalContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id);
    }

    [Test]
    public void Can_Copy_ContentType_With_Service_To_Root()
    {
        // Arrange
        var metaContentType = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaContentType);

        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType("category", "Category", metaContentType,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(simpleContentType);
        var categoryId = simpleContentType.Id;

        // Act
        var clone = ContentTypeService.Copy(simpleContentType, "newcategory", "new category");

        // Assert
        Assert.That(clone.HasIdentity, Is.True);

        var cloned = ContentTypeService.Get(clone.Id);
        var original = ContentTypeService.Get(categoryId);

        Assert.That(cloned.CompositionAliases().Any(x => x.Equals("meta")), Is.False); // it's been copied to root
        Assert.AreEqual(cloned.ParentId, -1);
        Assert.AreEqual(cloned.Level, 1);
        Assert.AreEqual(cloned.PropertyTypes.Count(), original.PropertyTypes.Count());
        Assert.AreEqual(cloned.PropertyGroups.Count(), original.PropertyGroups.Count());

        for (var i = 0; i < cloned.PropertyGroups.Count; i++)
        {
            Assert.AreEqual(cloned.PropertyGroups[i].PropertyTypes.Count,
                original.PropertyGroups[i].PropertyTypes.Count);
            foreach (var propertyType in cloned.PropertyGroups[i].PropertyTypes)
            {
                Assert.IsTrue(propertyType.HasIdentity);
            }
        }

        foreach (var propertyType in cloned.PropertyTypes)
        {
            Assert.IsTrue(propertyType.HasIdentity);
        }

        Assert.AreNotEqual(cloned.Id, original.Id);
        Assert.AreNotEqual(cloned.Key, original.Key);
        Assert.AreNotEqual(cloned.Path, original.Path);
        Assert.AreNotEqual(cloned.SortOrder, original.SortOrder);
        Assert.AreNotEqual(cloned.PropertyTypes.First(x => x.Alias.Equals("title")).Id,
            original.PropertyTypes.First(x => x.Alias.Equals("title")).Id);
        Assert.AreNotEqual(cloned.PropertyGroups.First(x => x.Name.Equals("Content")).Id,
            original.PropertyGroups.First(x => x.Name.Equals("Content")).Id);
    }

    [Test]
    public void Can_Copy_ContentType_To_New_Parent_With_Service()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var parentContentType1 =
            ContentTypeBuilder.CreateSimpleContentType("parent1", "Parent1", defaultTemplateId: template.Id);
        ContentTypeService.Save(parentContentType1);
        var parentContentType2 = ContentTypeBuilder.CreateSimpleContentType("parent2", "Parent2",
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(parentContentType2);

        var simpleContentType = ContentTypeBuilder.CreateSimpleContentType("category", "Category", parentContentType1,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(simpleContentType);

        // Act
        var clone = ContentTypeService.Copy(simpleContentType, "newAlias", "new alias", parentContentType2);

        // Assert
        Assert.That(clone.HasIdentity, Is.True);

        var clonedContentType = ContentTypeService.Get(clone.Id);
        var originalContentType = ContentTypeService.Get(simpleContentType.Id);

        Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
        Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

        Assert.AreEqual(clonedContentType.Path, "-1," + parentContentType2.Id + "," + clonedContentType.Id);
        Assert.AreEqual(clonedContentType.PropertyTypes.Count(), originalContentType.PropertyTypes.Count());

        Assert.AreNotEqual(clonedContentType.ParentId, originalContentType.ParentId);
        Assert.AreEqual(clonedContentType.ParentId, parentContentType2.Id);

        Assert.AreNotEqual(clonedContentType.Id, originalContentType.Id);
        Assert.AreNotEqual(clonedContentType.Key, originalContentType.Key);
        Assert.AreNotEqual(clonedContentType.Path, originalContentType.Path);

        Assert.AreNotEqual(clonedContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id,
            originalContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id);
        Assert.AreNotEqual(clonedContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id,
            originalContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id);
    }

    [Test]
    public void Cannot_Add_Duplicate_PropertyType_Alias_To_Referenced_Composition()
    {
        // Related the second issue in screencast from this post http://issues.umbraco.org/issue/U4-5986

        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var parent = ContentTypeBuilder.CreateSimpleContentType(defaultTemplateId: template.Id);
        ContentTypeService.Save(parent);
        var child = ContentTypeBuilder.CreateSimpleContentType("simpleChildPage", "Simple Child Page", parent,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(child);
        var composition = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(composition);

        // Adding Meta-composition to child doc type
        child.AddContentType(composition);
        ContentTypeService.Save(child);

        // Act
        var duplicatePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var added = composition.AddPropertyType(duplicatePropertyType, "meta", "Meta");

        // Assert
        Assert.That(added, Is.True);
        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(composition));
        Assert.DoesNotThrow(() => ContentTypeService.Get("simpleChildPage"));
    }

    [Test]
    public void Cannot_Add_Duplicate_PropertyType_Alias_In_Composition_Graph()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var basePage = ContentTypeBuilder.CreateSimpleContentType("basePage", "Base Page", randomizeAliases: true,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", basePage,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(advancedPage);

        var metaComposition = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaComposition);
        var seoComposition = ContentTypeBuilder.CreateMetaContentType("seo", "SEO");
        ContentTypeService.Save(seoComposition);

        var metaAdded = contentPage.AddContentType(metaComposition);
        ContentTypeService.Save(contentPage);
        var seoAdded = advancedPage.AddContentType(seoComposition);
        ContentTypeService.Save(advancedPage);

        // Act
        var duplicatePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var addedToBasePage = basePage.AddPropertyType(duplicatePropertyType, "content", "Content");
        var addedToAdvancedPage = advancedPage.AddPropertyType(duplicatePropertyType, "content", "Content");
        var addedToMeta = metaComposition.AddPropertyType(duplicatePropertyType, "meta", "Meta");
        var addedToSeo = seoComposition.AddPropertyType(duplicatePropertyType, "seo", "Seo");

        // Assert
        Assert.That(metaAdded, Is.True);
        Assert.That(seoAdded, Is.True);

        Assert.That(addedToBasePage, Is.True);
        Assert.That(addedToAdvancedPage, Is.False);
        Assert.That(addedToMeta, Is.True);
        Assert.That(addedToSeo, Is.True);

        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(basePage));
        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(metaComposition));
        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(seoComposition));

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("meta"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("seo"));
    }

    [Test]
    public void Cannot_Add_Duplicate_PropertyType_Alias_At_Root_Which_Conflicts_With_Third_Levels_Composition()
    {
        /*
         * BasePage, gets 'Title' added but should not be allowed
         * -- Content Page
         * ---- Advanced Page -> Content Meta
         * Content Meta :: Composition, has 'Title'
         *
         * Content Meta has 'Title' PropertyType
         * Adding 'Title' to BasePage should fail
        */

        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var compositionAdded = advancedPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(advancedPage);

        // NOTE: It should not be possible to Save 'BasePage' with the Title PropertyType added
        var titlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var titleAdded = basePage.AddPropertyType(titlePropertyType, "content", "Content");

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(titleAdded, Is.True);
        Assert.That(compositionAdded, Is.True);

        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(basePage));

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
    }

    [Test]
    public void Cannot_Save_ContentType_With_Empty_Name()
    {
        // Arrange
        var contentType = ContentTypeBuilder.CreateSimpleContentType("contentType", string.Empty);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => ContentTypeService.Save(contentType));
    }

    [Test]
    public void Cannot_Rename_PropertyType_Alias_On_Composition_Which_Would_Cause_Conflict_In_Other_Composition()
    {
        /*
         * Meta renames alias to 'title'
         * Seo has 'Title'
         * BasePage
         * -- ContentPage
         * ---- AdvancedPage -> Seo
         * ------ MoreAdvanedPage -> Meta
         */

        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);
        var moreAdvancedPage =
            ContentTypeBuilder.CreateBasicContentType("moreAdvancedPage", "More Advanced Page", advancedPage);
        ContentTypeService.Save(moreAdvancedPage);

        var seoComposition = ContentTypeBuilder.CreateMetaContentType("seo", "SEO");
        ContentTypeService.Save(seoComposition);
        var metaComposition = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaComposition);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "content", "Content");
        ContentTypeService.Save(advancedPage);

        var titlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var titleAdded = seoComposition.AddPropertyType(titlePropertyType, "content", "Content");
        ContentTypeService.Save(seoComposition);

        var seoCompositionAdded = advancedPage.AddContentType(seoComposition);
        var metaCompositionAdded = moreAdvancedPage.AddContentType(metaComposition);
        ContentTypeService.Save(advancedPage);
        ContentTypeService.Save(moreAdvancedPage);

        var keywordsPropertyType = metaComposition.PropertyTypes.First(x => x.Alias.Equals("metakeywords"));
        keywordsPropertyType.Alias = "title";

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(titleAdded, Is.True);
        Assert.That(seoCompositionAdded, Is.True);
        Assert.That(metaCompositionAdded, Is.True);

        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(metaComposition));

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("moreAdvancedPage"));
    }

    [Test]
    public void Can_Add_Additional_Properties_On_Composition_Once_Composition_Has_Been_Saved()
    {
        /*
         * Meta renames alias to 'title'
         * Seo has 'Title'
         * BasePage
         * -- ContentPage
         * ---- AdvancedPage -> Seo
         * ------ MoreAdvancedPage -> Meta
         */

        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);
        var moreAdvancedPage =
            ContentTypeBuilder.CreateBasicContentType("moreAdvancedPage", "More Advanced Page", advancedPage);
        ContentTypeService.Save(moreAdvancedPage);

        var seoComposition = ContentTypeBuilder.CreateMetaContentType("seo", "SEO");
        ContentTypeService.Save(seoComposition);
        var metaComposition = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaComposition);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "content", "Content");
        ContentTypeService.Save(advancedPage);

        var titlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var titleAdded = seoComposition.AddPropertyType(titlePropertyType, "content", "Content");
        ContentTypeService.Save(seoComposition);

        var seoCompositionAdded = advancedPage.AddContentType(seoComposition);
        var metaCompositionAdded = moreAdvancedPage.AddContentType(metaComposition);
        ContentTypeService.Save(advancedPage);
        ContentTypeService.Save(moreAdvancedPage);

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(titleAdded, Is.True);
        Assert.That(seoCompositionAdded, Is.True);
        Assert.That(metaCompositionAdded, Is.True);

        var testPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "test")
            {
                Name = "Test",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var testAdded = seoComposition.AddPropertyType(testPropertyType, "content", "Content");
        ContentTypeService.Save(seoComposition);

        Assert.That(testAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("moreAdvancedPage"));
    }

    [Test]
    public void Cannot_Rename_PropertyGroup_On_Child_Avoiding_Conflict_With_Parent_PropertyGroup()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);

        var page = ContentTypeBuilder.CreateSimpleContentType("page", "Page", randomizeAliases: true,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(page);
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", page,
            randomizeAliases: true, propertyGroupAlias: "content2", propertyGroupName: "Content_",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage,
            randomizeAliases: true, propertyGroupAlias: "details", propertyGroupName: "Details",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(advancedPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "content", "Content");
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var compositionAdded = contentPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(contentPage);

        // Change the name of the tab on the "root" content type 'page'.
        var propertyGroup = contentPage.PropertyGroups["content2"];
        Assert.Throws<ArgumentException>(() => contentPage.PropertyGroups.Add(new PropertyGroup(true)
        {
            Id = propertyGroup.Id,
            Alias = "content",
            Name = "Content",
            SortOrder = 0
        }));

        // Assert
        Assert.That(compositionAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);
        Assert.That(authorAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
    }

    [Test]
    public void Cannot_Rename_PropertyType_Alias_Causing_Conflicts_With_Parents()
    {
        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);

        // Act
        var titlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var titleAdded = basePage.AddPropertyType(titlePropertyType, "content", "Content");
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = contentPage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "content", "Content");
        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = advancedPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);
        ContentTypeService.Save(contentPage);
        ContentTypeService.Save(advancedPage);

        // Rename the PropertyType to something that already exists in the Composition - NOTE this should not be allowed and Saving should throw an exception
        var authorPropertyTypeToRename = advancedPage.PropertyTypes.First(x => x.Alias.Equals("author"));
        authorPropertyTypeToRename.Alias = "title";

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(titleAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);

        Assert.Throws<InvalidCompositionException>(() => ContentTypeService.Save(advancedPage));

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));
    }

    [Test]
    public void Can_Add_PropertyType_Alias_Which_Exists_In_Composition_Outside_Graph()
    {
        /*
         * Meta (Composition)
         * Content Meta (Composition) has 'Title' -> Meta
         * BasePage
         * -- ContentPage gets 'Title' added -> Meta
         * ---- Advanced Page
         */
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var basePage = ContentTypeBuilder.CreateSimpleContentType("basePage", "Base Page", randomizeAliases: true,
            defaultTemplateId: template.Id);
        ContentTypeService.Save(basePage);
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", basePage,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage,
            randomizeAliases: true, defaultTemplateId: template.Id);
        ContentTypeService.Save(advancedPage);

        var metaComposition = ContentTypeBuilder.CreateMetaContentType();
        ContentTypeService.Save(metaComposition);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        var metaAdded = contentPage.AddContentType(metaComposition);
        ContentTypeService.Save(contentPage);

        var metaAddedToComposition = contentMetaComposition.AddContentType(metaComposition);
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var propertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var addedToContentPage = contentPage.AddPropertyType(propertyType, "content", "Content");

        // Assert
        Assert.That(metaAdded, Is.True);
        Assert.That(metaAddedToComposition, Is.True);

        Assert.That(addedToContentPage, Is.True);
        Assert.DoesNotThrow(() => ContentTypeService.Save(contentPage));
    }

    [Test]
    public void Can_Rename_PropertyGroup_With_Inherited_PropertyGroups()
    {
        // Related the first issue in screencast from this post http://issues.umbraco.org/issue/U4-5986

        // Arrange
        // create 'page' content type with a 'Content_' group
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var page = ContentTypeBuilder.CreateSimpleContentType("page", "Page", propertyGroupAlias: "content2",
            propertyGroupName: "Content_", defaultTemplateId: template.Id);
        Assert.AreEqual(1, page.PropertyGroups.Count);
        Assert.AreEqual("Content_", page.PropertyGroups.First().Name);
        Assert.AreEqual(3, page.PropertyTypes.Count());
        Assert.AreEqual("Title", page.PropertyTypes.First().Name);
        Assert.AreEqual("Body text", page.PropertyTypes.Skip(1).First().Name);
        Assert.AreEqual("Author", page.PropertyTypes.Skip(2).First().Name);
        ContentTypeService.Save(page);

        // create 'contentPage' content type as a child of 'page'
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", page,
            randomizeAliases: true, defaultTemplateId: template.Id);
        Assert.AreEqual(1, page.PropertyGroups.Count);
        Assert.AreEqual("Content_", page.PropertyGroups.First().Name);
        Assert.AreEqual(3, contentPage.PropertyTypes.Count());
        Assert.AreEqual("Title", contentPage.PropertyTypes.First().Name);
        Assert.AreEqual("Body text", contentPage.PropertyTypes.Skip(1).First().Name);
        Assert.AreEqual("Author", contentPage.PropertyTypes.Skip(2).First().Name);
        ContentTypeService.Save(contentPage);

        // add 'Content' group to 'meta' content type
        var meta = ContentTypeBuilder.CreateMetaContentType();
        Assert.AreEqual(1, meta.PropertyGroups.Count);
        Assert.AreEqual("Meta", meta.PropertyGroups.First().Name);
        Assert.AreEqual(2, meta.PropertyTypes.Count());
        Assert.AreEqual("Meta Keywords", meta.PropertyTypes.First().Name);
        Assert.AreEqual("Meta Description", meta.PropertyTypes.Skip(1).First().Name);
        meta.AddPropertyGroup("content", "Content");
        Assert.AreEqual(2, meta.PropertyTypes.Count());
        ContentTypeService.Save(meta);

        // add 'meta' content type to 'contentPage' composition
        contentPage.AddContentType(meta);
        ContentTypeService.Save(contentPage);

        // add property 'prop1' to 'contentPage' group 'Content_'
        var prop1 =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "testTextbox")
            {
                Name = "Test Textbox",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var prop1Added = contentPage.AddPropertyType(prop1, "content2", "Content_");
        Assert.IsTrue(prop1Added);

        // add property 'prop2' to 'contentPage' group 'Content'
        var prop2 =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "anotherTextbox")
            {
                Name = "Another Test Textbox",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var prop2Added = contentPage.AddPropertyType(prop2, "content", "Content");
        Assert.IsTrue(prop2Added);

        // save 'contentPage' content type
        ContentTypeService.Save(contentPage);

        var group = page.PropertyGroups["content2"];
        group.Name = "ContentTab"; // rename the group
        ContentTypeService.Save(page);
        Assert.AreEqual(3, page.PropertyTypes.Count());

        // get 'contentPage' content type again
        var contentPageAgain = ContentTypeService.Get("contentPage");
        Assert.IsNotNull(contentPageAgain);

        // assert that 'Content_' group is still there because we don't propagate renames
        var findGroup = contentPageAgain.CompositionPropertyGroups.FirstOrDefault(x => x.Name == "Content_");
        Assert.IsNotNull(findGroup);

        // count all property types (local and composed)
        var propertyTypeCount = contentPageAgain.PropertyTypes.Count();
        Assert.That(propertyTypeCount, Is.EqualTo(5));

        // count composed property types
        var compPropertyTypeCount = contentPageAgain.CompositionPropertyTypes.Count();
        Assert.That(compPropertyTypeCount, Is.EqualTo(10));
    }

    [Test]
    public void Can_Rename_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var page = ContentTypeBuilder.CreateSimpleContentType("page", "Page", randomizeAliases: true,
            propertyGroupAlias: "content2", propertyGroupName: "Content_", defaultTemplateId: template.Id);
        ContentTypeService.Save(page);
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", page,
            randomizeAliases: true, propertyGroupAlias: "contentx", propertyGroupName: "Contentx",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentPage);
        var advancedPage = ContentTypeBuilder.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage,
            randomizeAliases: true, propertyGroupAlias: "contenty", propertyGroupName: "Contenty",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(advancedPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);
        var compositionAdded = contentPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(contentPage);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded =
            contentPage.AddPropertyType(bodyTextPropertyType, "content2",
                "Content_"); // Will be added to the parent tab
        var subtitleAdded =
            contentPage.AddPropertyType(subtitlePropertyType, "content",
                "Content"); // Will be added to the "Content Meta" composition
        ContentTypeService.Save(contentPage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var descriptionPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "description")
            {
                Name = "Description",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var keywordsPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "keywords")
            {
                Name = "Keywords",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded =
            advancedPage.AddPropertyType(authorPropertyType, "content2",
                "Content_"); // Will be added to an ancestor tab
        var descriptionAdded =
            advancedPage.AddPropertyType(descriptionPropertyType, "contentx",
                "Contentx"); // Will be added to a parent tab
        var keywordsAdded =
            advancedPage.AddPropertyType(keywordsPropertyType, "content",
                "Content"); // Will be added to the "Content Meta" composition
        ContentTypeService.Save(advancedPage);

        // Change the name of the tab on the "root" content type 'page'.
        var propertyGroup = page.PropertyGroups["content2"];
        page.PropertyGroups.Add(new PropertyGroup(true)
        {
            Id = propertyGroup.Id,
            Name = "Content",
            Alias = "content",
            SortOrder = 0
        });
        ContentTypeService.Save(page);

        // Assert
        Assert.That(compositionAdded, Is.True);
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(descriptionAdded, Is.True);
        Assert.That(keywordsAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));

        var advancedPageReloaded = ContentTypeService.Get("advancedPage");
        var contentUnderscoreTabExists =
            advancedPageReloaded.CompositionPropertyGroups.Any(x => x.Name.Equals("Content_"));

        // now is true, because we don't propagate renames anymore
        Assert.That(contentUnderscoreTabExists, Is.True);

        var numberOfContentTabs = advancedPageReloaded.CompositionPropertyGroups.Count(x => x.Name.Equals("Content"));
        Assert.That(numberOfContentTabs, Is.EqualTo(4));
    }

    [Test]
    public void Can_Rename_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups_v2()
    {
        // Arrange
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var page = ContentTypeBuilder.CreateSimpleContentType("page", "Page", randomizeAliases: true,
            propertyGroupAlias: "content2", propertyGroupName: "Content_", defaultTemplateId: template.Id);
        ContentTypeService.Save(page);
        var contentPage = ContentTypeBuilder.CreateSimpleContentType("contentPage", "Content Page", page,
            randomizeAliases: true, propertyGroupAlias: "content", propertyGroupName: "Content",
            defaultTemplateId: template.Id);
        ContentTypeService.Save(contentPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var subtitlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "subtitle")
            {
                Name = "Subtitle",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = page.AddPropertyType(bodyTextPropertyType, "content2", "Content_");
        var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "content", "Content");
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content2", "Content_");
        ContentTypeService.Save(page);
        ContentTypeService.Save(contentPage);

        var compositionAdded = contentPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(contentPage);

        // Change the alias/name of the tab on the "root" content type 'page'.
        var propertyGroup = page.PropertyGroups["content2"];
        page.PropertyGroups.Add(new PropertyGroup(true)
        {
            Id = propertyGroup.Id,
            Alias = "content",
            Name = "Content",
            SortOrder = 0
        });
        ContentTypeService.Save(page);

        // Assert
        Assert.That(compositionAdded, Is.True);
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(subtitleAdded, Is.True);
        Assert.That(authorAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
    }

    [Test]
    public void Can_Remove_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups()
    {
        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);

        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);

        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var compositionAdded = contentPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(contentPage);

        basePage.RemovePropertyGroup("content");
        ContentTypeService.Save(basePage);

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(compositionAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));

        var contentType = ContentTypeService.Get("contentPage");
        var propertyGroup = contentType.PropertyGroups["content"];
    }

    [Test]
    public void Can_Remove_PropertyGroup_Without_Removing_Property_Types()
    {
        var basePage = (IContentType)ContentTypeBuilder.CreateBasicContentType();
        basePage.AddPropertyGroup("content", "Content");
        basePage.AddPropertyGroup("meta", "Meta");
        ContentTypeService.Save(basePage);

        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        Assert.IsTrue(basePage.AddPropertyType(authorPropertyType, "content", "Content"));

        var titlePropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "title")
            {
                Name = "Title",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        Assert.IsTrue(basePage.AddPropertyType(titlePropertyType, "meta", "Meta"));

        ContentTypeService.Save(basePage);
        basePage = ContentTypeService.Get(basePage.Id);

        var count = basePage.PropertyTypes.Count();
        Assert.AreEqual(2, count);

        basePage.RemovePropertyGroup("content");

        ContentTypeService.Save(basePage);
        basePage = ContentTypeService.Get(basePage.Id);

        Assert.AreEqual(count, basePage.PropertyTypes.Count());
    }

    [Test]
    public void Can_Add_PropertyGroup_With_Same_Name_On_Parent_and_Child()
    {
        /*
         * BasePage
         * - Content Page
         * -- Advanced Page
         * Content Meta :: Composition
        */

        // Arrange
        var basePage = ContentTypeBuilder.CreateBasicContentType();
        ContentTypeService.Save(basePage);

        var contentPage = ContentTypeBuilder.CreateBasicContentType("contentPage", "Content Page", basePage);
        ContentTypeService.Save(contentPage);

        var advancedPage = ContentTypeBuilder.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
        ContentTypeService.Save(advancedPage);

        var contentMetaComposition = ContentTypeBuilder.CreateContentMetaContentType();
        ContentTypeService.Save(contentMetaComposition);

        // Act
        var authorPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "author")
            {
                Name = "Author",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
        ContentTypeService.Save(contentPage);

        var bodyTextPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext,
                "bodyText")
            {
                Name = "Body Text",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "content", "Content");
        ContentTypeService.Save(basePage);

        var compositionAdded = contentPage.AddContentType(contentMetaComposition);
        ContentTypeService.Save(contentPage);

        // Assert
        Assert.That(bodyTextAdded, Is.True);
        Assert.That(authorAdded, Is.True);
        Assert.That(compositionAdded, Is.True);

        Assert.DoesNotThrow(() => ContentTypeService.Get("contentPage"));
        Assert.DoesNotThrow(() => ContentTypeService.Get("advancedPage"));

        var contentType = ContentTypeService.Get("contentPage");
        var propertyGroup = contentType.PropertyGroups["content"];

        var numberOfContentTabs = contentType.CompositionPropertyGroups.Count(x => x.Name.Equals("Content"));
        Assert.That(numberOfContentTabs, Is.EqualTo(3));

        // Ensure that adding a new PropertyType to the "Content"-tab also adds it to the right group
        var descriptionPropertyType =
            new PropertyType(ShortStringHelper, Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "description",
                Name = "Description",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
        var descriptionAdded = contentType.AddPropertyType(descriptionPropertyType, "content", "Content");
        ContentTypeService.Save(contentType);
        Assert.That(descriptionAdded, Is.True);

        var contentPageReloaded = ContentTypeService.Get("contentPage");
        var propertyGroupReloaded = contentPageReloaded.PropertyGroups["content"];
        var hasDescriptionPropertyType = propertyGroupReloaded.PropertyTypes.Contains("description");
        Assert.That(hasDescriptionPropertyType, Is.True);

        var descriptionPropertyTypeReloaded = propertyGroupReloaded.PropertyTypes["description"];
        Assert.That(descriptionPropertyTypeReloaded.PropertyGroupId.IsValueCreated, Is.False);
    }

    [Test]
    public void Empty_Description_Is_Always_Null_After_Saving_Content_Type()
    {
        var contentType = ContentTypeBuilder.CreateBasicContentType();
        contentType.Description = null;
        ContentTypeService.Save(contentType);

        var contentType2 = ContentTypeBuilder.CreateBasicContentType("basePage2", "Base Page 2");
        contentType2.Description = string.Empty;
        ContentTypeService.Save(contentType2);

        Assert.IsNull(contentType.Description);
        Assert.IsNull(contentType2.Description);
    }

    [Test]
    public void Variations_In_Compositions()
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var typeA = ContentTypeBuilder.CreateSimpleContentType("a", "A", defaultTemplateId: template.Id);
        typeA.Variations = ContentVariation.Culture; // make it variant
        typeA.PropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations =
            ContentVariation.Culture; // with a variant property
        ContentTypeService.Save(typeA);

        var typeB = ContentTypeBuilder.CreateSimpleContentType("b", "B", typeA, randomizeAliases: true,
            defaultTemplateId: template.Id);
        typeB.Variations = ContentVariation.Nothing; // make it invariant
        ContentTypeService.Save(typeB);

        var typeC = ContentTypeBuilder.CreateSimpleContentType("c", "C", typeA, randomizeAliases: true,
            defaultTemplateId: template.Id);
        typeC.Variations = ContentVariation.Culture; // make it variant
        ContentTypeService.Save(typeC);

        // property is variant on A
        var test = ContentTypeService.Get(typeA.Id);
        Assert.AreEqual(ContentVariation.Culture,
            test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
        Assert.AreEqual(ContentVariation.Culture,
            test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title"))
                .Variations);

        // but not on B
        test = ContentTypeService.Get(typeB.Id);
        Assert.AreEqual(ContentVariation.Nothing,
            test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
        Assert.AreEqual(ContentVariation.Nothing,
            test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title"))
                .Variations);

        // but on C
        test = ContentTypeService.Get(typeC.Id);
        Assert.AreEqual(ContentVariation.Culture,
            test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
        Assert.AreEqual(ContentVariation.Culture,
            test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title"))
                .Variations);
    }

    private ContentType CreateComponent()
    {
        var component = new ContentType(ShortStringHelper, -1)
        {
            Alias = "component",
            Name = "Component",
            Description = "ContentType used for Component grouping",
            Icon = ".sprTreeDoc3",
            Thumbnail = "doc.png",
            SortOrder = 1,
            CreatorId = 0,
            Trashed = false
        };

        var contentCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "componentGroup")
            {
                Name = "Component Group",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            }
        };

        component.PropertyGroups.Add(new PropertyGroup(contentCollection)
        {
            Alias = "component",
            Name = "Component",
            SortOrder = 1
        });

        return component;
    }

    private ContentType CreateBannerComponent(ContentType parent)
    {
        const string contentTypeAlias = "banner";
        var banner = new ContentType(ShortStringHelper, parent, contentTypeAlias)
        {
            Alias = contentTypeAlias,
            Name = "Banner Component",
            Description = "ContentType used for Banner Component",
            Icon = ".sprTreeDoc3",
            Thumbnail = "doc.png",
            SortOrder = 1,
            CreatorId = 0,
            Trashed = false
        };

        var propertyType = new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "bannerName")
        {
            Name = "Banner Name",
            Description = string.Empty,
            Mandatory = false,
            SortOrder = 2,
            DataTypeId = -88
        };
        banner.AddPropertyType(propertyType, "component", "Component");
        return banner;
    }

    private ContentType CreateSite()
    {
        var site = new ContentType(ShortStringHelper, -1)
        {
            Alias = "site",
            Name = "Site",
            Description = "ContentType used for Site inheritence",
            Icon = ".sprTreeDoc3",
            Thumbnail = "doc.png",
            SortOrder = 2,
            CreatorId = 0,
            Trashed = false
        };

        var contentCollection = new PropertyTypeCollection(true)
        {
            new PropertyType(ShortStringHelper, "test", ValueStorageType.Ntext, "hostname")
            {
                Name = "Hostname",
                Description = string.Empty,
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            }
        };
        site.PropertyGroups.Add(
            new PropertyGroup(contentCollection) { Name = "Site Settings", Alias = "siteSettings", SortOrder = 1 });

        return site;
    }

    private ContentType CreateHomepage(ContentType parent)
    {
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        return ContentTypeBuilder.CreateSimpleContentType("homepage", "Homepage", parent,
            defaultTemplateId: template.Id);
    }

    private IContentType[] CreateContentTypeHierarchy()
    {
        // create the master type
        var template = TemplateBuilder.CreateTextPageTemplate();
        FileService.SaveTemplate(template);
        var masterContentType = ContentTypeBuilder.CreateSimpleContentType("masterContentType", "MasterContentType",
            defaultTemplateId: template.Id);
        masterContentType.Key = new Guid("C00CA18E-5A9D-483B-A371-EECE0D89B4AE");
        ContentTypeService.Save(masterContentType);

        // add the one we just created
        var list = new List<IContentType> { masterContentType };

        for (var i = 0; i < 10; i++)
        {
            var contentType = ContentTypeBuilder.CreateSimpleContentType(
                "childType" + i,
                "ChildType" + i,
                list.Last(), // make the last entry in the list, this one's parent
                randomizeAliases: true,
                defaultTemplateId: template.Id);

            list.Add(contentType);
        }

        return list.ToArray();
    }

    public class ContentNotificationHandler : INotificationHandler<ContentMovedToRecycleBinNotification>
    {
        public static Action<ContentMovedToRecycleBinNotification> MovedContentToRecycleBin { get; set; }

        public void Handle(ContentMovedToRecycleBinNotification notification) =>
            MovedContentToRecycleBin?.Invoke(notification);
    }

    public class ContentTypeNotificationHandler : INotificationHandler<ContentTypeDeletedNotification>
    {
        public static Action<ContentTypeDeletedNotification> Deleted { get; set; }
        public void Handle(ContentTypeDeletedNotification notification) => Deleted?.Invoke(notification);
    }
}
