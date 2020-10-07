using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Integration.Testing;
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
    public class EntityServiceTests : UmbracoIntegrationTest
    {
        private Language _langFr;
        private Language _langEs;

        private ILocalizationService LocalizationService => GetRequiredService<ILocalizationService>();
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        private IContentService ContentService => GetRequiredService<IContentService>();
        private IEntityService EntityService => GetRequiredService<IEntityService>();
        private ISqlContext SqlContext => GetRequiredService<ISqlContext>();
        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
        private IMediaService MediaService => GetRequiredService<IMediaService>();
        private IFileService FileService => GetRequiredService<IFileService>();

        [SetUp]
        public void SetupTestData()
        {
            if (_langFr == null && _langEs == null)
            {
                var globalSettings = new GlobalSettings();
                _langFr = new Language(globalSettings, "fr-FR");
                _langEs = new Language(globalSettings, "es-ES");
                LocalizationService.Save(_langFr);
                LocalizationService.Save(_langEs);
            }

            CreateTestData();
        }

        [Test]
        public void EntityService_Can_Get_Paged_Descendants_Ordering_Path()
        {

            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var rootId = root.Id;
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                ids.Add(c1.Id);
                root = c1; // make a hierarchy
            }

            long total;

            var entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            //Test ordering direction

            entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 0, 6, out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1], entities[0].Id);

            entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 1, 6, out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Children()
        {

            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                ids.Add(c1.Id);
            }

            long total;

            var entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            //Test ordering direction

            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1], entities[0].Id);

            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants()
        {
            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    var c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                    count++;
                }
            }

            long total;
            var entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_Including_Recycled()
        {
            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ContentService.MoveToRecycleBin(content);
            }

            long total;
            //search at root to see if it returns recycled
            var entities = EntityService.GetPagedDescendants(-1, UmbracoObjectTypes.Document, 0, 1000, out total)
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
            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                ContentService.MoveToRecycleBin(content);
            }

            long total;
            //search at root to see if it returns recycled
            var entities = EntityService.GetPagedDescendants(UmbracoObjectTypes.Document, 0, 1000, out total, includeTrashed: false)
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
            var contentType = ContentTypeService.Get("umbTextpage");

            var root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, "ssss" + Guid.NewGuid(), root);
                ContentService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    var c2 = ContentBuilder.CreateSimpleContent(contentType, "tttt" + Guid.NewGuid(), c1);
                    ContentService.Save(c2);
                }
            }

            long total;
            var entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 10, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 50, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Children()
        {
            var folderType = MediaTypeService.Get(1031);
            var imageMediaType = MediaTypeService.Get(1032);

            var root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            for (int i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);
            }

            long total;
            var entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 0, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants()
        {
            var folderType = MediaTypeService.Get(1031);
            var imageMediaType = MediaTypeService.Get(1032);

            var root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            var count = 0;
            for (int i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                    count++;
                }
            }

            long total;
            var entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_Including_Recycled()
        {
            var folderType = MediaTypeService.Get(1031);
            var imageMediaType = MediaTypeService.Get(1032);

            var root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                MediaService.MoveToRecycleBin(content);
            }

            long total;
            //search at root to see if it returns recycled
            var entities = EntityService.GetPagedDescendants(-1, UmbracoObjectTypes.Media, 0, 1000, out total)
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
            var folderType = MediaTypeService.Get(1031);
            var imageMediaType = MediaTypeService.Get(1032);

            var root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                }
            }

            foreach (var content in toDelete)
            {
                MediaService.MoveToRecycleBin(content);
            }

            long total;
            //search at root to see if it returns recycled
            var entities = EntityService.GetPagedDescendants(UmbracoObjectTypes.Media, 0, 1000, out total, includeTrashed: false)
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
            var folderType = MediaTypeService.Get(1031);
            var imageMediaType = MediaTypeService.Get(1032);

            var root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                c1.Name = "ssss" + Guid.NewGuid();
                MediaService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    var c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    c2.Name = "tttt" + Guid.NewGuid();
                    MediaService.Save(c2);
                }
            }

            long total;
            var entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 10, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 50, out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectTypes()
        {
            var entities = EntityService.GetAll(UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectType_Id()
        {
            var objectTypeId = Constants.ObjectTypes.Document;
            var entities = EntityService.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_Type()
        {
            var entities = EntityService.GetAll<IContent>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType()
        {
            var entities = EntityService.GetChildren(-1, UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(1));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Content_By_UmbracoObjectType_With_Variant_Names()
        {
            var alias = "test" + Guid.NewGuid();
            var contentType = ContentTypeBuilder.CreateSimpleContentType(alias, alias);
            contentType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            var c1 = ContentBuilder.CreateSimpleContent(contentType, "Test", -1);
            c1.SetCultureName("Test - FR", _langFr.IsoCode);
            c1.SetCultureName("Test - ES", _langEs.IsoCode);
            ContentService.Save(c1);

            var result = EntityService.Get(c1.Id, UmbracoObjectTypes.Document);
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
            var contentType = ContentTypeBuilder.CreateSimpleContentType("test1", "Test1");
            contentType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            var root = ContentBuilder.CreateSimpleContent(contentType);
            root.SetCultureName("Root", _langFr.IsoCode); // else cannot save
            ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                if (i % 2 == 0)
                {
                    c1.SetCultureName("Test " + i + " - FR", _langFr.IsoCode);
                    c1.SetCultureName("Test " + i + " - ES", _langEs.IsoCode);
                }
                else
                {
                    c1.SetCultureName("Test", _langFr.IsoCode); // else cannot save
                }
                ContentService.Save(c1);
            }

            var entities = EntityService.GetChildren(root.Id, UmbracoObjectTypes.Document).ToArray();

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
            var entities = EntityService.GetChildren(folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(3));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Descendants_By_ParentId()
        {
            var entities = EntityService.GetDescendants(folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Throws_When_Getting_All_With_Invalid_Type()
        {
            var objectTypeId = Constants.ObjectTypes.ContentItem;

            Assert.Throws<NotSupportedException>(() => EntityService.GetAll<IContentBase>());
            Assert.Throws<NotSupportedException>(() => EntityService.GetAll(objectTypeId));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectTypes()
        {
            var entities = EntityService.GetAll(UmbracoObjectTypes.DocumentType).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectType_Id()
        {
            var objectTypeId = Constants.ObjectTypes.DocumentType;
            var entities = EntityService.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_Type()
        {
            var entities = EntityService.GetAll<IContentType>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_Media_By_UmbracoObjectTypes()
        {
            var entities = EntityService.GetAll(UmbracoObjectTypes.Media).ToArray();

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
        { ;
            var mediaObjectType = EntityService.GetObjectType(1031);

            Assert.NotNull(mediaObjectType);
            Assert.AreEqual(mediaObjectType, UmbracoObjectTypes.MediaType);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id_With_Unknown_Type()
        {
            var result = EntityService.GetKey(1052, UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id()
        {
            var result = EntityService.GetKey(1052, UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Key_For_Id_With_Incorrect_Object_Type()
        {
            var result1 = EntityService.GetKey(1052, UmbracoObjectTypes.DocumentType);
            var result2 = EntityService.GetKey(1052, UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key_With_Unknown_Type()
        {
            var result = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1052, result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key()
        {
            var result = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(1052, result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Id_For_Key_With_Incorrect_Object_Type()
        {
            var result1 = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);
            var result2 = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void ReserveId()
        {
            var guid = Guid.NewGuid();

            // can reserve
            var reservedId = EntityService.ReserveId(guid);
            Assert.IsTrue(reservedId > 0);

            // can get it back
            var id = EntityService.GetId(guid, UmbracoObjectTypes.DocumentType);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // anything goes
            id = EntityService.GetId(guid, UmbracoObjectTypes.Media);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // a random guid won't work
            Assert.IsFalse(EntityService.GetId(Guid.NewGuid(), UmbracoObjectTypes.DocumentType).Success);
        }

        private static bool _isSetup = false;

        private int folderId;

        public void CreateTestData()
        {
            if (_isSetup == false)
            {
                _isSetup = true;

                //Create and Save ContentType "umbTextpage" -> 1052
                ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage");
                contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
                FileService.SaveTemplate(contentType.DefaultTemplate); // else, FK violation on contentType!
                ContentTypeService.Save(contentType);

                //Create and Save Content "Homepage" based on "umbTextpage" -> 1053
                Content textpage = ContentBuilder.CreateSimpleContent(contentType);
                textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
                ContentService.Save(textpage, 0);

                //Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
                Content subpage = ContentBuilder.CreateSimpleContent(contentType, "Text Page 1", textpage.Id);
                subpage.ContentSchedule.Add(DateTime.Now.AddMinutes(-5), null);
                ContentService.Save(subpage, 0);

                //Create and Save Content "Text Page 2" based on "umbTextpage" -> 1055
                Content subpage2 = ContentBuilder.CreateSimpleContent(contentType, "Text Page 2", textpage.Id);
                ContentService.Save(subpage2, 0);

                //Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1056
                Content trashed = ContentBuilder.CreateSimpleContent(contentType, "Text Page Deleted", -20);
                trashed.Trashed = true;
                ContentService.Save(trashed, 0);

                //Create and Save folder-Media -> 1057
                var folderMediaType = MediaTypeService.Get(1031);
                var folder = MediaBuilder.CreateMediaFolder(folderMediaType, -1);
                MediaService.Save(folder, 0);
                folderId = folder.Id;

                //Create and Save image-Media -> 1058
                var imageMediaType = MediaTypeService.Get(1032);
                var image = MediaBuilder.CreateMediaImage(imageMediaType, folder.Id);
                MediaService.Save(image, 0);

                //Create and Save file-Media -> 1059
                var fileMediaType = MediaTypeService.Get(1033);
                var file = MediaBuilder.CreateMediaFile(fileMediaType, folder.Id);
                MediaService.Save(file, 0);

                // Create and save sub folder -> 1060
                var subfolder = MediaBuilder.CreateMediaFolder(folderMediaType, folder.Id);
                MediaService.Save(subfolder, 0);
                // Create and save sub folder -> 1061
                var subfolder2 = MediaBuilder.CreateMediaFolder(folderMediaType, subfolder.Id);
                MediaService.Save(subfolder2, 0);
            }
        }
    }
}
