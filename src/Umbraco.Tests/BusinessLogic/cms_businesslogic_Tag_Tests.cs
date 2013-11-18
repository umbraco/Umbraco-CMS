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
    public class cms_businesslogic_Tag_Tests : BaseDatabaseFactoryTestWithContext
    {
        #region Tests
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

            EnsureAllTestRecordsAreDeleted();

            Assert.That(getTestTagDto(_tag1.Id), Is.Null);
            Assert.That(getTestTagDto(_tag2.Id), Is.Null);
            Assert.That(getTestTagDto(_tag3.Id), Is.Null);
            Assert.That(getTestTagDto(_tag4.Id), Is.Null);
            Assert.That(getTestTagDto(_tag5.Id), Is.Null);

            Assert.That(getTestTagRelationshipDto(_node1.Id, _tag1.Id), Is.Null);
            Assert.That(getTestTagRelationshipDto(_node1.Id, _tag2.Id), Is.Null);
            Assert.That(getTestTagRelationshipDto(_node1.Id, _tag3.Id), Is.Null);
            Assert.That(getTestTagRelationshipDto(_node2.Id, _tag4.Id), Is.Null);
            Assert.That(getTestTagRelationshipDto(_node3.Id, _tag5.Id), Is.Null);

            Assert.That(getTestNodeDto(_node1.Id), Is.Null);
            Assert.That(getTestNodeDto(_node2.Id), Is.Null);
            Assert.That(getTestNodeDto(_node3.Id), Is.Null);
            Assert.That(getTestNodeDto(_node4.Id), Is.Null);
            Assert.That(getTestNodeDto(_node5.Id), Is.Null);
        }

        [Test(Description = "Constructors")]
        public void _2nd_Test_Tag_Constructors()
        {
            // Tag class has two constructors but no one of them is communicating with back-end database
            //public Tag() { }
            //public Tag(int id, string tag, string group, int nodeCount)
            Assert.True(true);  
        }

        [Test(Description = "Test 'public static int AddTag(string tag, string group)' method")]
        public void Test_Tag_AddTag_Using_TagName_GroupName()
        {
            string tagName = string.Format("Tag {0}", uniqueLabel);
            int id = addTag(tagName, TEST_GROUP_NAME);
            var tagDto = getTestTagDto(id);

            Assert.That(tagDto, !Is.Null, "Failed to add Tag");
            Assert.That(id, Is.EqualTo(tagDto.Id), "Double-check - AddTag - failed");
        }

        [Test(Description = "Test 'public static int GetTagId(string tag, string group)' method")]
        public void Test_Tag_GetTagId_By_TagName_GroupName()
        {
            string tagName = string.Format("Tag {0}", uniqueLabel); 
            int id1 = addTag(tagName, TEST_GROUP_NAME);
            int id2 = Tag.GetTagId(tagName, TEST_GROUP_NAME);
            var tagDto = getTestTagDto(id2);

            Assert.That(tagDto, !Is.Null, "Failed to get Tag Id");
            Assert.That(id2, Is.EqualTo(id1), "Double-check - GetTagId - 1 failed");
            Assert.That(tagDto.Id, Is.EqualTo(id1), "Double-check - GetTagId - 2 - failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(string group)' method")]
        public void Test_Tag_GetTags_By_GroupName()
        {
            int tagsAddedCount = addTestTags(groupName: TEST_GROUP_NAME);
            int testCount = countTags(groupName: TEST_GROUP_NAME);
            
            var testGroup = Tag.GetTags(TEST_GROUP_NAME).ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "GetTags(string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags()' method")]
        public void Test_Tag_GetTags_All()
        {
            int tagsAddedCount = addTestTags();
            int testCount = countTags();

            var testGroup = Tag.GetTags().ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "IEnumerable<Tag> GetTags() test failed");
        }

        [Test(Description = "Test 'public static void AddTagsToNode(int nodeId, string tags, string group)' method")]
        public void Test_Tag_AddTagsToNode_Using_NodeId_TagsListString_GroupName()
        {
            int testCount1 = countTags(_node1.Id, TEST_GROUP_NAME);

            var list = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) list.Add(string.Format("Tag #{0}", uniqueLabel));
            string tagsSeparatedByComma = string.Join(",", list);

            Tag.AddTagsToNode(_node1.Id,  tagsSeparatedByComma, TEST_GROUP_NAME);

            int testCount2 = countTags(_node1.Id, TEST_GROUP_NAME);

            Assert.That(testCount2, Is.EqualTo(testCount1 + list.Count), "AddTagsToNode(int nodeId, string tags, string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId, string group)' method")]
        public void Test_Tag_GetTags_By_NodeId_GroupName()
        {
            int tagsAddedCount = addTestTags (nodeId: _node1.Id,  groupName: TEST_GROUP_NAME);
            int testCount1 = countTags       (nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            int testCount2  = Tag.GetTags(_node1.Id, TEST_GROUP_NAME).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId, string group) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId)' method")]
        public void Test_Tag_GetTags_By_NodeId()
        {
            int tagsAddedCount = addTestTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int testCount1 = countTags(nodeId: _node1.Id);

            int testCount2 = Tag.GetTags(_node1.Id).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId) test failed");
        }

        [Test(Description = "Test 'public static void AssociateTagToNode(int nodeId, int tagId)' method")]
        public void Test_Tag_AssociateTagToNode_Using_NodeId_TagId()
        {
            int tagId = addTag(uniqueLabel, TEST_GROUP_NAME);  
            int testCount1 = countTags(nodeId: _node1.Id);

            Tag.AssociateTagToNode(_node1.Id, tagId);

            int testCount2 = countTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1+1), "AssociateTagToNode test failed");
        }

        [Test(Description = "Test 'public static void RemoveTag(int tagId)' method")]
        public void Test_Tag_RemoveTag_By_NodeId()
        {
            int tagId = addTag(uniqueLabel, TEST_GROUP_NAME);
            int testCount1 = countTags();

            Tag.RemoveTag(tagId);

            int testCount2 = countTags();

            Assert.That(testCount2, Is.EqualTo(testCount1 - 1), "RemoveTag test failed");
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId)' method")]
        public void Test_Tag_RemoveTagsFromNode_Using_NodeId()
        {
            int testCount1 = countTags(nodeId: _node1.Id);
            int addedCount = addTestTags(nodeId: _node1.Id);   
            int testCount2 = countTags(nodeId: _node1.Id);

            Tag.RemoveTagsFromNode(_node1.Id);

            int testCount3 = countTags(nodeId: _node1.Id);

            Assert.That(testCount3, Is.EqualTo(testCount2 - addedCount - testCount1), "RemoveTagsFromNode(int nodeId) test failed");
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId, string group)' method")]
        public void Test_Tag_RemoveTagsFromNode_Using_NodeId_GroupName()
        {
            int testCount1 = countTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int addedCount = addTestTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int testCount2 = countTags(nodeId: _node1.Id, groupName:TEST_GROUP_NAME );

            Tag.RemoveTagsFromNode(_node1.Id, TEST_GROUP_NAME);

            int testCount3 = countTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            Assert.That(testCount3, Is.EqualTo(testCount2 - addedCount - testCount1), "RemoveTagsFromNode(int nodeId, string group) test failed");
        }
        
        [Test(Description = "Test 'public static void RemoveTagFromNode(int nodeId, string tag, string group)' method")]
        public void Test_Tag_RemoveTagFromNode_Using_NodeId_TagName_GroupName()
        {
            string tagName = "My Test Tag";
            int tagId = addTagToNode(_node1.Id, tagName, TEST_GROUP_NAME);
            int testCount1 = countTags(nodeId:_node1.Id);

            Tag.RemoveTagFromNode(_node1.Id, tagName, TEST_GROUP_NAME);

            int testCount2 = countTags(nodeId: _node1.Id);

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

            int testCount = countTags(_node1.Id, TEST_GROUP_NAME);
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
            addTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = countNodesWithTags(tags);

            int testCount2 = Tag.GetNodesWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetNodesWithTags(string tags) test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Document> GetDocumentsWithTags(string tags)' method")]
        [Obsolete("As it follows from the Tag.cs code GetDocumentsWithTags(...) method selecting only published Documents is depreciated/obsolete.")]
        public void Test_Tag_GetDocumentsWithTags_PublishedOnly()
        {
            var tagsList = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) tagsList.Add(uniqueLabel);
            string tags = string.Join(",", tagsList);

            addTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = countDocumentsWithTags(tags, publishedOnly: true);
            int testCount2 = Tag.GetDocumentsWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetDocumentsWithTags test failed");
        }

        [Test(Description = "Test 'public static IEnumerable<Document> GetAllDocumentsWithTags(string tags)' method")]
        public void Test_Tag_GetDocumentsWithTags_All()
        {
            var tagsList = new List<string>();
            for (int i = 1; i <= TEST_ITEMS_MAX_COUNT; i++) tagsList.Add(uniqueLabel);
            string tags = string.Join(",", tagsList);
            addTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = countDocumentsWithTags(tags, publishedOnly: false);
            int testCount2 = Tag.GetAllDocumentsWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetAllDocumentsWithTags test failed");
        }
        #endregion

        #region EnsureData
        const int TEST_ITEMS_MAX_COUNT = 7;

        private TagDto _tag1;
        private TagDto _tag2;
        private TagDto _tag3;
        private TagDto _tag4;
        private TagDto _tag5;

        private TagRelationshipDto _tagRel1;
        private TagRelationshipDto _tagRel2;
        private TagRelationshipDto _tagRel3;
        private TagRelationshipDto _tagRel4;
        private TagRelationshipDto _tagRel5;

        [MethodImpl(MethodImplOptions.Synchronized)]
        protected override void EnsureData()
        {
            if (!initialized)
            {
                _node1 = TestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);

                _tag1 = InsertTestTag("Tag11", "1");
                _tag2 = InsertTestTag("Tag12", "1");
                _tag3 = InsertTestTag("Tag13", "1");
                _tag4 = InsertTestTag("Tag21", "2");
                _tag5 = InsertTestTag("Tag22", "2");

                _tagRel1 = InsertTestTagRelationship(_node1.Id, _tag1.Id);
                _tagRel2 = InsertTestTagRelationship(_node1.Id, _tag2.Id);
                _tagRel3 = InsertTestTagRelationship(_node1.Id, _tag3.Id);
                _tagRel4 = InsertTestTagRelationship(_node2.Id, _tag4.Id);
                _tagRel5 = InsertTestTagRelationship(_node3.Id, _tag5.Id);
            }

            initialized = true;
        }

        private TagDto InsertTestTag(string tag, string group)
        {
            independentDatabase.Execute("insert into [cmsTags] ([Tag], [Group]) values (@0, @1)", tag, group);
            int id = independentDatabase.ExecuteScalar<int>("select Max(id) from cmsTags");
            return getTestTagDto(id);
        }
        private TagRelationshipDto InsertTestTagRelationship(int nodeId, int tagId)
        {
            independentDatabase.Execute("insert into [cmsTagRelationship] ([nodeId], [tagId]) values (@0, @1)", nodeId, tagId);
            return getTestTagRelationshipDto(nodeId, tagId);
        }

        private TagDto getTestTagDto(int id)
        {
            return getPersistedTestDto<TagDto>(id);
        }
        private TagRelationshipDto getTestTagRelationshipDto(int nodeId, int tagId)
        {
            return independentDatabase.SingleOrDefault<TagRelationshipDto>(string.Format("where [nodeId] = @0 and [tagId] = @1"), nodeId, tagId);
        }

        private NodeDto getTestNodeDto(int id)
        {
            return getPersistedTestDto<NodeDto>(id);
        }

        private void delRel(TagRelationshipDto tagRel)
        {
            if (tagRel != null) independentDatabase.Execute("delete from [cmsTagRelationship] " +
                                                                      string.Format("where [nodeId] = @0 and [tagId] = @1"), tagRel.NodeId, tagRel.TagId);
        }
        private void delTag(TagDto tag)
        {
            if (tag != null) independentDatabase.Execute("delete from [cmsTags] " +
                                                                      string.Format("where [Id] = @0"), tag.Id);
        }

        const string TEST_GROUP_NAME = "my test group";
        private int addTestTags(int qty = 5, string groupName = TEST_GROUP_NAME, int? nodeId = null)
        {
            for (int i = 1; i <= qty; i++)
            {
                string tagName = string.Format("Tag #{0}", uniqueLabel);
                if (nodeId == null)
                    addTag(tagName, groupName);
                else
                    addTagToNode((int)nodeId, tagName, groupName);
            }
            return qty;
        }

        private int addTagToNode(int nodeId, string tag, string group)
        {
            int id = getTagId(tag, group);
            if (id == 0) id = addTag(tag, group);

            //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
            string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
            //sorry, gotta do this, @params not supported in join clause
            sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
            sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

            independentDatabase.Execute(sql);

            return id;
        }

        private int addTag(string tag, string group)
        {
            // independentDatabase.Insert returns System.Decimal - why?
            return (int)(decimal)independentDatabase.Insert(new TagDto() { Tag = tag, Group = group });
        }

        internal class TagDtoExt : TagDto
        {
            public int NodeCount { get; set; }
        }

        private IEnumerable<TagDto> convertSqlToTags(string sql, params object[] param)
        {
            //string testSql = sql;
            //int index = 0;
            //foreach (var p in param)
            //{
            //    testSql = testSql.Replace(string.Format("@{0}", index++), p.ToString());  
            //}

            //l("TEST SQL = '{0}'", testSql);

            foreach (var tagDto in independentDatabase.Query<TagDtoExt>(sql, param))
            {
                yield return tagDto;
                //new Tag(tagDto.Id, tagDto.Tag, tagDto.Group, tagDto.NodeCount);
            }
        }
        private int convertSqlToTagsCount(string sql, params object[] param)
        {
            return convertSqlToTags(sql, param).ToArray().Length;
        }

        private int countTags(int? nodeId = null, string groupName = "")
        {
            var groupBy = " GROUP BY cmsTags.id, cmsTags.tag, cmsTags.[group]";
            var sql = @"SELECT cmsTags.id, cmsTags.tag, cmsTags.[group], count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags " +
                        "INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id";
            if (nodeId != null && !string.IsNullOrWhiteSpace(groupName))
                return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0 AND cmsTagRelationship.nodeid = @1" + groupBy, groupName, nodeId);
            else if (nodeId != null)
                return convertSqlToTagsCount(sql +  " WHERE cmsTagRelationship.nodeid = @0" + groupBy, nodeId);
            else if (!string.IsNullOrWhiteSpace(groupName))
                return convertSqlToTagsCount(sql + " WHERE cmsTags.[group] = @0" + groupBy, groupName);
            else
                return convertSqlToTagsCount(sql.Replace("INNER JOIN", "LEFT JOIN") + groupBy);
        }


        private int getTagId(string tag, string group)
        {
            var tagDto = independentDatabase.FirstOrDefault<TagDto>("where tag=@0 AND [group]=@1", tag, group);
            if (tagDto == null) return 0;
            return tagDto.Id;
        }

        public void addTagsToNode(int nodeId, string tags, string group)
        {
            string[] allTags = tags.Split(",".ToCharArray());
            for (int i = 0; i < allTags.Length; i++)
            {
                //if not found we'll get zero and handle that onsave instead...
                int id = getTagId(allTags[i], group);
                if (id == 0)
                    id = addTag(allTags[i], group);

                //Perform a subselect insert into cmsTagRelationship using a left outer join to perform the if not exists check
                string sql = "insert into cmsTagRelationship (nodeId,tagId) select " + string.Format("{0}", nodeId) + ", " + string.Format("{0}", id) + " from cmsTags ";
                //sorry, gotta do this, @params not supported in join clause
                sql += "left outer join cmsTagRelationship on (cmsTags.id = cmsTagRelationship.TagId and cmsTagRelationship.nodeId = " + string.Format("{0}", nodeId) + ") ";
                sql += "where cmsTagRelationship.tagId is null and cmsTags.id = " + string.Format("{0}", id);

                independentDatabase.Execute(sql);
            }
        }

        private int countDocumentsWithTags(string tags, bool publishedOnly = false)
        {
            int count = 0;
            var docs = new List<DocumentDto>();
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";
            foreach (var id in independentDatabase.Query<int>(string.Format(sql, getSqlStringArray(tags)),
                                                   new { nodeType = Document._objectType }))
            {
                Document cnode = new Document(id);

                if (cnode != null && (!publishedOnly || cnode.Published)) count++;
            }

            return count;
        }

        private int countNodesWithTags(string tags)
        {
            int count = 0;

            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id WHERE (cmsTags.tag IN (" + getSqlStringArray(tags) + "))";
            foreach (var id in independentDatabase.Query<int>(sql)) count++;
            return count;
        }


        private void EnsureAllTestRecordsAreDeleted()
        {
            delRel(_tagRel1);
            delRel(_tagRel2);
            delRel(_tagRel3);
            delRel(_tagRel4);
            delRel(_tagRel5);

            delTag(_tag1);
            delTag(_tag2);
            delTag(_tag3);
            delTag(_tag4);
            delTag(_tag5);

            // nodes deletion recursively deletes all relations...
            if (_node1 != null) _node1.delete();
            if (_node2 != null) _node2.delete();
            if (_node3 != null) _node3.delete();
            if (_node4 != null) _node4.delete();
            if (_node5 != null) _node5.delete();

            //deleteContent();

            initialized = false;
        }

        #endregion
    
    }
}

// first run: 1 passed, 15 failed, 1 skipped (see 'Task List'), took 14.66 seconds (NUnit 2.6.1).
// second run: 3 passed, 13 failed, 1 skipped (see 'Task List'), took 14.37 seconds (NUnit 2.6.1).
// N-th run: 18 passed, 0 failed, 0 skipped, took 26.64 seconds (NUnit 2.6.1).
