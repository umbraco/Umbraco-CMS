using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
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