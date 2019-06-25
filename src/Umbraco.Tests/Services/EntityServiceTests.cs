using System;
using System.Collections.Generic;
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
        public void EntityService_Can_Get_Paged_Content_Children()
        {

            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ServiceContext.ContentService.Save(c2);
                    count++;
                }
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_Including_Recycled()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ServiceContext.ContentService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ServiceContext.ContentService.MoveToRecycleBin(content);
            }

            var service = ServiceContext.EntityService;

            long total;
            //search at root to see if it returns recycled
            var entities = service.GetPagedDescendants(-1, UmbracoObjectTypes.Document, 0, 1000, out total)
                .Select(x => x.Id)
                .ToArray();

            foreach (var c in toDelete)
            {
                Assert.True(entities.Contains(c.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_Without_Recycled()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ServiceContext.ContentService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ServiceContext.ContentService.MoveToRecycleBin(content);
            }

            var service = ServiceContext.EntityService;

            long total;
            //search at root to see if it returns recycled
            var entities = service.GetPagedDescendantsFromRoot(UmbracoObjectTypes.Document, 0, 1000, out total, includeTrashed:false)
                .Select(x => x.Id)
                .ToArray();

            foreach (var c in toDelete)
            {
                Assert.IsFalse(entities.Contains(c.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_With_Search()
        {
            var contentType = ServiceContext.ContentTypeService.GetContentType("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, "ssss" + Guid.NewGuid(), root);
                ServiceContext.ContentService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedContent.CreateSimpleContent(contentType, "tttt" + Guid.NewGuid(), c1);
                    ServiceContext.ContentService.Save(c2);
                }
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 10, out total, filter: "ssss").ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 50, out total, filter: "tttt").ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Children()
        {
            var folderType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);

            var root = MockedMedia.CreateMediaFolder(folderType, -1);
            ServiceContext.MediaService.Save(root);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageMediaType, root.Id);
                ServiceContext.MediaService.Save(c1);
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants()
        {
            var folderType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);

            var root = MockedMedia.CreateMediaFolder(folderType, -1);
            ServiceContext.MediaService.Save(root);
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageMediaType, root.Id);
                ServiceContext.MediaService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedMedia.CreateMediaImage(imageMediaType, c1.Id);
                    ServiceContext.MediaService.Save(c2);
                    count++;
                }
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_Including_Recycled()
        {
            var folderType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);

            var root = MockedMedia.CreateMediaFolder(folderType, -1);
            ServiceContext.MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageMediaType, root.Id);
                ServiceContext.MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedMedia.CreateMediaImage(imageMediaType, c1.Id);
                    ServiceContext.MediaService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ServiceContext.MediaService.MoveToRecycleBin(content);
            }

            var service = ServiceContext.EntityService;

            long total;
            //search at root to see if it returns recycled
            var entities = service.GetPagedDescendants(-1, UmbracoObjectTypes.Media, 0, 1000, out total)
                .Select(x => x.Id)
                .ToArray();

            foreach (var media in toDelete)
            {
                Assert.IsTrue(entities.Contains(media.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_Without_Recycled()
        {
            var folderType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);

            var root = MockedMedia.CreateMediaFolder(folderType, -1);
            ServiceContext.MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageMediaType, root.Id);
                ServiceContext.MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedMedia.CreateMediaImage(imageMediaType, c1.Id);
                    ServiceContext.MediaService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ServiceContext.MediaService.MoveToRecycleBin(content);
            }

            var service = ServiceContext.EntityService;

            long total;
            //search at root to see if it returns recycled
            var entities = service.GetPagedDescendantsFromRoot(UmbracoObjectTypes.Media, 0, 1000, out total, includeTrashed:false)
                .Select(x => x.Id)
                .ToArray();

            foreach (var media in toDelete)
            {
                Assert.IsFalse(entities.Contains(media.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_With_Search()
        {
            var folderType = ServiceContext.ContentTypeService.GetMediaType(1031);
            var imageMediaType = ServiceContext.ContentTypeService.GetMediaType(1032);

            var root = MockedMedia.CreateMediaFolder(folderType, -1);
            ServiceContext.MediaService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedMedia.CreateMediaImage(imageMediaType, root.Id);
                c1.Name = "ssss" + Guid.NewGuid();
                ServiceContext.MediaService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MockedMedia.CreateMediaImage(imageMediaType, c1.Id);
                    c2.Name = "tttt" + Guid.NewGuid();
                    ServiceContext.MediaService.Save(c2);
                }
            }

            var service = ServiceContext.EntityService;

            long total;
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 10, out total, filter: "ssss").ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 50, out total, filter: "tttt").ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
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
        }

        [Test]
        public void EntityService_Can_Get_ObjectType()
        {
            var service = ServiceContext.EntityService;
            var mediaObjectType = service.GetObjectType(1031);

            Assert.NotNull(mediaObjectType);
            Assert.AreEqual(mediaObjectType, UmbracoObjectTypes.MediaType);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id_With_Unknown_Type()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetKeyForId(1060, UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetKeyForId(1060, UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Key_For_Id_With_Incorrect_Object_Type()
        {
            var service = ServiceContext.EntityService;
            var result1 = service.GetKeyForId(1060, UmbracoObjectTypes.DocumentType);
            var result2 = service.GetKeyForId(1060, UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key_With_Unknown_Type()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetIdForKey(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1060, result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetIdForKey(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1060, result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Id_For_Key_With_Incorrect_Object_Type()
        {
            var service = ServiceContext.EntityService;
            var result1 = service.GetIdForKey(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);
            var result2 = service.GetIdForKey(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void ReserveId()
        {
            var service = ServiceContext.EntityService;
            var guid = Guid.NewGuid();

            // can reserve
            var reservedId = service.ReserveId(guid);
            Assert.IsTrue(reservedId > 0);

            // can get it back
            var id = service.GetIdForKey(guid, UmbracoObjectTypes.DocumentType);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // anything goes
            id = service.GetIdForKey(guid, UmbracoObjectTypes.Media);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // a random guid won't work
            Assert.IsFalse(service.GetIdForKey(Guid.NewGuid(), UmbracoObjectTypes.DocumentType).Success);
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
