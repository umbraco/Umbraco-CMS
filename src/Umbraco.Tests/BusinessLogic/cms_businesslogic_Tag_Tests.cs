//#define ENABLE_TRACE

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using System.Collections.Generic;
using umbraco.cms.businesslogic.Tags;
using umbraco.cms.businesslogic.web;

namespace Umbraco.Tests.BusinessLogic
{
    // cmsTags: id, tag (chars), ParentId, [Group] (chars)
    //      V 
    // cmsTagRelationship: nodeId, tagId < cmdNode (NodeId)

    [TestFixture]
    public class cms_businesslogic_Tag_Tests : BaseORMTest
    {
        protected override void EnsureData() { Ensure_Tag_TestData(); }

        [Test(Description = "Test EnsureData()")]
        public void _1st_Test_EnsureTestData()
        {
            var dto = new Tag();

            Assert.IsTrue(initialized);

            Assert.That(_tag1, !Is.Null);
            Assert.That(_tag2, !Is.Null);
            Assert.That(_tag3, !Is.Null);
            Assert.That(_tag4, !Is.Null);
            Assert.That(_tag5, !Is.Null);

            Assert.That(_tagRel1, !Is.Null);
            Assert.That(_tagRel2, !Is.Null);
            Assert.That(_tagRel3, !Is.Null);
            Assert.That(_tagRel4, !Is.Null);
            Assert.That(_tagRel5, !Is.Null);

            Assert.That(_node1, !Is.Null);
            Assert.That(_node2, !Is.Null);
            Assert.That(_node3, !Is.Null);
            Assert.That(_node4, !Is.Null);
            Assert.That(_node5, !Is.Null);

            EnsureAll_Tag_TestRecordsAreDeleted();

            Assert.That(TRAL.GetDto<TagDto>(_tag1.Id), Is.Null);
            Assert.That(TRAL.GetDto<TagDto>(_tag2.Id), Is.Null);
            Assert.That(TRAL.GetDto<TagDto>(_tag3.Id), Is.Null);
            Assert.That(TRAL.GetDto<TagDto>(_tag4.Id), Is.Null);
            Assert.That(TRAL.GetDto<TagDto>(_tag5.Id), Is.Null);

            Assert.That(TRAL.Tag.GetTestTagRelationshipDto(_node1.Id, _tag1.Id), Is.Null);
            Assert.That(TRAL.Tag.GetTestTagRelationshipDto(_node1.Id, _tag2.Id), Is.Null);
            Assert.That(TRAL.Tag.GetTestTagRelationshipDto(_node1.Id, _tag3.Id), Is.Null);
            Assert.That(TRAL.Tag.GetTestTagRelationshipDto(_node2.Id, _tag4.Id), Is.Null);
            Assert.That(TRAL.Tag.GetTestTagRelationshipDto(_node3.Id, _tag5.Id), Is.Null);

            Assert.That(TRAL.GetDto<NodeDto>(_node1.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node2.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node3.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node4.Id), Is.Null);
            Assert.That(TRAL.GetDto<NodeDto>(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors")]
        public void _2nd_Test_Tag_Constructors()
        {
            // Tag class has two constructors but no one of them is communicating with back-end database
            //public Tag() { }
            //public Tag(int id, string tag, string group, int nodeCount)
            Assert.True(true);  
        }

        [Test(Description = "Test 'public static int TRAL.Tag.CreateTag(string tag, string group)' method")]
        public void Test_Tag_AddTag_Using_TagName_GroupName()
        {
            string tagName = string.Format("Tag {0}", uniqueLabel);
            int id = TRAL.Tag.CreateTag(tagName, TEST_GROUP_NAME);
            var tagDto = TRAL.GetDto<TagDto>(id);

            Assert.That(tagDto, !Is.Null, "Failed to add Tag");
            Assert.That(id, Is.EqualTo(tagDto.Id), "Double-check - AddTag - failed");
        }

        [Test(Description = "Test 'public static int GetTagId(string tag, string group)' method")]
        public void Test_Tag_GetTagId_By_TagName_GroupName()
        {
            string tagName = string.Format("Tag {0}", uniqueLabel);
            int id1 = TRAL.Tag.CreateTag(tagName, TEST_GROUP_NAME);
            int id2 = Tag.GetTagId(tagName, TEST_GROUP_NAME);
            var tagDto = TRAL.GetDto<TagDto>(id2);

            Assert.That(tagDto, !Is.Null, "Failed to get Tag Id");
            Assert.That(id2, Is.EqualTo(id1), "Double-check - GetTagId - 1 failed");
            Assert.That(tagDto.Id, Is.EqualTo(id1), "Double-check - GetTagId - 2 - failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(string group)' method")]
        public void Test_Tag_GetTags_By_GroupName()
        {
            int tagsAddedCount = TRAL.Tag.CreateTestTags(groupName: TEST_GROUP_NAME);
            int testCount = TRAL.Tag.CountTags(groupName: TEST_GROUP_NAME);
            
            var testGroup = Tag.GetTags(TEST_GROUP_NAME).ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "GetTags(string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags()' method")]
        public void Test_Tag_GetTags_All()
        {
            int tagsAddedCount = TRAL.Tag.CreateTestTags();
            int testCount = TRAL.Tag.CountTags();

            var testGroup = Tag.GetTags().ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "IEnumerable<Tag> GetTags() test failed");
        }

        [Test(Description = "Test 'public static void TRAL.Tag.AddTagsToNode(int nodeId, string tags, string group)' method")]
        public void Test_Tag_AddTagsToNode_Using_NodeId_TagsListString_GroupName()
        {
            int testCount1 = TRAL.Tag.CountTags(_node1.Id, TEST_GROUP_NAME);

            var list = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) list.Add(string.Format("Tag #{0}", uniqueLabel));
            string tagsSeparatedByComma = string.Join(",", list);

            TRAL.Tag.AddTagsToNode(_node1.Id,  tagsSeparatedByComma, TEST_GROUP_NAME);

            int testCount2 = TRAL.Tag.CountTags(_node1.Id, TEST_GROUP_NAME);

            Assert.That(testCount2, Is.EqualTo(testCount1 + list.Count), "TRAL.Tag.AddTagsToNode(int nodeId, string tags, string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId, string group)' method")]
        public void Test_Tag_GetTags_By_NodeId_GroupName()
        {
            int tagsAddedCount = TRAL.Tag.CreateTestTags(nodeId: _node1.Id,  groupName: TEST_GROUP_NAME);
            int testCount1 = TRAL.Tag.CountTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            int testCount2  = Tag.GetTags(_node1.Id, TEST_GROUP_NAME).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId, string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId)' method")]
        public void Test_Tag_GetTags_By_NodeId()
        {
            int tagsAddedCount = TRAL.Tag.CreateTestTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int testCount1 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            int testCount2 = Tag.GetTags(_node1.Id).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId) test failed");
        }

