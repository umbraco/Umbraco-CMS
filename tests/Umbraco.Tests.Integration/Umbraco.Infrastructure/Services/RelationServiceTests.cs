// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;
using Constants = Umbraco.Cms.Core.Constants;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Services
{
    [TestFixture]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationServiceTests : UmbracoIntegrationTest
    {
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();

        private IContentService ContentService => GetRequiredService<IContentService>();

        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();

        private IMediaService MediaService => GetRequiredService<IMediaService>();

        private IRelationService RelationService => GetRequiredService<IRelationService>();

        [Test]
        public void Get_Paged_Relations_By_Relation_Type()
        {
            // Create content
            var createdContent = new List<IContent>();
            ContentType contentType = ContentTypeBuilder.CreateBasicContentType("blah");
            ContentTypeService.Save(contentType);
            for (int i = 0; i < 3; i++)
            {
                Content c1 = ContentBuilder.CreateBasicContent(contentType);
                ContentService.Save(c1);
                createdContent.Add(c1);
            }

            // Create media
            var createdMedia = new List<IMedia>();
            MediaType imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            MediaTypeService.Save(imageType);
            for (int i = 0; i < 3; i++)
            {
                Media c1 = MediaBuilder.CreateMediaImage(imageType, -1);
                MediaService.Save(c1);
                createdMedia.Add(c1);
            }

            IRelationType relType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);

            // Relate content to media
            foreach (IContent content in createdContent)
            {
                foreach (IMedia media in createdMedia)
                {
                    RelationService.Relate(content.Id, media.Id, relType);
                }
            }

            var paged = RelationService.GetPagedByRelationTypeId(relType.Id, 0, 4, out long totalRecs).ToList();

            Assert.AreEqual(9, totalRecs);
            Assert.AreEqual(4, paged.Count);

            // next page
            paged.AddRange(RelationService.GetPagedByRelationTypeId(relType.Id, 1, 4, out totalRecs));

            Assert.AreEqual(9, totalRecs);
            Assert.AreEqual(8, paged.Count);

            Assert.IsTrue(createdContent.Select(x => x.Id).ContainsAll(paged.Select(x => x.ParentId)));
            Assert.IsTrue(createdMedia.Select(x => x.Id).ContainsAll(paged.Select(x => x.ChildId)));
        }

        [Test]
        public void Return_List_Of_Content_Items_Where_Media_Item_Referenced()
        {
            MediaType mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
            MediaTypeService.Save(mt);
            Media m1 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
            MediaService.Save(m1);

            ContentType ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest");
            ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
            ContentTypeService.Save(ct);

            void CreateContentWithMediaRefs()
            {
                Content content = ContentBuilder.CreateTextpageContent(ct, "my content 2", -1);

                // 'bodyText' is a property with a RTE property editor which we knows automatically tracks relations
                content.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p>");
                ContentService.Save(content);
            }

            for (int i = 0; i < 6; i++)
            {
                CreateContentWithMediaRefs(); // create 6 content items referencing the same media
            }

            var relations = RelationService.GetByChildId(m1.Id, Constants.Conventions.RelationTypes.RelatedMediaAlias).ToList();
            Assert.AreEqual(6, relations.Count);

            var entities = RelationService.GetParentEntitiesFromRelations(relations).ToList();
            Assert.AreEqual(6, entities.Count);
        }

        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            IRelationService rs = RelationService;
            IRelationType rt = new RelationType("Test", "repeatedEventOccurence", false, Constants.ObjectTypes.Document, Constants.ObjectTypes.Media);

            Assert.DoesNotThrow(() => rs.Save(rt));

            // re-get
            rt = RelationService.GetRelationTypeById(rt.Id);

            Assert.AreEqual("Test", rt.Name);
            Assert.AreEqual("repeatedEventOccurence", rt.Alias);
            Assert.AreEqual(false, rt.IsBidirectional);
            Assert.AreEqual(Constants.ObjectTypes.Document, rt.ParentObjectType.Value);
            Assert.AreEqual(Constants.ObjectTypes.Media, rt.ChildObjectType.Value);
        }

        [Test]
        public void Create_Relation_Type_Without_Object_Types()
        {
            IRelationService rs = RelationService;
            IRelationType rt = new RelationType("repeatedEventOccurence", "repeatedEventOccurence", false, null, null);

            Assert.DoesNotThrow(() => rs.Save(rt));

            // re-get
            rt = RelationService.GetRelationTypeById(rt.Id);

            Assert.IsNull(rt.ChildObjectType);
            Assert.IsNull(rt.ParentObjectType);
        }

        [Test]
        public void Relation_Returns_Parent_Child_Object_Types_When_Creating()
        {
            IRelation r = CreateAndSaveRelation("Test", "test");

            Assert.AreEqual(Constants.ObjectTypes.Document, r.ParentObjectType);
            Assert.AreEqual(Constants.ObjectTypes.Media, r.ChildObjectType);
        }

        [Test]
        public void Relation_Returns_Parent_Child_Object_Types_When_Getting()
        {
            IRelation r = CreateAndSaveRelation("Test", "test");

            // re-get
            r = RelationService.GetById(r.Id);

            Assert.AreEqual(Constants.ObjectTypes.Document, r.ParentObjectType);
            Assert.AreEqual(Constants.ObjectTypes.Media, r.ChildObjectType);
        }

        [Test]
        public void Insert_Bulk_Relations()
        {
            IRelationService rs = RelationService;

            IEnumerable<IRelation> newRelations = CreateRelations(10);

            Assert.IsTrue(newRelations.All(x => !x.HasIdentity));

            RelationService.Save(newRelations);

            Assert.IsTrue(newRelations.All(x => x.HasIdentity));
        }

        [Test]
        public void Update_Bulk_Relations()
        {
            IRelationService rs = RelationService;

            DateTime date = DateTime.Now.AddDays(-10);
            IEnumerable<IRelation> newRelations = CreateRelations(10);
            foreach (IRelation r in newRelations)
            {
                r.CreateDate = date;
                r.UpdateDate = date;
            }

            // insert
            RelationService.Save(newRelations);
            Assert.IsTrue(newRelations.All(x => x.UpdateDate == date));

            DateTime newDate = DateTime.Now.AddDays(-5);
            foreach (IRelation r in newRelations)
            {
                r.UpdateDate = newDate;
            }

            // update
            RelationService.Save(newRelations);
            Assert.IsTrue(newRelations.All(x => x.UpdateDate == newDate));
        }

        private IRelation CreateAndSaveRelation(string name, string alias)
        {
            IRelationService rs = RelationService;
            var rt = new RelationType(name, alias, false, null, null);
            rs.Save(rt);

            ContentType ct = ContentTypeBuilder.CreateBasicContentType();
            ContentTypeService.Save(ct);

            MediaType mt = MediaTypeBuilder.CreateImageMediaType("img");
            MediaTypeService.Save(mt);

            Content c1 = ContentBuilder.CreateBasicContent(ct);
            Media c2 = MediaBuilder.CreateMediaImage(mt, -1);
            ContentService.Save(c1);
            MediaService.Save(c2);

            var r = new Relation(c1.Id, c2.Id, rt);
            RelationService.Save(r);

            return r;
        }

        /// <summary>
        /// Creates a bunch of content/media items return relation objects for them (unsaved)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private IEnumerable<IRelation> CreateRelations(int count)
        {
            IRelationService rs = RelationService;
            string rtName = Guid.NewGuid().ToString();
            var rt = new RelationType(rtName, rtName, false, null, null);
            rs.Save(rt);

            ContentType ct = ContentTypeBuilder.CreateBasicContentType();
            ContentTypeService.Save(ct);

            MediaType mt = MediaTypeBuilder.CreateImageMediaType("img");
            MediaTypeService.Save(mt);

            return Enumerable.Range(1, count).Select(index =>
            {
                Content c1 = ContentBuilder.CreateBasicContent(ct);
                Media c2 = MediaBuilder.CreateMediaImage(mt, -1);
                ContentService.Save(c1);
                MediaService.Save(c2);

                return new Relation(c1.Id, c2.Id, rt);
            }).ToList();
        }

        // TODO: Create a relation for entities of the wrong Entity Type (GUID) based on the Relation Type's defined parent/child object types
    }
}