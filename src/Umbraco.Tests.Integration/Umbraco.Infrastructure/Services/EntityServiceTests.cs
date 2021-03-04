﻿// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
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
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            int rootId = root.Id;
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                ids.Add(c1.Id);
                root = c1; // make a hierarchy
            }

            IEntitySlim[] entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 0, 6, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = EntityService.GetPagedDescendants(rootId, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            // Test ordering direction
            entities = EntityService.GetPagedDescendants(
                rootId,
                UmbracoObjectTypes.Document,
                0,
                6,
                out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[^1], entities[0].Id);

            entities = EntityService.GetPagedDescendants(
                rootId,
                UmbracoObjectTypes.Document,
                1,
                6,
                out total,
                ordering: Ordering.By("Path", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Children()
        {
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var ids = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                ids.Add(c1.Id);
            }

            IEntitySlim[] entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 0, 6, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[0], entities[0].Id);

            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Document, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[6], entities[0].Id);

            // Test ordering direction
            entities = EntityService.GetPagedChildren(
                root.Id,
                UmbracoObjectTypes.Document,
                0,
                6,
                out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[^1], entities[0].Id);

            entities = EntityService.GetPagedChildren(
                root.Id,
                UmbracoObjectTypes.Document,
                1,
                6,
                out total,
                ordering: Ordering.By("SortOrder", Direction.Descending)).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
            Assert.AreEqual(ids[ids.Count - 1 - 6], entities[0].Id);
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants()
        {
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    Content c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                    count++;
                }
            }

            IEntitySlim[] entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 0, 31, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Document, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_Including_Recycled()
        {
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    Content c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                }
            }

            foreach (IContent content in toDelete)
            {
                ContentService.MoveToRecycleBin(content);
            }

            // search at root to see if it returns recycled
            int[] entities = EntityService.GetPagedDescendants(-1, UmbracoObjectTypes.Document, 0, 1000, out long total)
                .Select(x => x.Id)
                .ToArray();

            foreach (IContent c in toDelete)
            {
                Assert.True(entities.Contains(c.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_Without_Recycled()
        {
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);
            var toDelete = new List<IContent>();
            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
                ContentService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    Content c2 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), c1);
                    ContentService.Save(c2);
                }
            }

            foreach (IContent content in toDelete)
            {
                ContentService.MoveToRecycleBin(content);
            }

            // search at root to see if it returns recycled
            int[] entities = EntityService.GetPagedDescendants(UmbracoObjectTypes.Document, 0, 1000, out long total, includeTrashed: false)
                .Select(x => x.Id)
                .ToArray();

            foreach (IContent c in toDelete)
            {
                Assert.IsFalse(entities.Contains(c.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Content_Descendants_With_Search()
        {
            IContentType contentType = ContentTypeService.Get("umbTextpage");

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, "ssss" + Guid.NewGuid(), root);
                ContentService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    Content c2 = ContentBuilder.CreateSimpleContent(contentType, "tttt" + Guid.NewGuid(), c1);
                    ContentService.Save(c2);
                }
            }

            IEntitySlim[] entities = EntityService.GetPagedDescendants(
                root.Id,
                UmbracoObjectTypes.Document,
                0,
                10,
                out long total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedDescendants(
                root.Id,
                UmbracoObjectTypes.Document,
                0,
                50,
                out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Children()
        {
            IMediaType folderType = MediaTypeService.Get(1031);
            IMediaType imageMediaType = MediaTypeService.Get(1032);

            Media root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);
            }

            IEntitySlim[] entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 0, 6, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(6));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedChildren(root.Id, UmbracoObjectTypes.Media, 1, 6, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(total, Is.EqualTo(10));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants()
        {
            IMediaType folderType = MediaTypeService.Get(1031);
            IMediaType imageMediaType = MediaTypeService.Get(1032);

            Media root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            int count = 0;
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);
                count++;

                for (int j = 0; j < 5; j++)
                {
                    Media c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                    count++;
                }
            }

            IEntitySlim[] entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 0, 31, out long total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(31));
            Assert.That(total, Is.EqualTo(60));
            entities = EntityService.GetPagedDescendants(root.Id, UmbracoObjectTypes.Media, 1, 31, out total).ToArray();
            Assert.That(entities.Length, Is.EqualTo(29));
            Assert.That(total, Is.EqualTo(60));
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_Including_Recycled()
        {
            IMediaType folderType = MediaTypeService.Get(1031);
            IMediaType imageMediaType = MediaTypeService.Get(1032);

            Media root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    Media c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                }
            }

            foreach (IMedia content in toDelete)
            {
                MediaService.MoveToRecycleBin(content);
            }

            // search at root to see if it returns recycled
            int[] entities = EntityService.GetPagedDescendants(-1, UmbracoObjectTypes.Media, 0, 1000, out long total)
                .Select(x => x.Id)
                .ToArray();

            foreach (IMedia media in toDelete)
            {
                Assert.IsTrue(entities.Contains(media.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_Without_Recycled()
        {
            IMediaType folderType = MediaTypeService.Get(1031);
            IMediaType imageMediaType = MediaTypeService.Get(1032);

            Media root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);
            var toDelete = new List<IMedia>();
            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                MediaService.Save(c1);

                if (i % 2 == 0)
                {
                    toDelete.Add(c1);
                }

                for (int j = 0; j < 5; j++)
                {
                    Media c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    MediaService.Save(c2);
                }
            }

            foreach (IMedia content in toDelete)
            {
                MediaService.MoveToRecycleBin(content);
            }

            // search at root to see if it returns recycled
            int[] entities = EntityService.GetPagedDescendants(UmbracoObjectTypes.Media, 0, 1000, out long total, includeTrashed: false)
                .Select(x => x.Id)
                .ToArray();

            foreach (IMedia media in toDelete)
            {
                Assert.IsFalse(entities.Contains(media.Id));
            }
        }

        [Test]
        public void EntityService_Can_Get_Paged_Media_Descendants_With_Search()
        {
            IMediaType folderType = MediaTypeService.Get(1031);
            IMediaType imageMediaType = MediaTypeService.Get(1032);

            Media root = MediaBuilder.CreateMediaFolder(folderType, -1);
            MediaService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageMediaType, root.Id);
                c1.Name = "ssss" + Guid.NewGuid();
                MediaService.Save(c1);

                for (int j = 0; j < 5; j++)
                {
                    Media c2 = MediaBuilder.CreateMediaImage(imageMediaType, c1.Id);
                    c2.Name = "tttt" + Guid.NewGuid();
                    MediaService.Save(c2);
                }
            }

            IEntitySlim[] entities = EntityService.GetPagedDescendants(
                root.Id,
                UmbracoObjectTypes.Media,
                0,
                10,
                out long total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("ssss"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(10));
            Assert.That(total, Is.EqualTo(10));
            entities = EntityService.GetPagedDescendants(
                root.Id,
                UmbracoObjectTypes.Media,
                0,
                50,
                out total,
                filter: SqlContext.Query<IUmbracoEntity>().Where(x => x.Name.Contains("tttt"))).ToArray();
            Assert.That(entities.Length, Is.EqualTo(50));
            Assert.That(total, Is.EqualTo(50));
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectTypes()
        {
            IEntitySlim[] entities = EntityService.GetAll(UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_UmbracoObjectType_Id()
        {
            Guid objectTypeId = Constants.ObjectTypes.Document;
            IEntitySlim[] entities = EntityService.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Find_All_Content_By_Type()
        {
            IEntitySlim[] entities = EntityService.GetAll<IContent>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.True);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType()
        {
            IEntitySlim[] entities = EntityService.GetChildren(-1, UmbracoObjectTypes.Document).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(1));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Content_By_UmbracoObjectType_With_Variant_Names()
        {
            string alias = "test" + Guid.NewGuid();
            Template template = TemplateBuilder.CreateTextPageTemplate(alias);
            FileService.SaveTemplate(template);
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test2", "Test2", defaultTemplateId: template.Id);
            contentType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            Content c1 = ContentBuilder.CreateSimpleContent(contentType, "Test", -1);
            c1.SetCultureName("Test - FR", _langFr.IsoCode);
            c1.SetCultureName("Test - ES", _langEs.IsoCode);
            ContentService.Save(c1);

            IEntitySlim result = EntityService.Get(c1.Id, UmbracoObjectTypes.Document);
            Assert.AreEqual("Test - FR", result.Name); // got name from default culture
            Assert.IsNotNull(result as IDocumentEntitySlim);
            var doc = (IDocumentEntitySlim)result;
            IReadOnlyDictionary<string, string> cultureNames = doc.CultureNames;
            Assert.AreEqual("Test - FR", cultureNames[_langFr.IsoCode]);
            Assert.AreEqual("Test - ES", cultureNames[_langEs.IsoCode]);
        }

        [Test]
        public void EntityService_Can_Get_Child_Content_By_ParentId_And_UmbracoObjectType_With_Variant_Names()
        {
            Template template = TemplateBuilder.CreateTextPageTemplate();
            FileService.SaveTemplate(template);
            ContentType contentType = ContentTypeBuilder.CreateSimpleContentType("test1", "Test1", defaultTemplateId: template.Id);
            contentType.Variations = ContentVariation.Culture;
            ContentTypeService.Save(contentType);

            Content root = ContentBuilder.CreateSimpleContent(contentType);
            root.SetCultureName("Root", _langFr.IsoCode); // else cannot save
            ContentService.Save(root);

            for (int i = 0; i < 10; i++)
            {
                Content c1 = ContentBuilder.CreateSimpleContent(contentType, Guid.NewGuid().ToString(), root);
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

            IEntitySlim[] entities = EntityService.GetChildren(root.Id, UmbracoObjectTypes.Document).ToArray();

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
            IEnumerable<IEntitySlim> entities = EntityService.GetChildren(_folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(3));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Can_Get_Descendants_By_ParentId()
        {
            IEnumerable<IEntitySlim> entities = EntityService.GetDescendants(_folderId);

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(4));
            Assert.That(entities.Any(x => x.Trashed), Is.False);
        }

        [Test]
        public void EntityService_Throws_When_Getting_All_With_Invalid_Type()
        {
            Guid objectTypeId = Constants.ObjectTypes.ContentItem;

            Assert.Throws<NotSupportedException>(() => EntityService.GetAll<IContentBase>());
            Assert.Throws<NotSupportedException>(() => EntityService.GetAll(objectTypeId));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectTypes()
        {
            IEntitySlim[] entities = EntityService.GetAll(UmbracoObjectTypes.DocumentType).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_UmbracoObjectType_Id()
        {
            Guid objectTypeId = Constants.ObjectTypes.DocumentType;
            IEntitySlim[] entities = EntityService.GetAll(objectTypeId).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_ContentTypes_By_Type()
        {
            IEntitySlim[] entities = EntityService.GetAll<IContentType>().ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Count(), Is.EqualTo(1));
        }

        [Test]
        public void EntityService_Can_Find_All_Media_By_UmbracoObjectTypes()
        {
            IEntitySlim[] entities = EntityService.GetAll(UmbracoObjectTypes.Media).ToArray();

            Assert.That(entities.Any(), Is.True);
            Assert.That(entities.Length, Is.EqualTo(5));

            foreach (IEntitySlim entity in entities)
            {
                Assert.IsTrue(entity.GetType().Implements<IMediaEntitySlim>());
                Console.WriteLine(((IMediaEntitySlim)entity).MediaPath);
                Assert.IsNotEmpty(((IMediaEntitySlim)entity).MediaPath);
            }
        }

        [Test]
        public void EntityService_Can_Get_ObjectType()
        {
            UmbracoObjectTypes mediaObjectType = EntityService.GetObjectType(1031);

            Assert.NotNull(mediaObjectType);
            Assert.AreEqual(mediaObjectType, UmbracoObjectTypes.MediaType);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id_With_Unknown_Type()
        {
            Attempt<Guid> result = EntityService.GetKey(_contentType.Id, UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Key_For_Id()
        {
            Attempt<Guid> result = EntityService.GetKey(_contentType.Id, UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Key_For_Id_With_Incorrect_Object_Type()
        {
            Attempt<Guid> result1 = EntityService.GetKey(_contentType.Id, UmbracoObjectTypes.DocumentType);
            Attempt<Guid> result2 = EntityService.GetKey(_contentType.Id, UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key_With_Unknown_Type()
        {
            Attempt<int> result = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.Unknown);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(_contentType.Id, result.Result);
        }

        [Test]
        public void EntityService_Can_Get_Id_For_Key()
        {
            Attempt<int> result = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);

            Assert.IsTrue(result.Success);
            Assert.AreEqual(_contentType.Id, result.Result);
        }

        [Test]
        public void EntityService_Cannot_Get_Id_For_Key_With_Incorrect_Object_Type()
        {
            Attempt<int> result1 = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.DocumentType);
            Attempt<int> result2 = EntityService.GetId(Guid.Parse("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522"), UmbracoObjectTypes.MediaType);

            Assert.IsTrue(result1.Success);
            Assert.IsFalse(result2.Success);
        }

        [Test]
        public void ReserveId()
        {
            var guid = Guid.NewGuid();

            // can reserve
            int reservedId = EntityService.ReserveId(guid);
            Assert.IsTrue(reservedId > 0);

            // can get it back
            Attempt<int> id = EntityService.GetId(guid, UmbracoObjectTypes.DocumentType);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // anything goes
            id = EntityService.GetId(guid, UmbracoObjectTypes.Media);
            Assert.IsTrue(id.Success);
            Assert.AreEqual(reservedId, id.Result);

            // a random guid won't work
            Assert.IsFalse(EntityService.GetId(Guid.NewGuid(), UmbracoObjectTypes.DocumentType).Success);
        }

        private static bool s_isSetup = false;

        private int _folderId;
        private ContentType _contentType;
        private Content _textpage;
        private Content _subpage;
        private Content _subpage2;
        private Content _trashed;
        private IMediaType _folderMediaType;
        private Media _folder;
        private IMediaType _imageMediaType;
        private Media _image;
        private Media _subfolder;
        private Media _subfolder2;

        public void CreateTestData()
        {
            if (s_isSetup == false)
            {
                s_isSetup = true;

                Template template = TemplateBuilder.CreateTextPageTemplate();
                FileService.SaveTemplate(template); // else, FK violation on contentType!

                // Create and Save ContentType "umbTextpage" -> _contentType.Id
                _contentType = ContentTypeBuilder.CreateSimpleContentType("umbTextpage", "Textpage", defaultTemplateId: template.Id);
                _contentType.Key = new Guid("1D3A8E6E-2EA9-4CC1-B229-1AEE19821522");
                ContentTypeService.Save(_contentType);

                // Create and Save Content "Homepage" based on "umbTextpage" -> 1053
                _textpage = ContentBuilder.CreateSimpleContent(_contentType);
                _textpage.Key = new Guid("B58B3AD4-62C2-4E27-B1BE-837BD7C533E0");
                ContentService.Save(_textpage, 0);

                // Create and Save Content "Text Page 1" based on "umbTextpage" -> 1054
                _subpage = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 1", _textpage.Id);
                _subpage.ContentSchedule.Add(DateTime.Now.AddMinutes(-5), null);
                ContentService.Save(_subpage, 0);

                // Create and Save Content "Text Page 2" based on "umbTextpage" -> 1055
                _subpage2 = ContentBuilder.CreateSimpleContent(_contentType, "Text Page 2", _textpage.Id);
                ContentService.Save(_subpage2, 0);

                // Create and Save Content "Text Page Deleted" based on "umbTextpage" -> 1056
                _trashed = ContentBuilder.CreateSimpleContent(_contentType, "Text Page Deleted", -20);
                _trashed.Trashed = true;
                ContentService.Save(_trashed, 0);

                // Create and Save folder-Media -> 1057
                _folderMediaType = MediaTypeService.Get(1031);
                _folder = MediaBuilder.CreateMediaFolder(_folderMediaType, -1);
                MediaService.Save(_folder, 0);
                _folderId = _folder.Id;

                // Create and Save image-Media -> 1058
                _imageMediaType = MediaTypeService.Get(1032);
                _image = MediaBuilder.CreateMediaImage(_imageMediaType, _folder.Id);
                MediaService.Save(_image, 0);

                // Create and Save file-Media -> 1059
                IMediaType fileMediaType = MediaTypeService.Get(1033);
                Media file = MediaBuilder.CreateMediaFile(fileMediaType, _folder.Id);
                MediaService.Save(file, 0);

                // Create and save sub folder -> 1060
                _subfolder = MediaBuilder.CreateMediaFolder(_folderMediaType, _folder.Id);
                MediaService.Save(_subfolder, 0);

                // Create and save sub folder -> 1061
                _subfolder2 = MediaBuilder.CreateMediaFolder(_folderMediaType, _subfolder.Id);
                MediaService.Save(_subfolder2, 0);
            }
        }
    }
}