        [Test(Description = "Test 'public static void AssociateTagToNode(int nodeId, int tagId)' method")]
        public void Test_Tag_AssociateTagToNode_Using_NodeId_TagId()
        {
            int tagId = TRAL.Tag.CreateTag(uniqueLabel, TEST_GROUP_NAME);  
            int testCount1 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            Tag.AssociateTagToNode(_node1.Id, tagId);

            int testCount2 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1+1), "AssociateTagToNode test failed");
        }

        [Test(Description = "Test 'public static void RemoveTag(int tagId)' method")]
        public void Test_Tag_RemoveTag_By_NodeId()
        {
            int tagId = TRAL.Tag.CreateTag(uniqueLabel, TEST_GROUP_NAME);
            int testCount1 = TRAL.Tag.CountTags();

            Tag.RemoveTag(tagId);

            int testCount2 = TRAL.Tag.CountTags();

            Assert.That(testCount2, Is.EqualTo(testCount1 - 1), "RemoveTag test failed");
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId)' method")]
        public void Test_Tag_RemoveTagsFromNode_Using_NodeId()
        {
            int testCount1 = TRAL.Tag.CountTags(nodeId: _node1.Id);
            int addedCount = TRAL.Tag.CreateTestTags(nodeId: _node1.Id);   
            int testCount2 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            Tag.RemoveTagsFromNode(_node1.Id);

            int testCount3 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            Assert.That(testCount3, Is.EqualTo(testCount2 - addedCount - testCount1), "RemoveTagsFromNode(int nodeId) test failed");
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId, string group)' method")]
        public void Test_Tag_RemoveTagsFromNode_Using_NodeId_GroupName()
        {
            int testCount1 = TRAL.Tag.CountTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int addedCount = TRAL.Tag.CreateTestTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int testCount2 = TRAL.Tag.CountTags(nodeId: _node1.Id, groupName:TEST_GROUP_NAME );

            Tag.RemoveTagsFromNode(_node1.Id, TEST_GROUP_NAME);

            int testCount3 = TRAL.Tag.CountTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            Assert.That(testCount3, Is.EqualTo(testCount2 - addedCount - testCount1), "RemoveTagsFromNode(int nodeId, string group) test failed");
        }
        
        [Test(Description = "Test 'public static void RemoveTagFromNode(int nodeId, string tag, string group)' method")]
        public void Test_Tag_RemoveTagFromNode_Using_NodeId_TagName_GroupName()
        {
            string tagName = "My Test Tag";
            int tagId = TRAL.Tag.AddTagToNode(_node1.Id, tagName, TEST_GROUP_NAME);
            int testCount1 = TRAL.Tag.CountTags(nodeId:_node1.Id);

            Tag.RemoveTagFromNode(_node1.Id, tagName, TEST_GROUP_NAME);

            int testCount2 = TRAL.Tag.CountTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1 - 1), "RemoveTagFromNode test failed");
        }

        [Test(Description = "Test 'public static void MergeTagsToNode(int nodeId, string tags, string group)' method")]
        public void Test_Tag_MergeTagsToNode_Using_NodeId_TagsListString_GroupName()
        {
#if ENABLE_TRACE
            DateTime st = DateTime.Now;
            int initialTagsCount = Tag.GetTags(_node1.Id).ToArray().Length;
#endif

            var list = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) list.Add(string.Format("Tag #{0}", uniqueLabel));
            string tagsSeparatedByComma = string.Join(",", list);

            //GE 2011-11-01
            //When you have a new CSV list of tags (e.g. from control) and want to push those to the DB, the only way to do this
            //is delete all the existing tags and add the new ones. 
            //On a lot of tags, or a very full cmsTagRelationship table, this will perform too slowly
            Tag.MergeTagsToNode(_node1.Id, tagsSeparatedByComma, TEST_GROUP_NAME);

            int testCount = TRAL.Tag.CountTags(_node1.Id, TEST_GROUP_NAME);
            Assert.That(testCount, Is.EqualTo(list.Count), "MergeTagsToNode(int nodeId, string tags, string group) test failed");

