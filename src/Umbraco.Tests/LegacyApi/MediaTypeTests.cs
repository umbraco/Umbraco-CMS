using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;

namespace Umbraco.Tests.LegacyApi
{
    [TestFixture, Ignore]
    public class MediaTypeTests : BaseDatabaseFactoryTest
    {
        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            CreateTestData();
        }

        [Test]
        public void Can_Verify_AllowedChildContentTypes_On_MediaType()
        {
            // Arrange
            var folder = MediaType.GetByAlias("Folder");
            var folderStructure = folder.AllowedChildContentTypeIDs.ToList();
            folderStructure.Add(1045);

            // Act
            folder.AllowedChildContentTypeIDs = folderStructure.ToArray();
            folder.Save();

            // Assert
            var updated = MediaType.GetByAlias("Folder");

            Assert.That(updated.AllowedChildContentTypeIDs.Any(), Is.True);
            Assert.That(updated.AllowedChildContentTypeIDs.Any(x => x == 1045), Is.True);
        }

        [Test]
        public void Can_Set_Tab_On_PropertyType()
        {
            var UPLOAD_DATATYPE_ID = -90;
            var CROP_DATATYPE_ID = 1043;
            var LABEL_DATATYPE_ID = -92;
            var mediaTypeName = "ImageWide";

            MediaType mediaType = MediaType.MakeNew(new User(0), mediaTypeName);

            int imageTab = mediaType.AddVirtualTab("Image");
            int cropTab = mediaType.AddVirtualTab("Crop");

            mediaType.AddPropertyType(new DataTypeDefinition(UPLOAD_DATATYPE_ID), "umbracoFile", "Upload image");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), "umbracoWidth", "Width");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), "umbracoHeight", "Height");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), "umbracoBytes", "Size");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), "umbracoExtension", "Type");
            mediaType.AddPropertyType(new DataTypeDefinition(CROP_DATATYPE_ID), "wideImage", "Wide image");

            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("umbracoFile"), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("umbracoWidth"), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("umbracoHeight"), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("umbracoBytes"), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("umbracoExtension"), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType("wideImage"), cropTab);

            mediaType.Text = mediaTypeName;
            mediaType.IconUrl = "mediaPhoto.gif";
            mediaType.Save();

            Assert.That(mediaType.getVirtualTabs.Count(), Is.EqualTo(2));
            Assert.That(mediaType.getVirtualTabs.Any(x => x.Caption.Equals("Image")), Is.True);
            Assert.That(mediaType.getVirtualTabs.Any(x => x.Caption.Equals("Crop")), Is.True);

            var updated = new MediaType(mediaType.Id);
            Assert.That(updated.getVirtualTabs.Count(), Is.EqualTo(2));
            Assert.That(updated.getVirtualTabs.Any(x => x.Caption.Equals("Image")), Is.True);
            Assert.That(updated.getVirtualTabs.Any(x => x.Caption.Equals("Crop")), Is.True);
            Assert.That(updated.ContentTypeItem.PropertyGroups.Count(), Is.EqualTo(2));
            Assert.That(updated.ContentTypeItem.PropertyTypes.Count(), Is.EqualTo(6));
        }

        public void CreateTestData()
        {
            //Create and Save ContentType "video" -> 1045
            var videoMediaType = MockedContentTypes.CreateVideoMediaType();
            ServiceContext.ContentTypeService.Save(videoMediaType);
        }


        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }
    }
}