using System.Collections.Generic;
using NUnit.Framework;
using umbraco.BusinessLogic;
using Umbraco.Tests.TestHelpers;
using umbraco.cms.businesslogic.datatype;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using System.Reflection;
using umbraco.cms.businesslogic;
using System;
using System.Xml;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.web;
using umbraco.cms.businesslogic.relation;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_Relation_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Relation_TestData(); }

        [Test(Description = "Verify if EnsureData() and related helper test methods execute well")]
        public void _1st_Test_Relation_EnsureData()
        {
            Assert.IsTrue(initialized);

            Assert.That(_node1, !Is.Null);   
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            Guid objectType = Document._objectType;
            int expectedValue = independentDatabase.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @objectType", new { objectType});
            Assert.That(CMSNode.CountByObjectType(Document._objectType), Is.EqualTo(expectedValue));

            Assert.That(_relationType1, !Is.Null);
            Assert.That(_relationType2, !Is.Null);
            Assert.That(_relation1, !Is.Null);
            Assert.That(_relation2, !Is.Null);
            Assert.That(_relation3, !Is.Null);

            EnsureAll_Relation_TestRecordsAreDeleted();

            expectedValue = independentDatabase.ExecuteScalar<int>("SELECT COUNT(*) from umbracoNode WHERE nodeObjectType = @objectType", new { objectType });
            Assert.That(CMSNode.CountByObjectType(Document._objectType), Is.EqualTo(expectedValue));

            Assert.That(RelationType.GetById(_relationType1.Id), Is.Null);
            Assert.That(RelationType.GetById(_relationType2.Id), Is.Null);
            Assert.That(Relation.GetRelations(_relation1.Id).Length, Is.EqualTo(0));
            Assert.That(Relation.GetRelations(_relation2.Id).Length, Is.EqualTo(0));
            Assert.That(Relation.GetRelations(_relation3.Id).Length, Is.EqualTo(0));

        }

        [Test(Description = "Test 'public Relation(int Id)' constructor")]
        public void _2nd_Test_Relation_Constructor()
        {
            // persisted test relation
            var testRelation = new Relation(_relation1.Id);
            Assert.That(_relation1.Id, Is.EqualTo(testRelation.Id));

            // not persisted Relation
            Assert.Throws(typeof(ArgumentException), delegate { new Relation(12345); });
        }

        [Test(Description = "Test 'public CMSNode Parent' property.set")]
        public void Test_Relation_Parent_set()
        {
            var newValue = _node4;
            var expectedValue = newValue.Id;
            Setter_Persists_Ext<Relation, CMSNode, int>(
                    n => n.Parent,
                    n => n.Parent = newValue,
                    "umbracoRelation",
                    "parentid",
                    expectedValue,
                    "Id",
                    _relation1.Id,
                    useSecondGetter: true,
                    getter2: n => n.Parent.Id,
                    oldValue2: _relation1.ParentId
                );
        }

        [Test(Description = "Test 'public CMSNode Child' property.set")]
        public void Test_Relation_Child_set()
        {
            var newValue = _node4;
            var expectedValue = newValue.Id;
            Setter_Persists_Ext<Relation, CMSNode, int>(
                    n => n.Child,
                    n => n.Child = newValue,
                    "umbracoRelation",
                    "childid",
                    expectedValue,
                    "Id",
                    _relation1.Id,
                    useSecondGetter: true,
                    getter2: n => n.Child.Id,
                    oldValue2: _relation1.ChildId 
                );
        }

        [Test(Description = "Test 'public string Comment' property.set")]
        public void Test_Relation_Comment_set()
        {
            var newValue = "new comment";
            var expectedValue = newValue;
            Setter_Persists_Ext<Relation, string, string>(
                    n => n.Comment,
                    n => n.Comment = newValue,
                    "umbracoRelation",
                    "comment",
                    expectedValue,
                    "Id",
                    _relation1.Id
                );
        }

        [Test(Description = "Test 'public RelationType RelType' property.set")]
        public void Test_Relation_RelationType_set()
        {
           var newValue = new RelationType(_relationType2.Id);
            var expectedValue = newValue.Id;
            Setter_Persists_Ext<Relation, RelationType, int>(
                    n => n.RelType,
                    n => n.RelType = newValue,
                    "umbracoRelation",
                    "reltype",
                    expectedValue,
                    "Id",
                    _relation1.Id,
                    useSecondGetter: true,
                    getter2: n => n.RelType.Id,
                    oldValue2: _relation1.RelationType 
                );
        }

        [Test(Description = "Test 'public Delete' method")]
        public void Test_Relation_Delete()
        {
            var relationId = _relation1.Id;

            // before relation is deleted
            var testRelation1 = new Relation(relationId);
            Assert.That(testRelation1.Id, Is.EqualTo(_relation1.Id));

            testRelation1.Delete();

            // after relation is deleted
            Assert.Throws<ArgumentException>( () => { new Relation(relationId); });
            Assert.That(getDto<RelationDto>(relationId), Is.Null);

            initialized = false;
        }
        
        [Test(Description = "Test 'public static Relation MakeNew(int parentId, int childId, RelationType relType, string comment)' method")]
        public void Test_Relation_MakeNew()
        {
            var  testRelation1 = Relation.MakeNew(_node4.Id, _node5.Id, new RelationType(_relationType2.Id), "Test Relation MakeNew");
            int testRelationId = testRelation1.Id;

            var testRelation2 = getDto<RelationDto>(testRelationId);

            Assert.That(testRelationId, Is.EqualTo(testRelation2.Id));
        }

        [Test(Description = "Test 'public static Relation[] GetRelations(int NodeId)' method")]
        public void Test_Relation_GetRelations_1()
        {
            var relations = Relation.GetRelations(_node1.Id);
            // there are two test relations for _node1 created in EnsureData()
            Assert.That(relations.Length, Is.EqualTo(2));
        }

        [Test(Description = "public static List<Relation> GetRelationsAsList(int NodeId)' method")]
        public void Test_Relation_GetRelationsAsList()
        {
            var relations = Relation.GetRelationsAsList(_node1.Id);
            // there are two test relations for _node1 created in EnsureData()
            Assert.That(relations.Count, Is.EqualTo(2));
        }

        [Test(Description = "Test 'public static Relation[] GetRelations(int NodeId, RelationType Filter)' method")]
        public void Test_Relation_GetRelations_2()
        {
            var relations1 = Relation.GetRelations(_node1.Id, new RelationType(_relationType1.Id));
            // there is one test relation of type _ralationType1 for _node1 created in EnsureData()
            Assert.That(relations1.Length, Is.EqualTo(1));

            var relations2 = Relation.GetRelations(_node1.Id, new RelationType(_relationType2.Id));
            // there is one test relation of type _ralationType2 for _node1 created in EnsureData()
            Assert.That(relations1.Length, Is.EqualTo(1));
        }

        [Test(Description = "Test 'public static bool IsRelated(int ParentID, int ChildId)' method")]
        public void Test_Relation_IsRelated_1()
        {
            var result1 = Relation.IsRelated(_node1.Id, _node2.Id);
            Assert.IsTrue(result1);
            var result2 = Relation.IsRelated(_node1.Id, _node4.Id);
            Assert.IsFalse(result2);
        }

        [Test(Description = "Test 'public static bool IsRelated(int ParentID, int ChildId, RelationType Filter)' method")]
        public void Test_Relation_IsRelated_2()
        {
            var result1 = Relation.IsRelated(_node1.Id, _node2.Id, new RelationType(_relationType1.Id));
            Assert.IsTrue(result1);
            var result2 = Relation.IsRelated(_node1.Id, _node2.Id, new RelationType(_relationType2.Id));
            Assert.IsFalse(result2);
        }

        [Test(Description = "Test 'internal PopulateFromDto(RelationDto relationDto)' method")]
        public void Test_Relation_PopulateFromDto()
        {
            var testRelation = new Relation(_relation1.Id);
            Assert.That(testRelation.Id, Is.EqualTo(_relation1.Id));    
            Assert.That(testRelation.Parent.Id, Is.EqualTo(_relation1.ParentId));    
            Assert.That(testRelation.Child.Id, Is.EqualTo(_relation1.ChildId));    
            Assert.That(testRelation.RelType.Id, Is.EqualTo(_relation1.RelationType));    
            Assert.That(testRelation.Comment, Is.EqualTo(_relation1.Comment));    
            Assert.That(testRelation.CreateDate, Is.EqualTo(_relation1.Datetime));    
        }

    }
}