#if ENABLE_TRACE
            int mergedTasgCount = list.Count;
            DateTime et = DateTime.Now;

            System.Console.WriteLine("Elapsed Time: {0}, testCount = {1}, initialTagsCount = {2}, mergedTasgCount = {3}", 
                    (et - st).TotalSeconds, testCount, initialTagsCount, mergedTasgCount);    
#endif

        }

        [Test(Description = "Test 'public static IEnumerable<CMSNode> GetNodesWithTags(string tags)' method")]
        public void Test_Tag_GetNodesWithTags()
        {
            var tagsList = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) tagsList.Add(uniqueLabel);
            string tags = string.Join(",", tagsList);
            TRAL.Tag.AddTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = TRAL.Tag.CountNodesWithTags(tags);

            int testCount2 = Tag.GetNodesWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetNodesWithTags(string tags) test failed");
        }

        // System.InvalidOperationException : The ServiceContext has not been set on the ApplicationContext
        [Test(Description = "Test 'public static IEnumerable<Document> GetDocumentsWithTags(string tags)' method")]
        [Obsolete("As it follows from the Tag.cs code GetDocumentsWithTags(...) method selecting only published Documents is depreciated/obsolete.")]
        public void Test_Tag_GetDocumentsWithTags_PublishedOnly()
        {
            var tagsList = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) tagsList.Add(uniqueLabel);
            string tags = string.Join(",", tagsList);

            TRAL.Tag.AddTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = TRAL.Tag.CountDocumentsWithTags(tags, publishedOnly: true);
            int testCount2 = Tag.GetDocumentsWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetDocumentsWithTags test failed");
        }

        // System.InvalidOperationException : The ServiceContext has not been set on the ApplicationContext
        [Test(Description = "Test 'public static IEnumerable<Document> GetAllDocumentsWithTags(string tags)' method")]
        public void Test_Tag_GetDocumentsWithTags_All()
        {
            var tagsList = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) tagsList.Add(uniqueLabel);
            string tags = string.Join(",", tagsList);
            TRAL.Tag.AddTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            TRAL.Tag.AddTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = TRAL.Tag.CountDocumentsWithTags(tags, publishedOnly: false);
            int testCount2 = Tag.GetAllDocumentsWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetAllDocumentsWithTags test failed");
        }
   
    }
}

// first run: 1 passed, 15 failed, 1 skipped (see 'Task List'), took 14.66 seconds (NUnit 2.6.1).
// second run: 3 passed, 13 failed, 1 skipped (see 'Task List'), took 14.37 seconds (NUnit 2.6.1).
// N-th run: 18 passed, 0 failed, 0 skipped, took 26.64 seconds (NUnit 2.6.1).
