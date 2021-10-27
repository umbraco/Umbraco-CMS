using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{

    /// <summary>
    /// Tests covering the EntityService
    /// </summary>
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerFixture)]
    public class EntityServiceTests : TestWithSomeContentBase
    {
        private Language _langFr;
        private Language _langEs;

        public override void SetUp()
        {
            base.SetUp();

            if (_langFr == null && _langEs == null)
            {
                _langFr = new Language("fr-FR");
                _langEs = new Language("es-ES");
                ServiceContext.LocalizationService.Save(_langFr);
                ServiceContext.LocalizationService.Save(_langEs);
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Descendants_Ordering_Path()
        {

            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            var rootId = root.Id;
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);
                ids.Add(c1.Id);
                root = c1; // make a hierarchy
            }

            var service = ServiceContext.EntityService;

            long total;

            var entities = service.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = service.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            //Test ordering direction

            entities = service.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 0, 6, out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1], entities[0].Id);

            entities = service.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 1, 6, out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Children()
        {

            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

            var root = MockedContent.CreateSimpleContent(contentType);
            ServiceContext.ContentService.Save(root);
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ServiceContext.ContentService.Save(c1);
                ids.Add(c1.Id);
            }

            var service = ServiceContext.EntityService;

            long total;

            var entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            //Test ordering direction

            entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1], entities[0].Id);

            entities = service.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants()
        {
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

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
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

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
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

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
            var entities = service.GetPagedDescendants(UmbracoObjectTypes.Document, 0, 1000, out total, includeTrashed: false)
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
            var contentType = ServiceContext.ContentTypeService.Get("umbTextpage");

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
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 10, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 50, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Children()
        {
            var folderType = ServiceContext.MediaTypeService.Get(1031);
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);

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
            var folderType = ServiceContext.MediaTypeService.Get(1031);
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);

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
            var folderType = ServiceContext.MediaTypeService.Get(1031);
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);

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
            var folderType = ServiceContext.MediaTypeService.Get(1031);
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);

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
            var entities = service.GetPagedDescendants(UmbracoObjectTypes.Media, 0, 1000, out total, includeTrashed: false)
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
            var folderType = ServiceContext.MediaTypeService.Get(1031);
            var imageMediaType = ServiceContext.MediaTypeService.Get(1032);

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
            var entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 10, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = service.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 50, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectTypes()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll(UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(5));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectType_Id()
        {
            var service = ServiceContext.EntityService;

            var objectTypeId = Constants.ObjectTypes.Document;
            var entities = service.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(5));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_Type()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetAll<IContent>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(5));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType()
        {
            var service = ServiceContext.EntityService;

            var entities = service.GetChildren(-1, UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(1));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Content_By_UmbracoObjectType_With_Variant_Names()
        {
            var service = ServiceContext.EntityService;

            var alias = "test" + Guid.NewGuid();
            var contentType = MockedContentTypes.CreateSimpleContentType(alias, alias, false);
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            var c1 = MockedContent.CreateSimpleContent(contentType, "Test", -1);
            c1.SetCultureName("Test - FR", _langFr.IsoCode);
            c1.SetCultureName("Test - ES", _langEs.IsoCode);
            ServiceContext.ContentService.Save(c1);

            var result = service.Get(c1.Id, UmbracoObjectTypes.Document);
            Assert.AreEqual("Test - FR", result.Name); // got name from default culture
            Assert.IsNotNull(result as IDocumentEntitySlim);
            var doc = (IDocumentEntitySlim)result;
            var cultureNames = doc.CultureNames;
            Assert.AreEqual("Test - FR", cultureNames[_langFr.IsoCode]);
            Assert.AreEqual("Test - ES", cultureNames[_langEs.IsoCode]);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType_With_Variant_Names()
        {
            var service = ServiceContext.EntityService;

            var contentType = MockedContentTypes.CreateSimpleContentType("test1", "Test1", false);
            contentType.Variations = ContentVariation.Culture;
            ServiceContext.ContentTypeService.Save(contentType);

            var root = MockedContent.CreateSimpleContent(contentType);
            root.SetCultureName("Root", _langFr.IsoCode); // else cannot save
            ServiceContext.ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = MockedContent.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                if (i % 2 == 0)
                {
                    c1.SetCultureName("Test " + i + " - FR", _langFr.IsoCode);
                    c1.SetCultureName("Test " + i + " - ES", _langEs.IsoCode);
                }
                else
                {
                    c1.SetCultureName("Test", _langFr.IsoCode); // else cannot save
                }
                ServiceContext.ContentService.Save(c1);
            }

            var entities = service.GetChildren(root.Id, UmbracoObjectTypes.Document).ToArray();

            Assert.AreEqual(10, entities.Length);

            for (int i = 0; i < entities.Length; i++)
            {
                Assert.AreEqual(0, entities[i].AdditionalData.Count);

                if (i % 2 == 0)
                {
                    var doc = (IDocumentEntitySlim)entities[i];
                    var keys = doc.CultureNames.Keys.ToList();
                    var vals = doc.CultureNames.Values.ToList();
                    Assert.AreEqual(_langFr.IsoCode.ToLowerInvariant(), keys[0].ToLowerInvariant());
                    Assert.AreEqual("Test " + i + " - FR", vals[0]);
                    Assert.AreEqual(_langEs.IsoCode.ToLowerInvariant(), keys[1].ToLowerInvariant());
                    Assert.AreEqual("Test " + i + " - ES", vals[1]);
                }
                else
                {
                    Assert.AreEqual(0, entities[i].AdditionalData.Count);
                }
            }
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

            var entities = service.GetDescendants(folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Throws_When_Getting_All_With_Invalid_Type()
        {
            var service = ServiceContext.EntityService;
            var objectTypeId = Constants.ObjectTypes.ContentItem;

            Assert.Throws<NotSupportedException>(() => service.GetAll<IContentBase>());
            Assert.Throws<NotSupportedException>(() => service.GetAll(objectTypeId));
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

            var objectTypeId = Constants.ObjectTypes.DocumentType;
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
            Assert.That(entities.Length, Is.EqualTo(5));

            foreach (var entity in entities)
            {
                Assert.IsTrue(entity.GetType().Implements<IMediaEntitySlim>());
                Console.WriteLine(((IMediaEntitySlim)entity).MediaPath);
                Assert.IsNotEmpty(((IMediaEntitySlim)entity).MediaPath);
            }
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
            var result = service.GetKey(1061, UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetKey(1061, UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Key_For_Id_With_Incorrect_Object_Type()
        {
            var service = ServiceContext.EntityService;
            var result1 = service.GetKey(1061, UmbracoObjectTypes.DocumentType);
            var result2 = service.GetKey(1061, UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key_With_Unknown_Type()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1061, result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key()
        {
            var service = ServiceContext.EntityService;
            var result = service.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1061, result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Id_For_Key_With_Incorrect_Object_Type()
        {
            var service = ServiceContext.EntityService;
            var result1 = service.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);
            var result2 = service.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.MediaType);

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
            var id = service.GetId(guid, UmbracoObjectTypes.DocumentType);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // anything goes
            id = service.GetId(guid, UmbracoObjectTypes.Media);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // a random guid won't work
            Assert.IsFalse(service.GetId(Guid.NewGuid(), UmbracoObjectTypes.DocumentType).Success);
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
                var folderMediaType = ServiceContext.MediaTypeService.Get(1031);
                var folder = MockedMedia.CreateMediaFolder(folderMediaType, -1);
                ServiceContext.MediaService.Save(folder, 0);
                folderId = folder.Id;

                //Create and Save image-Media -> 1051
                var imageMediaType = ServiceContext.MediaTypeService.Get(1032);
                var image = MockedMedia.CreateMediaImage(imageMediaType, folder.Id);
                ServiceContext.MediaService.Save(image, 0);

                //Create and Save file-Media -> 1052
                var fileMediaType = ServiceContext.MediaTypeService.Get(1033);
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
