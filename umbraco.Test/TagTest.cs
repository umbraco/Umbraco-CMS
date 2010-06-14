using umbraco.cms.businesslogic.Tags;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.cms.businesslogic.web;
using System.Collections.Generic;
using umbraco.cms.businesslogic;
using System.Linq;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for TagTest and is intended
    ///to contain all TagTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TagTest
    {

        /// <summary>
        /// Create a new tag and delete it
        ///</summary>
        [TestMethod()]
        public void Tag_Make_New()
        {
            var t = Tag.AddTag(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"));
            Assert.IsTrue(t > 0); //id should be greater than zero
            Assert.AreEqual<int>(1, Tag.GetTags().Where(x => x.Id == t).Count());

            Tag.RemoveTag(t);
            //make sure it's gone
            Assert.AreEqual<int>(0, Tag.GetTags().Where(x => x.Id == t).Count());
         
        }


        #region Tests to write

        ///// <summary>
        /////A test for Tag Constructor
        /////</summary>
        //[TestMethod()]
        //public void TagConstructorTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    string tag = string.Empty; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    int nodeCount = 0; // TODO: Initialize to an appropriate value
        //    Tag target = new Tag(id, tag, group, nodeCount);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Tag Constructor
        /////</summary>
        //[TestMethod()]
        //public void TagConstructorTest1()
        //{
        //    Tag target = new Tag();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        

        ///// <summary>
        /////A test for AddTagsToNode
        /////</summary>
        //[TestMethod()]
        //public void AddTagsToNodeTest()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    string tags = string.Empty; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    Tag.AddTagsToNode(nodeId, tags, group);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for AssociateTagToNode
        /////</summary>
        //[TestMethod()]
        //public void AssociateTagToNodeTest()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    int tagId = 0; // TODO: Initialize to an appropriate value
        //    Tag.AssociateTagToNode(nodeId, tagId);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetDocumentsWithTags
        /////</summary>
        //[TestMethod()]
        //public void GetDocumentsWithTagsTest()
        //{
        //    string tags = string.Empty; // TODO: Initialize to an appropriate value
        //    IEnumerable<Document> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<Document> actual;
        //    actual = Tag.GetDocumentsWithTags(tags);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetNodesWithTags
        /////</summary>
        //[TestMethod()]
        //public void GetNodesWithTagsTest()
        //{
        //    string tags = string.Empty; // TODO: Initialize to an appropriate value
        //    IEnumerable<CMSNode> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<CMSNode> actual;
        //    actual = Tag.GetNodesWithTags(tags);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTagId
        /////</summary>
        //[TestMethod()]
        //public void GetTagIdTest()
        //{
        //    string tag = string.Empty; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Tag.GetTagId(tag, group);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTags
        /////</summary>
        //[TestMethod()]
        //public void GetTagsTest()
        //{
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> actual;
        //    actual = Tag.GetTags(group);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTags
        /////</summary>
        //[TestMethod()]
        //public void GetTagsTest1()
        //{
        //    IEnumerable<Tag> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> actual;
        //    actual = Tag.GetTags();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTags
        /////</summary>
        //[TestMethod()]
        //public void GetTagsTest2()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> actual;
        //    actual = Tag.GetTags(nodeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTags
        /////</summary>
        //[TestMethod()]
        //public void GetTagsTest3()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<Tag> actual;
        //    actual = Tag.GetTags(nodeId, group);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RemoveTagFromNode
        /////</summary>
        //[TestMethod()]
        //public void RemoveTagFromNodeTest()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    string tag = string.Empty; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    Tag.RemoveTagFromNode(nodeId, tag, group);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for RemoveTagsFromNode
        /////</summary>
        //[TestMethod()]
        //public void RemoveTagsFromNodeTest()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    string group = string.Empty; // TODO: Initialize to an appropriate value
        //    Tag.RemoveTagsFromNode(nodeId, group);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for RemoveTagsFromNode
        /////</summary>
        //[TestMethod()]
        //public void RemoveTagsFromNodeTest1()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    Tag.RemoveTagsFromNode(nodeId);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Group
        /////</summary>
        //[TestMethod()]
        //public void GroupTest()
        //{
        //    Tag target = new Tag(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Group = expected;
        //    actual = target.Group;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    Tag target = new Tag(); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.Id = expected;
        //    actual = target.Id;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for TagCaption
        /////</summary>
        //[TestMethod()]
        //public void TagCaptionTest()
        //{
        //    Tag target = new Tag(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.TagCaption = expected;
        //    actual = target.TagCaption;
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
