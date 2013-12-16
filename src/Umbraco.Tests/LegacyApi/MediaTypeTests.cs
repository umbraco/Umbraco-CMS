using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.media;
using Umbraco.Core;

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
            var folder = MediaType.GetByAlias(Constants.Conventions.MediaTypes.Folder);
            var folderStructure = folder.AllowedChildContentTypeIDs.ToList();
            folderStructure.Add(NodeDto.NodeIdSeed);

            // Act
            folder.AllowedChildContentTypeIDs = folderStructure.ToArray();
            folder.Save();

            // Assert
            var updated = MediaType.GetByAlias(Constants.Conventions.MediaTypes.Folder);

            Assert.That(updated.AllowedChildContentTypeIDs.Any(), Is.True);
            Assert.That(updated.AllowedChildContentTypeIDs.Any(x => x == NodeDto.NodeIdSeed), Is.True);
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

            mediaType.AddPropertyType(new DataTypeDefinition(UPLOAD_DATATYPE_ID), Constants.Conventions.Media.File, "Upload image");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), Constants.Conventions.Media.Width, "Width");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), Constants.Conventions.Media.Height, "Height");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), Constants.Conventions.Media.Bytes, "Size");
            mediaType.AddPropertyType(new DataTypeDefinition(LABEL_DATATYPE_ID), Constants.Conventions.Media.Extension, "Type");
            mediaType.AddPropertyType(new DataTypeDefinition(CROP_DATATYPE_ID), "wideImage", "Wide image");

            mediaType.SetTabOnPropertyType(mediaType.getPropertyType(Constants.Conventions.Media.File), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType(Constants.Conventions.Media.Width), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType(Constants.Conventions.Media.Height), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType(Constants.Conventions.Media.Bytes), imageTab);
            mediaType.SetTabOnPropertyType(mediaType.getPropertyType(Constants.Conventions.Media.Extension), imageTab);
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
            //Create and Save ContentType "video" -> NodeDto.NodeIdSeed
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