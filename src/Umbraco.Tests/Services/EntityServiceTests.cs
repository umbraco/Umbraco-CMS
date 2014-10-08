using System;
using System.Linq;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;

namespace Umbraco.Tests.Services
{
    /// <summary>
    /// Tests covering the EntityService
    /// </summary>
    [DatabaseTestBehavior(DatabaseBehavior.NewDbFileAndSchemaPerFixture)]
    [TestFixture, RequiresSTA]
    public class EntityServiceTests : BaseServiceTest
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
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectType_Id()
        {
            var service = ServiceContext.EntityService;

            var objectTypeId = new Guid(Constants.ObjectTypes.Document);
            var entities = service.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_Type()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll<IContent>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetChildren(-1, UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Children_By_ParentId()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetChildren(folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(3));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Descendants_By_ParentId()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetDescendents(folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Throws_When_Getting_All_With_Invalid_Type()
        {
            var service = ServiceContext.EntityService;
            var objectTypeId = new Guid(Constants.ObjectTypes.ContentItem);

            Assert.Throws<NotSupportedException>(() => service.GetAll<IContentBase>());
            Assert.Throws<NullReferenceException>(() => service.GetAll(UmbracoObjectTypes.ContentItem));
            Assert.Throws<NullReferenceException>(() => service.GetAll(objectTypeId));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.DocumentType).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectType_Id()
        {
            var service = ServiceContext.EntityService;

            var objectTypeId = new Guid(Constants.ObjectTypes.DocumentType);
            var entities = service.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_Type()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll<IContentType>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_Media_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.Media).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(5));

            Assert.That(
                entities.Any(
                    x =>
                    x.AdditionalData.Any(y => y.Value is UmbracoEntity.EntityProperty 
                        && ((UmbracoEntity.EntityProperty)y.Value).PropertyEditorAlias == Constants.PropertyEditors.UploadFieldAlias)), Is.True);
        }

        [Test]
        public void EntityService_Can_Get_ObjectType()
        {
            var service = ServiceContext.EntityService;
            var mediaObjectType = service.GetObjectType(1031);

            Assert.NotNull(mediaObjectType);
            Assert.AreEqual(mediaObjectType, UmbracoObjectTypes.MediaType);
        }

        private static bool _isSetup = false;

        private int folderId;

        public override void CreateTestData()
        {
            if (_isSetup == false)
            {
                _isSetup = true;

                base.CreateTestData();

                //Create and Save folder-Media -> 1050
                var folderMediaType = ServiceContext.ContentTypeService.GetMediaType(1031);
                var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);                
                ServiceContext.MediaService.Save(folder, 0);
                folderId = folder.Id;

                //Create and Save image-Media -> 1051
                var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);
                var image = MockedMedia.CreateMediaImage(imageMediaType, folder.Id);
                ServiceContext.MediaService.Save(image, 0);

                //Create and Save file-Media -> 1052
                var fileMediaType = ServiceContext.ContentTypeService.GetMediaType(1033);
                var file = MockedMedia.CreateMediaFile(fileMediaType, folder.Id);
                ServiceContext.MediaService.Save(file, 0);

                var subfolder = MockedMedia.CreateMediaFolder(folderMediaType, folder.Id);
                ServiceContext.MediaService.Save(subfolder, 0);
                var subfolder2 = MockedMedia.CreateMediaFolder(folderMediaType, subfolder.Id);
                ServiceContext.MediaService.Save(subfolder2, 0);
                
            }
            
        }
    }
}