using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Tests.Common.Builders;
using Umbraco.Tests.Common.Builders.Extensions;
using Umbraco.Tests.Integration.Testing;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Integration.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationServiceTests : UmbracoIntegrationTest
    {
        private IContentTypeService ContentTypeService => GetRequiredService<IContentTypeService>();
        private IMediaTypeService MediaTypeService => GetRequiredService<IMediaTypeService>();
        private IContentService ContentService => GetRequiredService<IContentService>();
        private IMediaService MediaService => GetRequiredService<IMediaService>();
        private IRelationService RelationService => GetRequiredService<IRelationService>();

        [Test]
        public void Get_Paged_Relations_By_Relation_Type()
        {
            // Create content
            var createdContent = new List<IContent>();
            var contentType = ContentTypeBuilder.CreateBasicContentType("blah");
            ContentTypeService.Save(contentType);
            for (var i = 0; i < 10; i++)
            {
                var c1 = ContentBuilder.CreateBasicContent(contentType);
                ContentService.Save(c1);
                createdContent.Add(c1);
            }

            // Create media
            var createdMedia = new List<IMedia>();
            var imageType = MediaTypeBuilder.CreateImageMediaType("myImage");
            MediaTypeService.Save(imageType);
            for (var i = 0; i < 10; i++)
            {
                var c1 = MediaBuilder.CreateMediaImage(imageType, -1);
                MediaService.Save(c1);
                createdMedia.Add(c1);
            }

            var relType = RelationService.GetRelationTypeByAlias(Constants.Conventions.RelationTypes.RelatedMediaAlias);

            // Relate content to media
            foreach (var content in createdContent)
                foreach (var media in createdMedia)
                    RelationService.Relate(content.Id, media.Id, relType);

            var paged = RelationService.GetPagedByRelationTypeId(relType.Id, 0, 51, out var totalRecs).ToList();

            Assert.AreEqual(100, totalRecs);
            Assert.AreEqual(51, paged.Count);

            // Next page
            paged.AddRange(RelationService.GetPagedByRelationTypeId(relType.Id, 1, 51, out totalRecs));

            Assert.AreEqual(100, totalRecs);
            Assert.AreEqual(100, paged.Count);

            Assert.IsTrue(createdContent.Select(x => x.Id).ContainsAll(paged.Select(x => x.ParentId)));
            Assert.IsTrue(createdMedia.Select(x => x.Id).ContainsAll(paged.Select(x => x.ChildId)));
        }

        [Test]
        public void Return_List_Of_Content_Items_Where_Media_Item_Referenced()
        {
            var mt = MediaTypeBuilder.CreateSimpleMediaType("testMediaType", "Test Media Type");
            MediaTypeService.Save(mt);
            var m1 = MediaBuilder.CreateSimpleMedia(mt, "hello 1", -1);
            MediaService.Save(m1);

            var ct = ContentTypeBuilder.CreateTextPageContentType("richTextTest");
            ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
            ContentTypeService.Save(ct);

            void createContentWithMediaRefs()
            {
                var content = ContentBuilder.CreateTextpageContent(ct, "my content 2", -1);
                //'bodyText' is a property with a RTE property editor which we knows automatically tracks relations
                content.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p>");
                ContentService.Save(content);
            }

            for (var i = 0; i < 6; i++)
                createContentWithMediaRefs(); //create 6 content items referencing the same media

            var relations = RelationService.GetByChildId(m1.Id, Constants.Conventions.RelationTypes.RelatedMediaAlias).ToList();
            Assert.AreEqual(6, relations.Count);

            var entities = RelationService.GetParentEntitiesFromRelations(relations).ToList();
            Assert.AreEqual(6, entities.Count);
        }

        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            var rs = RelationService;
            var rt = new RelationTypeBuilder()
                .WithName("Test")
                .WithAlias("repeatedEventOccurence")
                .WithParentObjectType(Constants.ObjectTypes.Document)
                .WithChildObjectType(Constants.ObjectTypes.Media)
                .Build();

            Assert.DoesNotThrow(() => rs.Save(rt));

            //re-get
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
            var rs = RelationService;
            var rt = new RelationTypeBuilder()
                .WithName("repeatedEventOccurence")
                .WithAlias("repeatedEventOccurence")
                .Build();

            Assert.DoesNotThrow(() => rs.Save(rt));

            //re-get
            rt = RelationService.GetRelationTypeById(rt.Id);

            Assert.IsNull(rt.ChildObjectType);
            Assert.IsNull(rt.ParentObjectType);
        }

        [Test]
        public void Relation_Returns_Parent_Child_Object_Types_When_Creating()
        {
            var r = CreateAndSaveRelation("Test", "test");

            Assert.AreEqual(Constants.ObjectTypes.Document, r.ParentObjectType);
            Assert.AreEqual(Constants.ObjectTypes.Media, r.ChildObjectType);
        }

        [Test]
        public void Relation_Returns_Parent_Child_Object_Types_When_Getting()
        {
            var r = CreateAndSaveRelation("Test", "test");

            // re-get
            r = RelationService.GetById(r.Id);

            Assert.AreEqual(Constants.ObjectTypes.Document, r.ParentObjectType);
            Assert.AreEqual(Constants.ObjectTypes.Media, r.ChildObjectType);
        }

        [Test]
        public void Insert_Bulk_Relations()
        {
            var rs = RelationService;

            var relations = CreateRelations(10);

            Assert.IsTrue(relations.All(x => !x.HasIdentity));

            RelationService.Save(relations);

            var insertedRelations = RelationService.GetAllRelations();

            Assert.AreEqual(10, insertedRelations.Count());
        }

        [Test]
        public void Update_Bulk_Relations()
        {
            var date = DateTime.Now.AddDays(-10);
            var relations = CreateRelations(10);
            foreach (var r in relations)
            {
                r.CreateDate = date;
                r.UpdateDate = date;
            }

            // Insert
            RelationService.Save(relations);
            Assert.IsTrue(relations.All(x => x.UpdateDate == date));

            // Re-get the relations to ensure we have updated IDs.
            relations = RelationService.GetAllRelations();

            // Update
            var newDate = DateTime.Now.AddDays(-5);
            foreach (var r in relations)
            {
                r.UpdateDate = newDate;
            }

            RelationService.Save(relations);
            Assert.IsTrue(relations.All(x => x.UpdateDate == newDate));
        }

        private IRelation CreateAndSaveRelation(string name, string alias)
        {
            var rs = RelationService;
            var rt = new RelationTypeBuilder()
                .WithName(name)
                .WithAlias(alias)
                .Build();
            rs.Save(rt);

            var ct = ContentTypeBuilder.CreateBasicContentType();
            ContentTypeService.Save(ct);

            var mt = MediaTypeBuilder.CreateImageMediaType("img");
            MediaTypeService.Save(mt);

            var c1 = ContentBuilder.CreateBasicContent(ct);
            var c2 = MediaBuilder.CreateMediaImage(mt, -1);
            ContentService.Save(c1);
            MediaService.Save(c2);

            var r = new RelationBuilder()
                .BetweenIds(c1.Id, c2.Id)
                .WithRelationType(rt)
                .Build();
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
            var rs = RelationService;
            var rtName = Guid.NewGuid().ToString();
            var rt = new RelationTypeBuilder().Build();
            rs.Save(rt);

            var ct = ContentTypeBuilder.CreateBasicContentType();
            ContentTypeService.Save(ct);

            var mt = MediaTypeBuilder.CreateImageMediaType("img");
            MediaTypeService.Save(mt);

            return Enumerable.Range(1, count).Select(index =>
            {
                var c1 = ContentBuilder.CreateBasicContent(ct);
                var c2 = MediaBuilder.CreateMediaImage(mt, -1);
                ContentService.Save(c1);
                MediaService.Save(c2);

                return new RelationBuilder()
                    .BetweenIds(c1.Id, c2.Id)
                    .WithRelationType(rt)
                    .Build();
            }).ToList();
        }

        //TODO: Create a relation for entities of the wrong Entity Type (GUID) based on the Relation Type's defined parent/child object types
    }
}
