using System.Runtime.Remoting;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;

using Umbraco.Tests.CodeFirst.TestModels.Composition;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerTest)]
    [TestFixture, RequiresSTA]
    public class ContentTypeServiceTests : BaseServiceTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        [Test]
        public void Deleting_PropertyType_Removes_The_Property_From_Content()
        {
            IContentType contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "Test1");
            ServiceContext.ContentTypeService.Save(contentType1);
            IContent contentItem = MockedContent.CreateTextpageContent(contentType1, "Testing", -1);
            ServiceContext.ContentService.SaveAndPublishWithStatus(contentItem);
            var initProps = contentItem.Properties.Count;
            var initPropTypes = contentItem.PropertyTypes.Count();

            //remove a property
            contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
            ServiceContext.ContentTypeService.Save(contentType1);

            //re-load it from the db
            contentItem = ServiceContext.ContentService.GetById(contentItem.Id);

            Assert.AreEqual(initPropTypes - 1, contentItem.PropertyTypes.Count());
            Assert.AreEqual(initProps - 1, contentItem.Properties.Count);
        }

        [Test]
        public void Rebuild_Content_Xml_On_Alias_Change()
        {
            var contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "Test1");
            var contentType2 = MockedContentTypes.CreateTextpageContentType("test2", "Test2");
            ServiceContext.ContentTypeService.Save(contentType1);
            ServiceContext.ContentTypeService.Save(contentType2);
            var contentItems1 = MockedContent.CreateTextpageContent(contentType1, -1, 10).ToArray();
            contentItems1.ForEach(x => ServiceContext.ContentService.SaveAndPublishWithStatus(x));
            var contentItems2 = MockedContent.CreateTextpageContent(contentType2, -1, 5).ToArray();
            contentItems2.ForEach(x => ServiceContext.ContentService.SaveAndPublishWithStatus(x));
            //only update the contentType1 alias which will force an xml rebuild for all content of that type
            contentType1.Alias = "newAlias";
            ServiceContext.ContentTypeService.Save(contentType1);

            foreach (var c in contentItems1)
            {
                var xml = DatabaseContext.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                Assert.IsNotNull(xml);
                Assert.IsTrue(xml.Xml.StartsWith("<newAlias"));
            }

            foreach (var c in contentItems2)
            {
                var xml = DatabaseContext.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                Assert.IsNotNull(xml);
                Assert.IsTrue(xml.Xml.StartsWith("<test2")); //should remain the same
            }
        }

        [Test]
        public void Rebuild_Content_Xml_On_Property_Removal()
        {
            var contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "Test1");
            ServiceContext.ContentTypeService.Save(contentType1);
            var contentItems1 = MockedContent.CreateTextpageContent(contentType1, -1, 10).ToArray();
            contentItems1.ForEach(x => ServiceContext.ContentService.SaveAndPublishWithStatus(x));
            var alias = contentType1.PropertyTypes.First().Alias;
            var elementToMatch = "<" + alias + ">";

            foreach (var c in contentItems1)
            {
                var xml = DatabaseContext.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                Assert.IsNotNull(xml);
                Assert.IsTrue(xml.Xml.Contains(elementToMatch)); //verify that it is there before we remove the property
            }

            //remove a property
            contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
            ServiceContext.ContentTypeService.Save(contentType1);

            var reQueried = ServiceContext.ContentTypeService.GetContentType(contentType1.Id);
            var reContent = ServiceContext.ContentService.GetById(contentItems1.First().Id);

            foreach (var c in contentItems1)
            {
                var xml = DatabaseContext.Database.FirstOrDefault<ContentXmlDto>("WHERE nodeId = @Id", new { Id = c.Id });
                Assert.IsNotNull(xml);
                Assert.IsFalse(xml.Xml.Contains(elementToMatch)); //verify that it is no longer there
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
            var descendants = master.Descendants();

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
            var descendants = master.DescendantsAndSelf();

            //Assert
            Assert.AreEqual(11, descendants.Count());
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
            var dtdYesNo = ServiceContext.DataTypeService.GetDataTypeDefinitionById(-49);
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
            var homeDoc = cs.CreateContent("Home Page", -1, contentTypeAlias);
            cs.SaveAndPublishWithStatus(homeDoc);

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

            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title") { Name = "Title", Description = "", Mandatory = false, DataTypeDefinitionId = -88 });
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.TinyMCEAlias, DataTypeDatabaseType.Ntext, "bodyText") { Name = "Body Text", Description = "", Mandatory = false, DataTypeDefinitionId = -87 });
            contentType.AddPropertyType(new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author") { Name = "Author", Description = "Name of the author", Mandatory = false, DataTypeDefinitionId = -88 });

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

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", metaContentType);
            service.Save(simpleContentType);
            var categoryId = simpleContentType.Id;

            // Act
            var sut = simpleContentType.DeepCloneWithResetIdentities("newcategory");
            service.Save(sut);

            // Assert
            Assert.That(sut.HasIdentity, Is.True);

            var contentType = service.GetContentType(sut.Id);
            var category = service.GetContentType(categoryId);

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

            var simpleContentType = MockedContentTypes.CreateSimpleContentType("category", "Category", parentContentType1, true);
            service.Save(simpleContentType);

            // Act
            var clone = simpleContentType.DeepCloneWithResetIdentities("newcategory");
            clone.RemoveContentType("parent1");
            clone.AddContentType(parentContentType2);
            clone.ParentId = parentContentType2.Id;
            service.Save(clone);

            // Assert
            Assert.That(clone.HasIdentity, Is.True);

            var clonedContentType = service.GetContentType(clone.Id);
            var originalContentType = service.GetContentType(simpleContentType.Id);

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

            var cloned = service.GetContentType(clone.Id);
            var original = service.GetContentType(categoryId);

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

            var clonedContentType = service.GetContentType(clone.Id);
            var originalContentType = service.GetContentType(simpleContentType.Id);

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
            var duplicatePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var added = composition.AddPropertyType(duplicatePropertyType, "Meta");

            // Assert
            Assert.That(added, Is.True);
            Assert.Throws<Exception>(() => service.Save(composition));
            Assert.DoesNotThrow(() => service.GetContentType("simpleChildPage"));
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
            var duplicatePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
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

            Assert.Throws<Exception>(() => service.Save(basePage));
            Assert.Throws<Exception>(() => service.Save(metaComposition));
            Assert.Throws<Exception>(() => service.Save(seoComposition));

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
            Assert.DoesNotThrow(() => service.GetContentType("meta"));
            Assert.DoesNotThrow(() => service.GetContentType("seo"));
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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);
            
            var compositionAdded = advancedPage.AddContentType(contentMetaComposition);
            service.Save(advancedPage);

            //NOTE: It should not be possible to Save 'BasePage' with the Title PropertyType added
            var titlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var titleAdded = basePage.AddPropertyType(titlePropertyType, "Content");

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(titleAdded, Is.True);
            Assert.That(compositionAdded, Is.True);

            Assert.Throws<Exception>(() => service.Save(basePage));

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "Content");
            service.Save(advancedPage);

            var titlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
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

            Assert.Throws<Exception>(() => service.Save(metaComposition));

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
            Assert.DoesNotThrow(() => service.GetContentType("moreAdvancedPage"));
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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitleAdded = advancedPage.AddPropertyType(subtitlePropertyType, "Content");
            service.Save(advancedPage);

            var titlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
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

            var testPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "test")
            {
                 Name = "Test", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var testAdded = seoComposition.AddPropertyType(testPropertyType, "Content");
            service.Save(seoComposition);

            Assert.That(testAdded, Is.True);

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
            Assert.DoesNotThrow(() => service.GetContentType("moreAdvancedPage"));
        }

        [Test]
        public void Cannot_Rename_PropertyGroup_On_Child_Avoiding_Conflict_With_Parent_PropertyGroup()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true, "Content");
            service.Save(page);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true, "Content_");
            service.Save(contentPage);
            var advancedPage = MockedContentTypes.CreateSimpleContentType("advancedPage", "Advanced Page", contentPage, true, "Details");
            service.Save(advancedPage);

            var contentMetaComposition = MockedContentTypes.CreateContentMetaContentType();
            service.Save(contentMetaComposition);

            // Act
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            //Change the name of the tab on the "root" content type 'page'.
            var propertyGroup = contentPage.PropertyGroups["Content_"];
            Assert.Throws<Exception>(() => contentPage.PropertyGroups.Add(new PropertyGroup
            {
                Id = propertyGroup.Id,
                Name = "Content",
                SortOrder = 0
            }));

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
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
            var titlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var titleAdded = basePage.AddPropertyType(titlePropertyType, "Content");
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = contentPage.AddPropertyType(bodyTextPropertyType, "Content");
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 
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

            Assert.Throws<Exception>(() => service.Save(advancedPage));

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));
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
            var propertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "title")
            {
                 Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var addedToContentPage = contentPage.AddPropertyType(propertyType, "Content");

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

            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, false, "Content_");
            service.Save(page);
            var contentPage = MockedContentTypes.CreateSimpleContentType("contentPage", "Content Page", page, true);
            service.Save(contentPage);
            var composition = MockedContentTypes.CreateMetaContentType();
            composition.AddPropertyGroup("Content");
            service.Save(composition);
            //Adding Meta-composition to child doc type
            contentPage.AddContentType(composition);
            service.Save(contentPage);

            // Act
            var propertyTypeOne = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "testTextbox")
            {
                 Name = "Test Textbox", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var firstOneAdded = contentPage.AddPropertyType(propertyTypeOne, "Content_");
            var propertyTypeTwo = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "anotherTextbox")
            {
                 Name = "Another Test Textbox", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var secondOneAdded = contentPage.AddPropertyType(propertyTypeTwo, "Content");
            service.Save(contentPage);

            Assert.That(page.PropertyGroups.Contains("Content_"), Is.True);
            var propertyGroup = page.PropertyGroups["Content_"];
            page.PropertyGroups.Add(new PropertyGroup{ Id = propertyGroup.Id, Name = "ContentTab", SortOrder = 0});
            service.Save(page);

            // Assert
            Assert.That(firstOneAdded, Is.True);
            Assert.That(secondOneAdded, Is.True);

            var contentType = service.GetContentType("contentPage");
            Assert.That(contentType, Is.Not.Null);

            var compositionPropertyGroups = contentType.CompositionPropertyGroups;
            Assert.That(compositionPropertyGroups.Count(x => x.Name.Equals("Content_")), Is.EqualTo(0));

            var propertyTypeCount = contentType.PropertyTypes.Count();
            var compPropertyTypeCount = contentType.CompositionPropertyTypes.Count();
            Assert.That(propertyTypeCount, Is.EqualTo(5));
            Assert.That(compPropertyTypeCount, Is.EqualTo(10));
        }

        [Test]
        public void Can_Rename_PropertyGroup_On_Parent_Without_Causing_Duplicate_PropertyGroups()
        {
            // Arrange
            var service = ServiceContext.ContentTypeService;
            var page = MockedContentTypes.CreateSimpleContentType("page", "Page", null, true, "Content_");
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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = contentPage.AddPropertyType(bodyTextPropertyType, "Content_");//Will be added to the parent tab
            var subtitleAdded = contentPage.AddPropertyType(subtitlePropertyType, "Content");//Will be added to the "Content Meta" composition
            service.Save(contentPage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var descriptionPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "description")
            {
                 Name = "Description", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var keywordsPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "keywords")
            {
                 Name = "Keywords", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorAdded = advancedPage.AddPropertyType(authorPropertyType, "Content_");//Will be added to an ancestor tab
            var descriptionAdded = advancedPage.AddPropertyType(descriptionPropertyType, "Contentx");//Will be added to a parent tab
            var keywordsAdded = advancedPage.AddPropertyType(keywordsPropertyType, "Content");//Will be added to the "Content Meta" composition
            service.Save(advancedPage);

            //Change the name of the tab on the "root" content type 'page'.
            var propertyGroup = page.PropertyGroups["Content_"];
            page.PropertyGroups.Add(new PropertyGroup { Id = propertyGroup.Id, Name = "Content", SortOrder = 0 });
            service.Save(page);

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(descriptionAdded, Is.True);
            Assert.That(keywordsAdded, Is.True);

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));

            var advancedPageReloaded = service.GetContentType("advancedPage");
            var contentUnderscoreTabExists = advancedPageReloaded.CompositionPropertyGroups.Any(x => x.Name.Equals("Content_"));
            Assert.That(contentUnderscoreTabExists, Is.False);

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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var subtitlePropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "subtitle")
            {
                 Name = "Subtitle", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
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
            page.PropertyGroups.Add(new PropertyGroup { Id = propertyGroup.Id, Name = "Content", SortOrder = 0 });
            service.Save(page);

            // Assert
            Assert.That(compositionAdded, Is.True);
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(subtitleAdded, Is.True);
            Assert.That(authorAdded, Is.True);

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
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
            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
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

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));

            var contentType = service.GetContentType("contentPage");
            var propertyGroup = contentType.PropertyGroups["Content"];
            Assert.That(propertyGroup.ParentId.HasValue, Is.False);
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
            var authorPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "author")
            {
                 Name = "Author", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var authorAdded = contentPage.AddPropertyType(authorPropertyType, "Content");
            service.Save(contentPage);

            var bodyTextPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext, "bodyText")
            {
                 Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88
            };
            var bodyTextAdded = basePage.AddPropertyType(bodyTextPropertyType, "Content");
            service.Save(basePage);

            var compositionAdded = contentPage.AddContentType(contentMetaComposition);
            service.Save(contentPage);

            // Assert
            Assert.That(bodyTextAdded, Is.True);
            Assert.That(authorAdded, Is.True);
            Assert.That(compositionAdded, Is.True);

            Assert.DoesNotThrow(() => service.GetContentType("contentPage"));
            Assert.DoesNotThrow(() => service.GetContentType("advancedPage"));

            var contentType = service.GetContentType("contentPage");
            var propertyGroup = contentType.PropertyGroups["Content"];
            Assert.That(propertyGroup.ParentId.HasValue, Is.False);

            var numberOfContentTabs = contentType.CompositionPropertyGroups.Count(x => x.Name.Equals("Content"));
            Assert.That(numberOfContentTabs, Is.EqualTo(3));

            //Ensure that adding a new PropertyType to the "Content"-tab also adds it to the right group

            var descriptionPropertyType = new PropertyType(Constants.PropertyEditors.TextboxAlias, DataTypeDatabaseType.Ntext)
            {
                Alias = "description", Name = "Description", Description = "",  Mandatory = false, SortOrder = 1,DataTypeDefinitionId = -88
            };
            var descriptionAdded = contentType.AddPropertyType(descriptionPropertyType, "Content");
            service.Save(contentType);
            Assert.That(descriptionAdded, Is.True);

            var contentPageReloaded = service.GetContentType("contentPage");
            var propertyGroupReloaded = contentPageReloaded.PropertyGroups["Content"];
            var hasDescriptionPropertyType = propertyGroupReloaded.PropertyTypes.Contains("description");
            Assert.That(hasDescriptionPropertyType, Is.True);
            Assert.That(propertyGroupReloaded.ParentId.HasValue, Is.False);

            var descriptionPropertyTypeReloaded = propertyGroupReloaded.PropertyTypes["description"];
            Assert.That(descriptionPropertyTypeReloaded.PropertyGroupId.IsValueCreated, Is.False);
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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "componentGroup") { Name = "Component Group", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
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

            var propertyType = new PropertyType("test", DataTypeDatabaseType.Ntext, "bannerName")
            {
                Name = "Banner Name",
                Description = "",
                Mandatory = false,
                SortOrder = 2,
                DataTypeDefinitionId = -88
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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "hostname") { Name = "Hostname", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
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

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "title") { Name = "Title", Description = "",  Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "bodyText") { Name = "Body Text", Description = "",  Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType("test", DataTypeDatabaseType.Ntext, "author") { Name = "Author", Description = "Name of the author",  Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

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