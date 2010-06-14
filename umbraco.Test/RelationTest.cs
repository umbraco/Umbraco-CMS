using umbraco.cms.businesslogic.relation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using umbraco.cms.businesslogic;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for RelationTest and is intended
    ///to contain all RelationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class RelationTest
    {

        ///// <summary>
        ///// Creates 2 documents and relates them
        /////</summary>
        //[TestMethod()]
        //public void Relation_Make_New()
        //{
            
            
            
        //    actual = Relation.MakeNew(ParentId, ChildId, RelType, Comment);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        #region Tests to write

        ///// <summary>
        /////A test for Relation Constructor
        /////</summary>
        //[TestMethod()]
        //public void RelationConstructorTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetRelations
        /////</summary>
        //[TestMethod()]
        //public void GetRelationsTest()
        //{
        //    int NodeId = 0; // TODO: Initialize to an appropriate value
        //    RelationType Filter = null; // TODO: Initialize to an appropriate value
        //    Relation[] expected = null; // TODO: Initialize to an appropriate value
        //    Relation[] actual;
        //    actual = Relation.GetRelations(NodeId, Filter);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetRelations
        /////</summary>
        //[TestMethod()]
        //public void GetRelationsTest1()
        //{
        //    int NodeId = 0; // TODO: Initialize to an appropriate value
        //    Relation[] expected = null; // TODO: Initialize to an appropriate value
        //    Relation[] actual;
        //    actual = Relation.GetRelations(NodeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetRelationsAsList
        /////</summary>
        //[TestMethod()]
        //public void GetRelationsAsListTest()
        //{
        //    int NodeId = 0; // TODO: Initialize to an appropriate value
        //    List<Relation> expected = null; // TODO: Initialize to an appropriate value
        //    List<Relation> actual;
        //    actual = Relation.GetRelationsAsList(NodeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IsRelated
        /////</summary>
        //[TestMethod()]
        //public void IsRelatedTest()
        //{
        //    int ParentID = 0; // TODO: Initialize to an appropriate value
        //    int ChildId = 0; // TODO: Initialize to an appropriate value
        //    RelationType Filter = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = Relation.IsRelated(ParentID, ChildId, Filter);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IsRelated
        /////</summary>
        //[TestMethod()]
        //public void IsRelatedTest1()
        //{
        //    int ParentID = 0; // TODO: Initialize to an appropriate value
        //    int ChildId = 0; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = Relation.IsRelated(ParentID, ChildId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Child
        /////</summary>
        //[TestMethod()]
        //public void ChildTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    CMSNode expected = null; // TODO: Initialize to an appropriate value
        //    CMSNode actual;
        //    target.Child = expected;
        //    actual = target.Child;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Comment
        /////</summary>
        //[TestMethod()]
        //public void CommentTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Comment = expected;
        //    actual = target.Comment;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for CreateDate
        /////</summary>
        //[TestMethod()]
        //public void CreateDateTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    DateTime actual;
        //    actual = target.CreateDate;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.Id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Parent
        /////</summary>
        //[TestMethod()]
        //public void ParentTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    CMSNode expected = null; // TODO: Initialize to an appropriate value
        //    CMSNode actual;
        //    target.Parent = expected;
        //    actual = target.Parent;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RelType
        /////</summary>
        //[TestMethod()]
        //public void RelTypeTest()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Relation target = new Relation(Id); // TODO: Initialize to an appropriate value
        //    RelationType expected = null; // TODO: Initialize to an appropriate value
        //    RelationType actual;
        //    target.RelType = expected;
        //    actual = target.RelType;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //} 
        #endregion

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion
    }
}
