using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
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
            ctBase.AddPropertyType(new PropertyType(dtdYesNo) { Name = "Hide From Navigation", Alias = "umbracoNaviHide" } /*,"Navigation"*/ );
            cts.Save(ctBase);

            var ctHomePage = new ContentType(ctBase) { Name = "Home Page", Alias = "HomePage", Icon = "settingDomain.gif", Thumbnail = "folder.png", AllowedAsRoot = true };
            bool addedContentType = ctHomePage.AddContentType(ctBase);
            ctHomePage.AddPropertyType(new PropertyType(dtdYesNo) { Name = "Some property", Alias = "someProperty" }  /*,"Navigation"*/ );
            cts.Save(ctHomePage);

            // Act
            var homeDoc = cs.CreateContent("Home Page", -1, "HomePage");
            cs.SaveAndPublish(homeDoc);

            // Assert
            Assert.That(ctBase.HasIdentity, Is.True);
            Assert.That(ctHomePage.HasIdentity, Is.True);
            Assert.That(homeDoc.HasIdentity, Is.True);
            Assert.That(homeDoc.ContentTypeId, Is.EqualTo(ctHomePage.Id));
            Assert.That(addedContentType, Is.True);
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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "componentGroup", Name = "Component Group", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            component.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Component", SortOrder = 1 });

            return component;
        }

        private ContentType CreateBannerComponent(ContentType parent)
        {
            var banner = new ContentType(parent)
                                {
                                    Alias = "banner",
                                    Name = "Banner Component",
                                    Description = "ContentType used for Banner Component",
                                    Icon = ".sprTreeDoc3",
                                    Thumbnail = "doc.png",
                                    SortOrder = 1,
                                    CreatorId = 0,
                                    Trashed = false
                                };

            var propertyType = new PropertyType(new Guid(), DataTypeDatabaseType.Ntext)
                {
                    Alias = "bannerName",
                    Name = "Banner Name",
                    Description = "",
                    HelpText = "",
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
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "hostname", Name = "Hostname", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            site.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Site Settings", SortOrder = 1 });

            return site;
        }

        private ContentType CreateHomepage(ContentType parent)
        {
            var contentType = new ContentType(parent)
                             {
                                 Alias = "homepage",
                                 Name = "Homepage",
                                 Description = "ContentType used for the Homepage",
                                 Icon = ".sprTreeDoc3",
                                 Thumbnail = "doc.png",
                                 SortOrder = 1,
                                 CreatorId = 0,
                                 Trashed = false
                             };

            var contentCollection = new PropertyTypeCollection();
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "title", Name = "Title", Description = "", HelpText = "", Mandatory = false, SortOrder = 1, DataTypeDefinitionId = -88 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "bodyText", Name = "Body Text", Description = "", HelpText = "", Mandatory = false, SortOrder = 2, DataTypeDefinitionId = -87 });
            contentCollection.Add(new PropertyType(new Guid(), DataTypeDatabaseType.Ntext) { Alias = "author", Name = "Author", Description = "Name of the author", HelpText = "", Mandatory = false, SortOrder = 3, DataTypeDefinitionId = -88 });

            contentType.PropertyGroups.Add(new PropertyGroup(contentCollection) { Name = "Content", SortOrder = 1 });

            return contentType;
        }

		private IEnumerable<IContentType> CreateContentTypeHierarchy()
		{
			//create the master type
			var masterContentType = MockedContentTypes.CreateSimpleContentType("masterContentType", "MasterContentType");
			masterContentType.Key = new Guid("C00CA18E-5A9D-483B-A371-EECE0D89B4AE");
			ServiceContext.ContentTypeService.Save(masterContentType);

			//add the one we just created
			var list = new List<IContentType> {masterContentType};
			
			for (var i = 0; i < 10; i++)
			{
				var contentType = MockedContentTypes.CreateSimpleContentType("childType" + i, "ChildType" + i, 
					//make the last entry in the list, this one's parent
					list.Last());

				list.Add(contentType);				
			}

			return list;
		}
	}
}