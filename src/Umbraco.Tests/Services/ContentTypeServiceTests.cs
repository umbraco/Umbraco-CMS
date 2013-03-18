using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
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
            ctBase.AddPropertyType(new PropertyType(dtdYesNo) { Name = "Hide From Navigation", Alias = Constants.Conventions.Content.NaviHide } /*,"Navigation"*/ );
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