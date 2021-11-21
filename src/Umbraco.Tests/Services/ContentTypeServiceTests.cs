using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Services.Implement;
using Umbraco.Tests.LegacyXmlPublishedCache;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Category("Slow")]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest, PublishedRepositoryEvents = true)]
    public class ContentTypeServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void CanSaveAndGetIsElement()
        {
            //create content type with a property type that varies by culture
            IContentType contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Variations = ContentVariation.Nothing;
            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext)
            {
                Alias = "title",
                Name = "Title",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88,
                Variations = ContentVariation.Nothing
            });
            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });
            ServiceContext.ContentTypeService.Save(contentType);

            contentType = ServiceContext.ContentTypeService.Get(contentType.Id);
            Assert.IsFalse(contentType.IsElement);

            contentType.IsElement = true;
            ServiceContext.ContentTypeService.Save(contentType);

            contentType = ServiceContext.ContentTypeService.Get(contentType.Id);
            Assert.IsTrue(contentType.IsElement);
        }

        

        [Test]
        public void Deleting_Content_Type_With_Hierarchy_Of_Content_Items_Moves_Orphaned_Content_To_Recycle_Bin()
        {
            IContentType contentType1 = MockedContentTypes.CreateSimpleContentType("test1", "Test1");
            ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType1);
            IContentType contentType2 = MockedContentTypes.CreateSimpleContentType("test2", "Test2");
            ServiceContext.FileService.SaveTemplate(contentType2.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType2);
            IContentType contentType3 = MockedContentTypes.CreateSimpleContentType("test3", "Test3");
            ServiceContext.FileService.SaveTemplate(contentType3.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType3);

            var contentTypes = new[] { contentType1, contentType2, contentType3 };
            var parentId = -1;

            var ids = new List<int>();

            for (int i = 0; i < 2; i++)
            {
                for (var index = 0; index < contentTypes.Length; index++)
                {
                    var contentType = contentTypes[index];
                    var contentItem = MockedContent.CreateSimpleContent(contentType, "MyName_" + index + "_" + i, parentId);
                    ServiceContext.ContentService.Save(contentItem);
                    ServiceContext.ContentService.SaveAndPublish(contentItem);
                    parentId = contentItem.Id;

                    ids.Add(contentItem.Id);
                }
            }

            //delete the first content type, all other content of different content types should be in the recycle bin
            ServiceContext.ContentTypeService.Delete(contentTypes[0]);

            var found = ServiceContext.ContentService.GetByIds(ids);

            Assert.AreEqual(4, found.Count());
            foreach (var content in found)
            {
                Assert.IsTrue(content.Trashed);
            }
        }

        [Test]
        public void Deleting_Content_Types_With_Hierarchy_Of_Content_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items_1()
        {
            ContentService.Trashed += ContentServiceOnTrashed;

            try
            {
                IContentType contentType1 = MockedContentTypes.CreateSimpleContentType("test1", "Test1");
                ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType1);
                IContentType contentType2 = MockedContentTypes.CreateSimpleContentType("test2", "Test2");
                ServiceContext.FileService.SaveTemplate(contentType2.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType2);
                IContentType contentType3 = MockedContentTypes.CreateSimpleContentType("test3", "Test3");
                ServiceContext.FileService.SaveTemplate(contentType3.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType3);

                var contentTypes = new[] { contentType1, contentType2, contentType3 };
                var parentId = -1;

                for (int i = 0; i < 2; i++)
                {
                    for (var index = 0; index < contentTypes.Length; index++)
                    {
                        var contentType = contentTypes[index];
                        var contentItem = MockedContent.CreateSimpleContent(contentType, "MyName_" + index + "_" + i, parentId);
                        ServiceContext.ContentService.Save(contentItem);
                        ServiceContext.ContentService.SaveAndPublish(contentItem);
                        parentId = contentItem.Id;
                    }
                }

                foreach (var contentType in contentTypes.Reverse())
                {
                    ServiceContext.ContentTypeService.Delete(contentType);
                }
            }
            finally
            {
                ContentService.Trashed -= ContentServiceOnTrashed;
            }
        }

        [Test]
        public void Deleting_Content_Types_With_Hierarchy_Of_Content_Items_Doesnt_Raise_Trashed_Event_For_Deleted_Items_2()
        {
            ContentService.Trashed += ContentServiceOnTrashed;

            try
            {
                IContentType contentType1 = MockedContentTypes.CreateSimpleContentType("test1", "Test1");
                ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType1);
                IContentType contentType2 = MockedContentTypes.CreateSimpleContentType("test2", "Test2");
                ServiceContext.FileService.SaveTemplate(contentType2.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType2);
                IContentType contentType3 = MockedContentTypes.CreateSimpleContentType("test3", "Test3");
                ServiceContext.FileService.SaveTemplate(contentType3.DefaultTemplate);
                ServiceContext.ContentTypeService.Save(contentType3);

                var root = MockedContent.CreateSimpleContent(contentType1, "Root", -1);
                ServiceContext.ContentService.Save(root);
                ServiceContext.ContentService.SaveAndPublish(root);

                var level1 = MockedContent.CreateSimpleContent(contentType2, "L1", root.Id);
                ServiceContext.ContentService.Save(level1);
                ServiceContext.ContentService.SaveAndPublish(level1);

                for (int i = 0; i < 2; i++)
                {
                    var level3 = MockedContent.CreateSimpleContent(contentType3, "L2" + i, level1.Id);
                    ServiceContext.ContentService.Save(level3);
                    ServiceContext.ContentService.SaveAndPublish(level3);
                }

                ServiceContext.ContentTypeService.Delete(contentType1);
            }
            finally
            {
                ContentService.Trashed -= ContentServiceOnTrashed;
            }
        }

        private void ContentServiceOnTrashed(IContentService sender, MoveEventArgs<IContent> e)
        {
            foreach (var item in e.MoveInfoCollection)
            {
                //if this item doesn't exist then Fail!
                var exists = ServiceContext.ContentService.GetById(item.Entity.Id);
                if (exists == null)
                    Assert.Fail("The item doesn't exist");
            }
        }

        [Test]
        public void Deleting_PropertyType_Removes_The_Property_From_Content()
        {
            IContentType contentType1 = MockedContentTypes.CreateTextPageContentType("test1", "Test1");
            ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType1);
            IContent contentItem = MockedContent.CreateTextpageContent(contentType1, "Testing", -1);
            ServiceContext.ContentService.SaveAndPublish(contentItem);
            var initProps = contentItem.Properties.Count;

            //remove a property
            contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
            ServiceContext.ContentTypeService.Save(contentType1);

            //re-load it from the db
            contentItem = ServiceContext.ContentService.GetById(contentItem.Id);

            Assert.AreEqual(initProps - 1, contentItem.Properties.Count);
        }

        [Test]
        public void Rebuild_Content_Xml_On_Alias_Change()
        {
            var contentType1 = MockedContentTypes.CreateTextPageContentType("test1", "Test1");
            ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType1);

            var contentType2 = MockedContentTypes.CreateTextPageContentType("test2", "Test2");
            ServiceContext.FileService.SaveTemplate(contentType2.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType2);

            var contentItems1 = MockedContent.CreateTextpageContent(contentType1, -1, 10).ToArray();
            foreach (var x in contentItems1)
            {
                ServiceContext.ContentService.SaveAndPublish(x);
            }

            var contentItems2 = MockedContent.CreateTextpageContent(contentType2, -1, 5).ToArray();
            foreach (var x in contentItems2)
            {
                ServiceContext.ContentService.SaveAndPublish(x);
            }

            // make sure we have everything
            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var c in contentItems1)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsTrue(xml.Xml.StartsWith("<test1"));
                }

                foreach (var c in contentItems2)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsTrue(xml.Xml.StartsWith("<test2"));
                }

                scope.Complete();
            }

            // only update the contentType1 alias which will force an xml rebuild for all content of that type
            contentType1.Alias = "newAlias";
            ServiceContext.ContentTypeService.Save(contentType1);

            // make sure updates have taken place
            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var c in contentItems1)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsTrue(xml.Xml.StartsWith("<newAlias"));
                }

                foreach (var c in contentItems2)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsTrue(xml.Xml.StartsWith("<test2")); //should remain the same
                }

                scope.Complete();
            }
        }

        [Test]
        public void Rebuild_Content_Xml_On_Property_Removal()
        {
            var contentType1 = MockedContentTypes.CreateTextPageContentType("test1", "Test1");
            ServiceContext.FileService.SaveTemplate(contentType1.DefaultTemplate);
            ServiceContext.ContentTypeService.Save(contentType1);
            var contentItems1 = MockedContent.CreateTextpageContent(contentType1, -1, 10).ToArray();
            foreach (var x in contentItems1)
            {
                ServiceContext.ContentService.SaveAndPublish(x);
            }
            var alias = contentType1.PropertyTypes.First().Alias;
            var elementToMatch = "<" + alias + ">";

            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var c in contentItems1)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsTrue(xml.Xml.Contains(elementToMatch)); //verify that it is there before we remove the property
                }

                scope.Complete();
            }

            //remove a property
            contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
            ServiceContext.ContentTypeService.Save(contentType1);

            var reQueried = ServiceContext.ContentTypeService.Get(contentType1.Id);
            var reContent = ServiceContext.ContentService.GetById(contentItems1.First().Id);

            using (var scope = ScopeProvider.CreateScope())
            {
                foreach (var c in contentItems1)
                {
                    var xml = scope.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                    Assert.IsNotNull(xml);
                    Assert.IsFalse(xml.Xml.Contains(elementToMatch)); //verify that it is no longer there
                }

                scope.Complete();
            }
        }

        [Test]
        public void Get_Descendants()
        {
            // Arrange
            var contentTypeService = ServiceContext.ContentTypeService;
            var hierarchy = CreateContentTypeHierarchy();
            contentTypeService.Save(hierarchy, 0); //ensure they are saved!
            var master = hierarchy.First();

            //Act
            var descendants = contentTypeService.GetDescendants(master.Id, false);

            //Assert
            Assert.AreEqual(10, descendants.Count());
        }

        [Test]
        public void Get_Descendants_And_Self()
        {
            // Arrange
            var contentTypeService = ServiceContext.ContentTypeService;
            var hierarchy = CreateContentTypeHierarchy();
            contentTypeService.Save(hierarchy, 0); //ensure they are saved!
            var master = hierarchy.First();

            //Act
            var descendants = contentTypeService.GetDescendants(master.Id, true);

            //Assert
            Assert.AreEqual(11, descendants.Count());
        }

        [Test]
        public void Get_With_Missing_Guid()
        {
            // Arrange
            var mediaTypeService = ServiceContext.MediaTypeService;

            //Act
            var result = mediaTypeService.Get(Guid.NewGuid());

            //Assert
            Assert.IsNull(result);
        }

        [Test]
        public void Can_Bulk_Save_New_Hierarchy_Content_Types()
        {
            // Arrange
            var contentTypeService = ServiceContext.ContentTypeService;
            var hierarchy = CreateContentTypeHierarchy();

            // Act
            contentTypeService.Save(hierarchy, 0);

            Assert.That(hierarchy.Any(), Is.True);
            Assert.That(hierarchy.Any(x => x.HasIdentity == false), Is.False);
            //all parent id's should be ok, they are lazy and if they equal zero an exception will be thrown
            Assert.DoesNotThrow(() => hierarchy.Any(x => x.ParentId != 0));
            for (var i = 0; i < hierarchy.Count(); i++)
            {
                if (i == 0) continue;
                Assert.AreEqual(hierarchy.ElementAt(i).ParentId, hierarchy.ElementAt(i - 1).Id);
            }
        }

        [Test]
        public void Can_Save_ContentType_Structure_And_Create_Content_Based_On_It()
        {
            // Arrange
            var cs = ServiceContext.ContentService;
            var cts = ServiceContext.ContentTypeService;
            var dtdYesNo = ServiceContext.DataTypeService.GetDataType(-49);
            var ctBase = new ContentType(-1) { Name = "Base", Alias = "Base", Icon = "folder.gif", Thumbnail = "folder.png" };
            ctBase.AddPropertyType(new PropertyType(dtdYesNo, Constants.Conventions.Content.NaviHide)
            {
                Name = "Hide From Navigation",
            }
                /*,"Navigation"*/);
            cts.Save(ctBase);

            const string contentTypeAlias = "HomePage";
            var ctHomePage = new ContentType(ctBase, contentTypeAlias)
            {
                Name = "Home Page",
                Alias = contentTypeAlias,
                Icon = "settingDomain.gif",
                Thumbnail = "folder.png",
                AllowedAsRoot = true
            };
            ctHomePage.AddPropertyType(new PropertyType(dtdYesNo, "someProperty") { Name = "Some property" }
                /*,"Navigation"*/);
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
            var service = ServiceContext.ContentTypeService;

            var contentType = new ContentType(-1)
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

            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title") { Name = "Title", Description = "", Mandatory = false, DataTypeId = -88 });
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TinyMce, ValueStorageType.Ntext, "bodyText") { Name = "Body Text", Description = "", Mandatory = false, DataTypeId = -87 });
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author") { Name = "Author", Description = "Name of the author", Mandatory = false, DataTypeId = -88 });

            service.Save(contentType);

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
            var service = ServiceContext.ContentTypeService;
            var global = MockedContentTypes.CreateSimpleContentType("global", "Global");
            service.Save(global);

            var components = MockedContentTypes.CreateSimpleContentType("components", "Components", global, true);
            service.Save(components);

            var component = MockedContentTypes.CreateSimpleContentType("component", "Component", components, true);
            service.Save(component);

            var category = MockedContentTypes.CreateSimpleContentType("category", "Category", global, true);
            service.Save(category);

            var success = category.AddContentType(component);

            Assert.That(success, Is.False);
        }

        [Test]
        public void Can_Delete_Parent_ContentType_When_Child_Has_Content()
        {
            var cts = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true);
            cts.Save(contentType);
            var childContentType = MockedContentTypes.CreateSimpleContentType("childPage", "Child Page", contentType, true, "Child Content");
            cts.Save(childContentType);
            var cs = ServiceContext.ContentService;
            var content = cs.Create("Page 1", -1, childContentType.Alias);
            cs.Save(content);

            cts.Delete(contentType);

            Assert.IsNotNull(content.Id);
            Assert.AreNotEqual(0, content.Id);
            Assert.IsNotNull(childContentType.Id);
            Assert.AreNotEqual(0, childContentType.Id);
            Assert.IsNotNull(contentType.Id);
            Assert.AreNotEqual(0, contentType.Id);
            var deletedContent = cs.GetById(content.Id);
            var deletedChildContentType = cts.Get(childContentType.Id);
            var deletedContentType = cts.Get(contentType.Id);

            Assert.IsNull(deletedChildContentType);
            Assert.IsNull(deletedContent);
            Assert.IsNull(deletedContentType);
        }

        [Test]
        public void Can_Create_Container()
        {
            // Arrange
            var cts = ServiceContext.ContentTypeService;

            // Act
            var container = new EntityContainer(Constants.ObjectTypes.DocumentType);
            container.Name = "container1";
            cts.SaveContainer(container);

            // Assert
            var createdContainer = cts.GetContainer(container.Id);
            Assert.IsNotNull(createdContainer);
        }

        [Test]
        public void Can_Get_All_Containers()
        {
            // Arrange
            var cts = ServiceContext.ContentTypeService;

            // Act
            var container1 = new EntityContainer(Constants.ObjectTypes.DocumentType);
            container1.Name = "container1";
            cts.SaveContainer(container1);

            var container2 = new EntityContainer(Constants.ObjectTypes.DocumentType);
            container2.Name = "container2";
            cts.SaveContainer(container2);

            // Assert
            var containers = cts.GetContainers(new int[0]);
            Assert.AreEqual(2, containers.Count());
        }

        [Test]
        public void Deleting_ContentType_Sends_Correct_Number_Of_DeletedEntities_In_Events()
        {
            var cts = ServiceContext.ContentTypeService;
            var deletedEntities = 0;
            var contentType = MockedContentTypes.CreateSimpleContentType("page", "Page");
            cts.Save(contentType);

            ContentTypeService.Deleted += (sender, args) =>
            {
                deletedEntities += args.DeletedEntities.Count();
            };

            cts.Delete(contentType);

            Assert.AreEqual(deletedEntities, 1);
        }

        [Test]
        public void Deleting_Multiple_ContentTypes_Sends_Correct_Number_Of_DeletedEntities_In_Events()
        {
            var cts = ServiceContext.ContentTypeService;
            var deletedEntities = 0;
            var contentType = MockedContentTypes.CreateSimpleContentType("page", "Page");
            cts.Save(contentType);
            var contentType2 = MockedContentTypes.CreateSimpleContentType("otherPage", "Other page");
            cts.Save(contentType2);

            ContentTypeService.Deleted += (sender, args) =>
            {
                deletedEntities += args.DeletedEntities.Count();
            };

            cts.Delete(contentType);
            cts.Delete(contentType2);

            Assert.AreEqual(2, deletedEntities);
        }

        [Test]
        public void Deleting_ContentType_With_Child_Sends_Correct_Number_Of_DeletedEntities_In_Events()
        {
            var cts = ServiceContext.ContentTypeService;
            var deletedEntities = 0;
            var contentType = MockedContentTypes.CreateSimpleContentType("page", "Page");
            cts.Save(contentType);
            var contentType2 = MockedContentTypes.CreateSimpleContentType("subPage", "Sub page");
            contentType2.ParentId = contentType.Id;
            cts.Save(contentType2);

            ContentTypeService.Deleted += (sender, args) =>
            {
                deletedEntities += args.DeletedEntities.Count();
            };

            cts.Delete(contentType);

            Assert.AreEqual(2, deletedEntities);
        }

        [Test]
        public void Can_Remove_ContentType_Composition_From_ContentType()
        {
            //Test for U4-2234
            var cts = ServiceContext.ContentTypeService;
            //Arrange
            var component = CreateComponent();
            cts.Save(component);
            var banner = CreateBannerComponent(component);
            cts.Save(banner);
            var site = CreateSite();
            cts.Save(site);
            var homepage = CreateHomepage(site);
            cts.Save(homepage);

            //Add banner to homepage
            var added = homepage.AddContentType(banner);
            cts.Save(homepage);

            //Assert composition
            var bannerExists = homepage.ContentTypeCompositionExists(banner.Alias);
            var bannerPropertyExists = homepage.CompositionPropertyTypes.Any(x => x.Alias.Equals("bannerName"));
            Assert.That(added, Is.True);
            Assert.That(bannerExists, Is.True);
            Assert.That(bannerPropertyExists, Is.True);
            Assert.That(homepage.CompositionPropertyTypes.Count(), Is.EqualTo(6));

            //Remove banner from homepage
            var removed = homepage.RemoveContentType(banner.Alias);
            cts.Save(homepage);

            //Assert composition
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
            var service = ServiceContext.ContentTypeService;
            var metaContentType = MockedContentTypes.CreateMetaContentType();
            service.Save(metaContentType);

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", metaContentType) as IContentType;
            service.Save(simpleContentType);
            var categoryId = simpleContentType.Id;

            // Act
            var sut = simpleContentType.DeepCloneWithResetIdentities("newcategory");
            Assert.IsNotNull(sut);
            service.Save(sut);

            // Assert
            Assert.That(sut.HasIdentity, Is.True);

            var contentType = service.Get(sut.Id);
            var category = service.Get(categoryId);

            Assert.That(contentType.CompositionAliases().Any(x => x.Equals("meta")), Is.True);
            Assert.AreEqual(contentType.ParentId, category.ParentId);
            Assert.AreEqual(contentType.Level, category.Level);
            Assert.AreEqual(contentType.PropertyTypes.Count(), category.PropertyTypes.Count());
            Assert.AreNotEqual(contentType.Id, category.Id);
            Assert.AreNotEqual(contentType.Key, category.Key);
            Assert.AreNotEqual(contentType.Path, category.Path);
            Assert.AreNotEqual(contentType.SortOrder, category.SortOrder);
            Assert.AreNotEqual(contentType.PropertyTypes.First(x => x.Alias.Equals("title")).Id, category.PropertyTypes.First(x => x.Alias.Equals("title")).Id);
            Assert.AreNotEqual(contentType.PropertyGroups.First(x => x.Name.Equals("Content")).Id, category.PropertyGroups.First(x => x.Name.Equals("Content")).Id);
        }

        [Test]
        public void Can_Copy_ContentType_To_New_Parent_By_Performing_Clone()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;

            var parentContentType1 = MockedContentTypes.CreateSimpleContentType("parent1", "Parent1");
            service.Save(parentContentType1);
            var parentContentType2 = MockedContentTypes.CreateSimpleContentType("parent2", "Parent2", null, true);
            service.Save(parentContentType2);

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", parentContentType1, true) as IContentType;
            service.Save(simpleContentType);

            // Act
            var clone = simpleContentType.DeepCloneWithResetIdentities("newcategory");
            Assert.IsNotNull(clone);
            clone.RemoveContentType("parent1");
            clone.AddContentType(parentContentType2);
            clone.ParentId = parentContentType2.Id;
            service.Save(clone);

            // Assert
            Assert.That(clone.HasIdentity, Is.True);

            var clonedContentType = service.Get(clone.Id);
            var originalContentType = service.Get(simpleContentType.Id);

            Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
            Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

            Assert.AreEqual(clonedContentType.Path, "-1," + parentContentType2.Id + "," + clonedContentType.Id);
            Assert.AreEqual(clonedContentType.PropertyTypes.Count(), originalContentType.PropertyTypes.Count());

            Assert.AreNotEqual(clonedContentType.ParentId, originalContentType.ParentId);
            Assert.AreEqual(clonedContentType.ParentId, parentContentType2.Id);

            Assert.AreNotEqual(clonedContentType.Id, originalContentType.Id);
            Assert.AreNotEqual(clonedContentType.Key, originalContentType.Key);
            Assert.AreNotEqual(clonedContentType.Path, originalContentType.Path);

            Assert.AreNotEqual(clonedContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id, originalContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id);
            Assert.AreNotEqual(clonedContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id, originalContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id);
        }

        [Test]
        public void Can_Copy_ContentType_With_Service_To_Root()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var metaContentType = MockedContentTypes.CreateMetaContentType();
            service.Save(metaContentType);

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", metaContentType);
            service.Save(simpleContentType);
            var categoryId = simpleContentType.Id;

            // Act
            var clone = service.Copy(simpleContentType, "newcategory", "new category");

            // Assert
            Assert.That(clone.HasIdentity, Is.True);

            var cloned = service.Get(clone.Id);
            var original = service.Get(categoryId);

            Assert.That(cloned.CompositionAliases().Any(x => x.Equals("meta")), Is.False); //it's been copied to root
            Assert.AreEqual(cloned.ParentId, -1);
            Assert.AreEqual(cloned.Level, 1);
            Assert.AreEqual(cloned.PropertyTypes.Count(), original.PropertyTypes.Count());
            Assert.AreEqual(cloned.PropertyGroups.Count(), original.PropertyGroups.Count());

            for (int i = 0; i < cloned.PropertyGroups.Count; i++)
            {
                Assert.AreEqual(cloned.PropertyGroups[i].PropertyTypes.Count, original.PropertyGroups[i].PropertyTypes.Count);
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
            Assert.AreNotEqual(cloned.PropertyTypes.First(x => x.Alias.Equals("title")).Id, original.PropertyTypes.First(x => x.Alias.Equals("title")).Id);
            Assert.AreNotEqual(cloned.PropertyGroups.First(x => x.Name.Equals("Content")).Id, original.PropertyGroups.First(x => x.Name.Equals("Content")).Id);
        }

        [Test]
        public void Can_Copy_ContentType_To_New_Parent_With_Service()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;

            var parentContentType1 = MockedContentTypes.CreateSimpleContentType("parent1", "Parent1");
            service.Save(parentContentType1);
            var parentContentType2 = MockedContentTypes.CreateSimpleContentType("parent2", "Parent2", null, true);
            service.Save(parentContentType2);

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", parentContentType1, true);
            service.Save(simpleContentType);

            // Act
            var clone = service.Copy(simpleContentType, "newAlias", "new alias", parentContentType2);

            // Assert
            Assert.That(clone.HasIdentity, Is.True);

            var clonedContentType = service.Get(clone.Id);
            var originalContentType = service.Get(simpleContentType.Id);

            Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent2")), Is.True);
            Assert.That(clonedContentType.CompositionAliases().Any(x => x.Equals("parent1")), Is.False);

            Assert.AreEqual(clonedContentType.Path, "-1," + parentContentType2.Id + "," + clonedContentType.Id);
            Assert.AreEqual(clonedContentType.PropertyTypes.Count(), originalContentType.PropertyTypes.Count());

            Assert.AreNotEqual(clonedContentType.ParentId, originalContentType.ParentId);
            Assert.AreEqual(clonedContentType.ParentId, parentContentType2.Id);

            Assert.AreNotEqual(clonedContentType.Id, originalContentType.Id);
            Assert.AreNotEqual(clonedContentType.Key, originalContentType.Key);
            Assert.AreNotEqual(clonedContentType.Path, originalContentType.Path);

            Assert.AreNotEqual(clonedContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id, originalContentType.PropertyTypes.First(x => x.Alias.StartsWith("title")).Id);
            Assert.AreNotEqual(clonedContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id, originalContentType.PropertyGroups.First(x => x.Name.StartsWith("Content")).Id);
        }

        [Test]
        public void Cannot_Add_Duplicate_PropertyType_Alias_To_Referenced_Composition()
        {
            //Related the second issue in screencast from this post http://issues.umbraco.org/issue/U4-5986

            // Arrange
            var service = ServiceContext.ContentTypeService;

            var parent = MockedContentTypes.CreateSimpleContentType();
            service.Save(parent);
            var child = MockedContentTypes.CreateSimpleContentType("simpleChildPage", "Simple Child Page", parent, true);
            service.Save(child);
            var composition = MockedContentTypes.CreateMetaContentType();
            service.Save(composition);

            //Adding Meta-composition to child doc type
            child.AddContentType(composition);
            service.Save(child);

            // Act
            var duplicatePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var added = composition.AddPropertyType(duplicatePropertyType, "Meta");

            // Assert
            Assert.That(added, Is.True);
            Assert.Throws<InvalidCompositionException>(() => service.Save(composition));
            Assert.DoesNotThrow(() => service.Get("simpleChildPage"));
        }

        [Test]
        public void Cannot_Add_Duplicate_PropertyType_Alias_In_Composition_Graph()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;

            var basePage = MockedContentTypes.CreateSimpleContentType("basePage", "Base Page", null, true);
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage, true);
            service.Save(advancedPage);

            var metaComposition = MockedContentTypes.CreateMetaContentType();
            service.Save(metaComposition);
            var seoComposition = MockedContentTypes.CreateSeoContentType();
            service.Save(seoComposition);

            var metaAdded = contentPage.AddContentType(metaComposition);
            service.Save(contentPage);
            var seoAdded = advancedPage.AddContentType(seoComposition);
            service.Save(advancedPage);

            // Act
            var duplicatePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var addedToBasePage = basePage.AddPropertyType(duplicatePropertyType, "Content");
            var addedToAdvancedPage = advancedPage.AddPropertyType(duplicatePropertyType, "Content");
            var addedToMeta = metaComposition.AddPropertyType(duplicatePropertyType, "Meta");
            var addedToSeo = seoComposition.AddPropertyType(duplicatePropertyType, "Seo");

            // Assert
            Assert.That(metaAdded, Is.True);
            Assert.That(seoAdded, Is.True);

            Assert.That(addedToBasePage, Is.True);
            Assert.That(addedToAdvancedPage, Is.False);
            Assert.That(addedToMeta, Is.True);
            Assert.That(addedToSeo, Is.True);

            Assert.Throws<InvalidCompositionException>(() => service.Save(basePage));
            Assert.Throws<InvalidCompositionException>(() => service.Save(metaComposition));
            Assert.Throws<InvalidCompositionException>(() => service.Save(seoComposition));

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
            Assert.DoesNotThrow(() => service.Get("meta"));
            Assert.DoesNotThrow(() => service.Get("seo"));
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
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var compositionAdded = advancedPage.AddContentType(contentMetaComposition);
            service.Save(advancedPage);

            //NOTE: It should not be possible to Save 'BasePage' with the Title PropertyType added
            var titlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var titleAdded = basePage.AddPropertyType(titlePropertyType, "Content");

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(titleAdded, Is.True);
            Assert.That(compositionAdded, Is.True);

            Assert.Throws<InvalidCompositionException>(() => service.Save(basePage));

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
        }

        [Test]
        public void Cannot_Save_ContentType_With_Empty_Name()
        {
            // Arrange
            var contentType = MockedContentTypes.CreateSimpleContentType("contentType", string.Empty);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => ServiceContext.ContentTypeService.Save(contentType));
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
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);
            var moreAdvancedPage = MockedContentTypes.CreateBasicContentType("moreAdvancedPage", "More Advanced Page", advancedPage);
            service.Save(moreAdvancedPage);

            var seoComposition = MockedContentTypes.CreateSeoContentType();
            service.Save(seoComposition);
            var metaComposition = MockedContentTypes.CreateMetaContentType();
            service.Save(metaComposition);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "Content");
            service.Save(advancedPage);

            var titlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var titleAdded = seoComposition.AddPropertyType(titlePropertyType, "Content");
            service.Save(seoComposition);

            var seoCompositionAdded = advancedPage.AddContentType(seoComposition);
            var metaCompositionAdded = moreAdvancedPage.AddContentType(metaComposition);
            service.Save(advancedPage);
            service.Save(moreAdvancedPage);

            var keywordsPropertyType = metaComposition.PropertyTypes.First(x => x.Alias.Equals("metakeywords"));
            keywordsPropertyType.Alias = "title";

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(titleAdded, Is.True);
            Assert.That(seoCompositionAdded, Is.True);
            Assert.That(metaCompositionAdded, Is.True);

            Assert.Throws<InvalidCompositionException>(() => service.Save(metaComposition));

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
            Assert.DoesNotThrow(() => service.Get("moreAdvancedPage"));
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
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);
            var moreAdvancedPage = MockedContentTypes.CreateBasicContentType("moreAdvancedPage", "More Advanced Page", advancedPage);
            service.Save(moreAdvancedPage);

            var seoComposition = MockedContentTypes.CreateSeoContentType();
            service.Save(seoComposition);
            var metaComposition = MockedContentTypes.CreateMetaContentType();
            service.Save(metaComposition);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "Content");
            service.Save(advancedPage);

            var titlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var titleAdded = seoComposition.AddPropertyType(titlePropertyType, "Content");
            service.Save(seoComposition);

            var seoCompositionAdded = advancedPage.AddContentType(seoComposition);
            var metaCompositionAdded = moreAdvancedPage.AddContentType(metaComposition);
            service.Save(advancedPage);
            service.Save(moreAdvancedPage);

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(titleAdded, Is.True);
            Assert.That(seoCompositionAdded, Is.True);
            Assert.That(metaCompositionAdded, Is.True);

            var testPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "test")
            {
                 Name = "Test", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var testAdded = seoComposition.AddPropertyType(testPropertyType, "Content");
            service.Save(seoComposition);

            Assert.That(testAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
            Assert.DoesNotThrow(() => service.Get("moreAdvancedPage"));
        }

        [Test]
        public void Cannot_Rename_PropertyGroup_On_Child_Avoiding_Conflict_With_Parent_PropertyGroup()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true, "Content");
            service.Save(page);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true, "Content 2");
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage, true, "Details");
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "content", "Content");
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "content", "Content");
            service.Save(contentPage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            // Change the name of the tab on the "root" content type 'page'
            var propertyGroup = contentPage.PropertyGroups["Content 2"];
            Assert.Throws<ArgumentException>(() => contentPage.PropertyGroups.Add(new PropertyGroup(true)
            {
                Id = propertyGroup.Id,
                Name = "Content",
                Alias = "content",
                SortOrder = 0
            }));

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
        }

        [Test]
        public void Cannot_Rename_PropertyType_Alias_Causing_Conflicts_With_Parents()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);

            // Act
            var titlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var titleAdded = basePage.AddPropertyType(titlePropertyType, "Content");
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = contentPage.AddPropertyType(bodyTextPropertyType, "Content");
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = advancedPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(basePage);
            service.Save(contentPage);
            service.Save(advancedPage);

            //Rename the PropertyType to something that already exists in the Composition - NOTE this should not be allowed and Saving should throw an exception
            var authorPropertyTypeToRename = advancedPage.PropertyTypes.First(x => x.Alias.Equals("author"));
            authorPropertyTypeToRename.Alias = "title";

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(titleAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);

            Assert.Throws<InvalidCompositionException>(() => service.Save(advancedPage));

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));
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
            var service = ServiceContext.ContentTypeService;

            var basePage = MockedContentTypes.CreateSimpleContentType("basePage", "Base Page", null, true);
            service.Save(basePage);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", basePage, true);
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage, true);
            service.Save(advancedPage);

            var metaComposition = MockedContentTypes.CreateMetaContentType();
            service.Save(metaComposition);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            var metaAdded = contentPage.AddContentType(metaComposition);
            service.Save(contentPage);

            var metaAddedToComposition = contentMetaComposition.AddContentType(metaComposition);
            service.Save(contentMetaComposition);

            // Act
            var propertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var addedToContentPage = contentPage.AddPropertyType(propertyType, "content", "Content");

            // Assert
            Assert.That(metaAdded, Is.True);
            Assert.That(metaAddedToComposition, Is.True);

            Assert.That(addedToContentPage, Is.True);
            Assert.DoesNotThrow(() => service.Save(contentPage));
        }

        [Test]
        public void Can_Rename_PropertyGroup_With_Inherited_PropertyGroups()
        {
            //Related the first issue in screencast from this post http://issues.umbraco.org/issue/U4-5986

            // Arrange
            var service = ServiceContext.ContentTypeService;

            // create 'page' content type with a 'Content 2' group
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, false, "Content 2");
            Assert.AreEqual(1, page.PropertyGroups.Count);
            Assert.AreEqual("Content 2", page.PropertyGroups.First().Name);
            Assert.AreEqual(3, page.PropertyTypes.Count());
            Assert.AreEqual("Title", page.PropertyTypes.First().Name);
            Assert.AreEqual("Body Text", page.PropertyTypes.Skip(1).First().Name);
            Assert.AreEqual("Author", page.PropertyTypes.Skip(2).First().Name);
            service.Save(page);

            // create 'contentPage' content type as a child of 'page'
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true);
            Assert.AreEqual(1, page.PropertyGroups.Count);
            Assert.AreEqual("Content 2", page.PropertyGroups.First().Name);
            Assert.AreEqual(3, contentPage.PropertyTypes.Count());
            Assert.AreEqual("Title", contentPage.PropertyTypes.First().Name);
            Assert.AreEqual("Body Text", contentPage.PropertyTypes.Skip(1).First().Name);
            Assert.AreEqual("Author", contentPage.PropertyTypes.Skip(2).First().Name);
            service.Save(contentPage);

            // add 'Content' group to 'meta' content type
            var meta = MockedContentTypes.CreateMetaContentType();
            Assert.AreEqual(1, meta.PropertyGroups.Count);
            Assert.AreEqual("Meta", meta.PropertyGroups.First().Name);
            Assert.AreEqual(2, meta.PropertyTypes.Count());
            Assert.AreEqual("Meta Keywords", meta.PropertyTypes.First().Name);
            Assert.AreEqual("Meta Description", meta.PropertyTypes.Skip(1).First().Name);
            meta.AddPropertyGroup("content", "Content");
            Assert.AreEqual(2, meta.PropertyTypes.Count());
            service.Save(meta);

            // add 'meta' content type to 'contentPage' composition
            contentPage.AddContentType(meta);
            service.Save(contentPage);

            // add property 'prop1' to 'contentPage' group 'Content_'
            var prop1 = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "testTextbox")
            {
                 Name = "Test Textbox", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var prop1Added = contentPage.AddPropertyType(prop1, "content2", "Content 2");
            Assert.IsTrue(prop1Added);

            // add property 'prop2' to 'contentPage' group 'Content'
            var prop2 = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "anotherTextbox")
            {
                 Name = "Another Test Textbox", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var prop2Added = contentPage.AddPropertyType(prop2, "content2", "Content 2");
            Assert.IsTrue(prop2Added);

            // save 'contentPage' content type
            service.Save(contentPage);

            var group = page.PropertyGroups["content2"];
            group.Name = "ContentTab"; // rename the group
            service.Save(page);
            Assert.AreEqual(3, page.PropertyTypes.Count());

            // get 'contentPage' content type again
            var contentPageAgain = service.Get("contentPage");
            Assert.IsNotNull(contentPageAgain);

            // assert that 'Content_' group is still there because we don't propagate renames
            var findGroup = contentPageAgain.CompositionPropertyGroups.FirstOrDefault(x => x.Name == "Content 2");
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
            var service = ServiceContext.ContentTypeService;
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true, "Content 2");
            service.Save(page);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true, "Contentx");
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage, true, "Contenty");
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);
            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = contentPage.AddPropertyType(bodyTextPropertyType, "Content 2");//Will be added to the parent tab
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");//Will be added to the "Content Meta" composition
            service.Save(contentPage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var descriptionPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "description")
            {
                 Name = "Description", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var keywordsPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "keywords")
            {
                 Name = "Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = advancedPage.AddPropertyType(authorPropertyType, "Content 2");//Will be added to an ancestor tab
            var descriptionAdded = advancedPage.AddPropertyType(descriptionPropertyType, "Contentx");//Will be added to a parent tab
            var keywordsAdded = advancedPage.AddPropertyType(keywordsPropertyType, "Content");//Will be added to the "Content Meta" composition
            service.Save(advancedPage);

            //Change the name of the tab on the "root" content type 'page'.
            var propertyGroup = page.PropertyGroups["Content 2"];
            page.PropertyGroups.Add(new PropertyGroup(true) { Id = propertyGroup.Id, Name = "Content", SortOrder = 0 });
            service.Save(page);

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(descriptionAdded, Is.True);
            Assert.That(keywordsAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));

            var advancedPageReloaded = service.Get("advancedPage");
            var content2TabExists = advancedPageReloaded.CompositionPropertyGroups.Any(x => x.Name.Equals("Content 2"));

            // now is true, because we don't propagate renames anymore
            Assert.That(content2TabExists, Is.True);

            var numberOfContentTabs = advancedPageReloaded.CompositionPropertyGroups.Count(x => x.Name.Equals("Content"));
            Assert.That(numberOfContentTabs, Is.EqualTo(4));
        }

        [Test]
        public void Can_Rename_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups_v2()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true, "Content_");
            service.Save(page);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true, "Content");
            service.Save(contentPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = page.AddPropertyType(bodyTextPropertyType, "Content_");
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content_");
            service.Save(page);
            service.Save(contentPage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            //Change the name of the tab on the "root" content type 'page'.
            var propertyGroup = page.PropertyGroups["Content_"];
            page.PropertyGroups.Add(new PropertyGroup(true) { Id = propertyGroup.Id, Name = "Content", SortOrder = 0 });
            service.Save(page);

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
        }

        [Test]
        public void Can_Remove_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);

            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);

            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            basePage.RemovePropertyGroup("Content");
            service.Save(basePage);

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(compositionAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));

            var contentType = service.Get("contentPage");
            var propertyGroup = contentType.PropertyGroups["Content"];
        }

        [Test]
        public void Can_Remove_PropertyGroup_Without_Removing_Property_Types()
        {
            var service = ServiceContext.ContentTypeService;

            var basePage = (IContentType) MockedContentTypes.CreateBasicContentType();
            basePage.AddPropertyGroup("Content");
            basePage.AddPropertyGroup("Meta");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                Name = "Author",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
            Assert.IsTrue(basePage.AddPropertyType(authorPropertyType, "Content"));

            var titlePropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "title")
            {
                Name = "Title",
                Description = "",
                Mandatory = false,
                SortOrder = 1,
                DataTypeId = -88
            };
            Assert.IsTrue(basePage.AddPropertyType(titlePropertyType, "Meta"));

            service.Save(basePage);
            basePage = service.Get(basePage.Id);

            var count = basePage.PropertyTypes.Count();
            Assert.AreEqual(2, count);

            basePage.RemovePropertyGroup("Content");

            service.Save(basePage);
            basePage = service.Get(basePage.Id);

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
            var service = ServiceContext.ContentTypeService;
            var basePage = MockedContentTypes.CreateBasicContentType();
            service.Save(basePage);

            var contentPage = MockedContentTypes.CreateBasicContentType("contentPage", "Content Page", basePage);
            service.Save(contentPage);

            var advancedPage = MockedContentTypes.CreateBasicContentType("advancedPage", "Advanced Page", contentPage);
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(compositionAdded, Is.True);

            Assert.DoesNotThrow(() => service.Get("contentPage"));
            Assert.DoesNotThrow(() => service.Get("advancedPage"));

            var contentType = service.Get("contentPage");
            var propertyGroup = contentType.PropertyGroups["Content"];

            var numberOfContentTabs = contentType.CompositionPropertyGroups.Count(x => x.Name.Equals("Content"));
            Assert.That(numberOfContentTabs, Is.EqualTo(3));

            //Ensure that adding a new PropertyType to the "Content"-tab also adds it to the right group

            var descriptionPropertyType = new PropertyType(Constants.PropertyEditors.Aliases.TextBox, ValueStorageType.Ntext)
            {
                Alias = "description", Name = "Description", Description = "",  Mandatory = false, SortOrder = 1,DataTypeId = -88
            };
            var descriptionAdded = contentType.AddPropertyType(descriptionPropertyType, "Content");
            service.Save(contentType);
            Assert.That(descriptionAdded, Is.True);

            var contentPageReloaded = service.Get("contentPage");
            var propertyGroupReloaded = contentPageReloaded.PropertyGroups["Content"];
            var hasDescriptionPropertyType = propertyGroupReloaded.PropertyTypes.Contains("description");
            Assert.That(hasDescriptionPropertyType, Is.True);

            var descriptionPropertyTypeReloaded = propertyGroupReloaded.PropertyTypes["description"];
            Assert.That(descriptionPropertyTypeReloaded.PropertyGroupId.IsValueCreated, Is.False);
        }

        [Test]
        public void Empty_Description_Is_Always_Null_After_Saving_Content_Type()
        {
            var service = ServiceContext.ContentTypeService;
            var contentType = MockedContentTypes.CreateBasicContentType();
            contentType.Description = null;
            service.Save(contentType);

            var contentType2 = MockedContentTypes.CreateBasicContentType("basePage2", "Base Page 2");
            contentType2.Description = string.Empty;
            service.Save(contentType2);

            Assert.IsNull(contentType.Description);
            Assert.IsNull(contentType2.Description);
        }

        [Test]
        public void Variations_In_Compositions()
        {
            var service = ServiceContext.ContentTypeService;
            var typeA = MockedContentTypes.CreateSimpleContentType("a", "A");
            typeA.Variations = ContentVariation.Culture; // make it variant
            typeA.PropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations = ContentVariation.Culture; // with a variant property
            service.Save(typeA);

            var typeB = MockedContentTypes.CreateSimpleContentType("b", "B", typeA, true);
            typeB.Variations = ContentVariation.Nothing; // make it invariant
            service.Save(typeB);

            var typeC = MockedContentTypes.CreateSimpleContentType("c", "C", typeA, true);
            typeC.Variations = ContentVariation.Culture; // make it variant
            service.Save(typeC);

            // property is variant on A
            var test = service.Get(typeA.Id);
            Assert.AreEqual(ContentVariation.Culture, test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
            Assert.AreEqual(ContentVariation.Culture, test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);

            // but not on B
            test = service.Get(typeB.Id);
            Assert.AreEqual(ContentVariation.Nothing, test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
            Assert.AreEqual(ContentVariation.Nothing, test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);

            // but on C
            test = service.Get(typeC.Id);
            Assert.AreEqual(ContentVariation.Culture, test.CompositionPropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
            Assert.AreEqual(ContentVariation.Culture, test.CompositionPropertyGroups.Last().PropertyTypes.First(x => x.Alias.InvariantEquals("title")).Variations);
        }

        private ContentType CreateComponent()
        {
            var component = new ContentType(-1)
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

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext, "componentGroup") { Name = "Component Group", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            component.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Component", SortOrder = 1 });

            return component;
        }

        private ContentType CreateBannerComponent(ContentType parent)
        {
            const string contentTypeAlias = "banner";
            var banner = new ContentType(parent, contentTypeAlias)
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

            var propertyType = new PropertyType("test", ValueStorageType.Ntext, "bannerName")
            {
                Name = "Banner Name",
                Description = "",
                Mandatory = false,
                SortOrder = 2,
                DataTypeId = -88
            };
            banner.AddPropertyType(propertyType, "Component");
            return banner;
        }

        private ContentType CreateSite()
        {
            var site = new ContentType(-1)
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

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext, "hostname") { Name = "Hostname", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            site.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Site Settings", SortOrder = 1 });

            return site;
        }

        private ContentType CreateHomepage(ContentType parent)
        {
            const string contentTypeAlias = "homepage";
            var contentType = new ContentType(parent, contentTypeAlias)
            {
                Alias = contentTypeAlias,
                Name = "Homepage",
                Description = "ContentType used for the Homepage",
                Icon = ".sprTreeDoc3",
                Thumbnail = "doc.png",
                SortOrder = 1,
                CreatorId = 0,
                Trashed = false
            };

            var contentCollection = new PropertyTypeCollection(true);
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext, "title") { Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeId = -88 });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext, "bodyText") { Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeId = -87 });
            contentCollection.Add(new PropertyType("test", ValueStorageType.Ntext, "author") { Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

        private IContentType[] CreateContentTypeHierarchy()
        {
            //create the master type
            var masterContentType = MockedContentTypes.CreateSimpleContentType("masterContentType", "MasterContentType");
            masterContentType.Key = new Guid("C00CA18E-5A9D-483B-A371-EECE0D89B4AE");
            ServiceContext.ContentTypeService.Save(masterContentType);

            //add the one we just created
            var list = new List<IContentType> { masterContentType };

            for (var i = 0; i < 10; i++)
            {
                var contentType = MockedContentTypes.CreateSimpleContentType("childType" + i, "ChildType" + i,
                    //make the last entry in the list, this one's parent
                    list.Last(), true);

                list.Add(contentType);
            }

            return list.ToArray();
        }
    }
}
