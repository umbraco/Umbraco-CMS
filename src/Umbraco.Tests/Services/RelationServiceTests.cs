using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.TestHelpers.Entities;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Services
{
    [TestFixture]
    [Apartment(ApartmentState.STA)]
    [UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
    public class RelationServiceTests : TestWithSomeContentBase
    {
        [Test]
        public void Return_List_Of_Content_Items_Where_Media_Item_Referenced()
        {
            var mt = MockedContentTypes.CreateSimpleMediaType("testMediaType", "Test Media Type");
            ServiceContext.MediaTypeService.Save(mt);
            var m1 = MockedMedia.CreateSimpleMedia(mt, "hello 1", -1);
            ServiceContext.MediaService.Save(m1);

            var ct = MockedContentTypes.CreateTextPageContentType("richTextTest");
            ct.AllowedTemplates = Enumerable.Empty<ITemplate>();
            ServiceContext.ContentTypeService.Save(ct);

            void createContentWithMediaRefs()
            {
                var content = MockedContent.CreateTextpageContent(ct, "my content 2", -1);
                //'bodyText' is a property with a RTE property editor which we knows automatically tracks relations
                content.Properties["bodyText"].SetValue(@"<p>
        <img src='/media/12312.jpg' data-udi='umb://media/" + m1.Key.ToString("N") + @"' />
</p>");
                ServiceContext.ContentService.Save(content);
            }

            for (var i = 0; i < 6; i++)
                createContentWithMediaRefs(); //create 6 content items referencing the same media

            var relations = ServiceContext.RelationService.GetByChildId(m1.Id, Constants.Conventions.RelationTypes.RelatedMediaAlias).ToList();
            Assert.AreEqual(6, relations.Count);

            var entities = ServiceContext.RelationService.GetParentEntitiesFromRelations(relations).ToList();
            Assert.AreEqual(6, entities.Count);
        }

        [Test]
        public void Can_Create_RelationType_Without_Name()
        {
            var rs = ServiceContext.RelationService;
            IRelationType rt = new RelationType("Test", "repeatedEventOccurence", false, Constants.ObjectTypes.Document, Constants.ObjectTypes.Media);

            Assert.DoesNotThrow(() => rs.Save(rt));

            //re-get
            rt = ServiceContext.RelationService.GetRelationTypeById(rt.Id);

            Assert.AreEqual("Test", rt.Name);
            Assert.AreEqual("repeatedEventOccurence", rt.Alias);
            Assert.AreEqual(false, rt.IsBidirectional);
            Assert.AreEqual(Constants.ObjectTypes.Document, rt.ChildObjectType.Value);
            Assert.AreEqual(Constants.ObjectTypes.Media, rt.ParentObjectType.Value);
        }

        [Test]
        public void Create_Relation_Type_Without_Object_Types()
        {
            var rs = ServiceContext.RelationService;
            IRelationType rt = new RelationType("repeatedEventOccurence", "repeatedEventOccurence", false, null, null);

            Assert.DoesNotThrow(() => rs.Save(rt));

            //re-get
            rt = ServiceContext.RelationService.GetRelationTypeById(rt.Id);

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
            r = ServiceContext.RelationService.GetById(r.Id);

            Assert.AreEqual(Constants.ObjectTypes.Document, r.ParentObjectType);
            Assert.AreEqual(Constants.ObjectTypes.Media, r.ChildObjectType);
        }

        [Test]
        public void Insert_Bulk_Relations()
        {
            var rs = ServiceContext.RelationService;

            var newRelations = CreateRelations(10);

            Assert.IsTrue(newRelations.All(x => !x.HasIdentity));

            ServiceContext.RelationService.Save(newRelations);

            Assert.IsTrue(newRelations.All(x => x.HasIdentity));
        }

        [Test]
        public void Update_Bulk_Relations()
        {
            var rs = ServiceContext.RelationService;

            var date = DateTime.Now.AddDays(-10);
            var newRelations = CreateRelations(10);
            foreach (var r in newRelations)
            {
                r.CreateDate = date;
                r.UpdateDate = date;
            }   

            //insert
            ServiceContext.RelationService.Save(newRelations);
            Assert.IsTrue(newRelations.All(x => x.UpdateDate == date));

            var newDate = DateTime.Now.AddDays(-5);
            foreach (var r in newRelations)
                r.UpdateDate = newDate;

            //update
            ServiceContext.RelationService.Save(newRelations);
            Assert.IsTrue(newRelations.All(x => x.UpdateDate == newDate));
        }

        private IRelation CreateAndSaveRelation(string name, string alias)
        {
            var rs = ServiceContext.RelationService;
            var rt = new RelationType(name, alias, false, null, null);
            rs.Save(rt);

            var ct = MockedContentTypes.CreateBasicContentType();
            ServiceContext.ContentTypeService.Save(ct);

            var mt = MockedContentTypes.CreateImageMediaType("img");
            ServiceContext.MediaTypeService.Save(mt);

            var c1 = MockedContent.CreateBasicContent(ct);
            var c2 = MockedMedia.CreateMediaImage(mt, -1);
            ServiceContext.ContentService.Save(c1);
            ServiceContext.MediaService.Save(c2);

            var r = new Relation(c1.Id, c2.Id, rt);
            ServiceContext.RelationService.Save(r);

            return r;
        }

        /// <summary>
        /// Creates a bunch of content/media items return relation objects for them (unsaved)
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private IEnumerable<IRelation> CreateRelations(int count)
        {
            var rs = ServiceContext.RelationService;
            var rtName = Guid.NewGuid().ToString();
            var rt = new RelationType(rtName, rtName, false, null, null);
            rs.Save(rt);

            var ct = MockedContentTypes.CreateBasicContentType();
            ServiceContext.ContentTypeService.Save(ct);

            var mt = MockedContentTypes.CreateImageMediaType("img");
            ServiceContext.MediaTypeService.Save(mt);

            return Enumerable.Range(1, count).Select(index =>
            {
                var c1 = MockedContent.CreateBasicContent(ct);
                var c2 = MockedMedia.CreateMediaImage(mt, -1);
                ServiceContext.ContentService.Save(c1);
                ServiceContext.MediaService.Save(c2);

                return new Relation(c1.Id, c2.Id, rt);
            }).ToList();
        }

        //TODO: Create a relation for entities of the wrong Entity Type (GUID) based on the Relation Type's defined parent/child object types
    }
}
