using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Rdbms;
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
	    public void Deleting_PropertyType_Removes_The_Property_From_Content()
	    {
            IContentType contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "Test1");
            ServiceContext.ContentTypeService.Save(contentType1);
	        IContent contentItem = MockedContent.CreateTextpageContent(contentType1, "Testing", -1);
	        ServiceContext.ContentService.SaveAndPublish(contentItem);
	        var initProps = contentItem.Properties.Count;
	        var initPropTypes = contentItem.PropertyTypes.Count();

            //remove a property
            contentType1.RemovePropertyType(contentType1.PropertyTypes.First().Alias);
            ServiceContext.ContentTypeService.Save(contentType1);

            //re-load it from the db
            contentItem = ServiceContext.ContentService.GetById(contentItem.Id);

            Assert.AreEqual(initPropTypes - 1, contentItem.PropertyTypes.Count());
            Assert.AreEqual(initProps -1, contentItem.Properties.Count);
	    }

	    [Test]
	    public void Rebuild_Content_Xml_On_Alias_Change()
        {
            var contentType1 = MockedContentTypes.CreateTextpageContentType("test1", "Test1");
            var contentType2 = MockedContentTypes.CreateTextpageContentType("test2", "Test2");
            ServiceContext.ContentTypeService.Save(contentType1);
            ServiceContext.ContentTypeService.Save(contentType2);
	        var contentItems1 = MockedContent.CreateTextpageContent(contentType1, -1, 10).ToArray();
            contentItems1.ForEach(x => ServiceContext.ContentService.SaveAndPublish(x));
            var contentItems2 = MockedContent.CreateTextpageContent(contentType2, -1, 5).ToArray();
            contentItems2.ForEach(x => ServiceContext.ContentService.SaveAndPublish(x));
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
            contentItems1.ForEach(x => ServiceContext.ContentService.SaveAndPublish(x));
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
            ctBase.AddPropertyType(new PropertyType(dtdYesNo) { Name = "Hide From Navigation", Alias = Constants.Conventions.Content.NaviHide } /*,"Navigation"*/ );
            cts.Save(ctBase);

            var ctHomePage = new ContentType(ctBase) { Name = "Home Page", Alias = "HomePage", Icon = "settingDomain.gif", Thumbnail = "folder.png", AllowedAsRoot = true };
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