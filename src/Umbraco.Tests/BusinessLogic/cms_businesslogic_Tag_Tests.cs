using System;
using System.Linq;
using System.Runtime.CompilerServices;
using NUnit.Framework;
using Umbraco.Tests.TestHelpers;
using Umbraco.Core.Models.Rdbms;
using umbraco.cms.businesslogic.relation;
//using Umbraco.Core.Models;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.property;
using umbraco.interfaces;
using umbraco.cms.businesslogic.Tags;
using System.Text;
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
        public void Test_EnsureData()
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
        [Ignore] 
        public void Test_Property_Constructors()
        {
            // Tag class has two constructors but no one of them is communicating with back-end
            //public Tag() { }
            //public Tag(int id, string tag, string group, int nodeCount)
            Assert.True(true);  
        }

        [Test(Description = "Test 'public static int AddTag(string tag, string group)' method")]
        public void Test_Tag_AddTag()
        {
            int id = Tag.AddTag("Tag 21", "2");  
            var tagDto = getTestTagDto(id);

            Assert.That(tagDto, !Is.Null, "Failed to add Tag");
            Assert.That(id, Is.EqualTo(tagDto.Id), "Double-check - AddTag - failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static int GetTagId(string tag, string group)' method")]
        public void Test_Tag_GetTagId()
        {
            int id1 = addTag("Tag 21", "2");
            int id2 = Tag.GetTagId("Tag 21", "2");
            var tagDto = getTestTagDto(id2);

            Assert.That(tagDto, !Is.Null, "Failed to get Tag Id");
            Assert.That(id2, Is.EqualTo(id1), "Double-check - GetTagId - 1 failed");
            Assert.That(tagDto.Id , Is.EqualTo(id1), "Double-check - GetTagId - 2 - failed");

            EnsureAllTestRecordsAreDeleted();

        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(string group)' method")]
        public void Test_Tag_GetTags()
        {
            int tagsAddedCount = addTestTags(groupName: TEST_GROUP_NAME);
            int testCount = countTags(groupName: TEST_GROUP_NAME);
            
            var testGroup = Tag.GetTags(TEST_GROUP_NAME).ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "GetTags(string group) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags()' method")]
        public void Test_Tag_GetTags2()
        {
            int tagsAddedCount = addTestTags();
            int testCount = countTags();

            var testGroup = Tag.GetTags().ToArray();

            Assert.That(testGroup.Length, Is.EqualTo(testCount), "IEnumerable<Tag> GetTags() test failed");
            
            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void AddTagsToNode(int nodeId, string tags, string group)' method")]
        public void Test_Tag_AddTagsToNodes()
        {
            int testCount1 = countTags(_node1.Id, TEST_GROUP_NAME);

            var list = new List<string>();
            (new int[] { 1, 2, 3, 4, 5 }).ToList().ForEach(x => list.Add(string.Format("Tag #{0}", x)));
            string tagsSeparatedByComma = string.Join(",", list);

            Tag.AddTagsToNode(_node1.Id,  tagsSeparatedByComma, TEST_GROUP_NAME);

            int testCount2 = countTags(_node1.Id, TEST_GROUP_NAME);

            Assert.That(testCount2, Is.EqualTo(testCount1 + 5), "AddTagsToNode(int nodeId, string tags, string group) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId, string group)' method")]
        public void Test_Tag_GetTags3()
        {
            int tagsAddedCount = addTestTags (nodeId: _node1.Id,  groupName: TEST_GROUP_NAME);
            int testCount1 = countTags       (nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            int testCount2  = Tag.GetTags(_node1.Id, TEST_GROUP_NAME).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId, string group) test failed");

            EnsureAllTestRecordsAreDeleted();
        }


        [Test(Description = "Test 'public static IEnumerable<Tag> GetTags(int nodeId)' method")]
        public void Test_Tag_GetTags4()
        {
            int tagsAddedCount = addTestTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);
            int testCount1 = countTags(nodeId: _node1.Id);

            int testCount2 = Tag.GetTags(_node1.Id).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetTags(int nodeId) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void AssociateTagToNode(int nodeId, int tagId)' method")]
        public void Test_Tag_AssociateTagToNode()
        {
            int tagId = addTag("My Test Tag", TEST_GROUP_NAME);  
            int testCount1 = countTags(nodeId: _node1.Id);

            Tag.AssociateTagToNode(_node1.Id, tagId);

            int testCount2 = countTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1+1), "AssociateTagToNode test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void RemoveTag(int tagId)' method")]
        public void Test_Tag_RemoveTag()
        {
            int tagId = addTag("My Test Tag", TEST_GROUP_NAME);
            int testCount1 = countTags();

            Tag.RemoveTag(tagId);

            int testCount2 = countTags();

            Assert.That(testCount2, Is.EqualTo(testCount1 - 1), "RemoveTag test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId)' method")]
        public void Test_Tag_RemoveTagsFromNode()
        {
            int addedCount = addTestTags(nodeId: _node1.Id);   

            int testCount1 = countTags(nodeId: _node1.Id);

            Tag.RemoveTagsFromNode(_node1.Id);

            int testCount2 = countTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1 - addedCount), "RemoveTagsFromNode(int nodeId) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void RemoveTagsFromNode(int nodeId, string group)' method")]
        public void Test_Tag_RemoveTagsFromNode2()
        {
            int addedCount =  addTestTags(nodeId: _node1.Id);

            int testCount1 = countTags(nodeId: _node1.Id, groupName:TEST_GROUP_NAME );

            Tag.RemoveTagsFromNode(_node1.Id, TEST_GROUP_NAME);

            int testCount2 = countTags(nodeId: _node1.Id, groupName: TEST_GROUP_NAME);

            Assert.That(testCount2, Is.EqualTo(testCount1 - addedCount), "RemoveTagsFromNode(int nodeId, string group) test failed");

            EnsureAllTestRecordsAreDeleted();
        }
        
        [Test(Description = "Test 'public static void RemoveTagFromNode(int nodeId, string tag, string group)' method")]
        public void Test_Tag_RemoveTagFromNode()
        {
            string tagName = "My Test Tag";
            int tagId = addTagToNode(_node1.Id, tagName, TEST_GROUP_NAME);
            int testCount1 = countTags(nodeId:_node1.Id);

            Tag.RemoveTagFromNode(_node1.Id, tagName, TEST_GROUP_NAME);

            int testCount2 = countTags(nodeId: _node1.Id);

            Assert.That(testCount2, Is.EqualTo(testCount1 - 1), "RemoveTagFromNode test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static void MergeTagsToNode(int nodeId, string tags, string group)' method")]
        public void Test_Tag_MergeTagsToNode()
        {
            int testCount1 = countTags(_node1.Id, TEST_GROUP_NAME);

            var list = new List<string>();
            (new int[] { 1, 2, 3, 4, 5 }).ToList().ForEach(x => list.Add(string.Format("Tag #{0}", x)));
            string tagsSeparatedByComma = string.Join(",", list);

            Tag.MergeTagsToNode(_node1.Id, tagsSeparatedByComma, TEST_GROUP_NAME);

            int testCount2 = countTags(_node1.Id, TEST_GROUP_NAME);

            Assert.That(testCount2, Is.EqualTo(testCount1 + 5), "MergeTagsToNode(int nodeId, string tags, string group) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static IEnumerable<CMSNode> GetNodesWithTags(string tags)' method")]
        public void Test_Tag_GetNodesWithTags()
        {

            string tags = "Tag #1, Tag #2, Tag #3, Tag #4, Tag #5";
            addTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = countNodesWithTags(tags);

            int testCount2 = Tag.GetNodesWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetNodesWithTags(string tags) test failed");

            EnsureAllTestRecordsAreDeleted();
        }

        [Test(Description = "Test 'public static IEnumerable<Document> GetDocumentsWithTags(string tags)' method")]
        public void Test_Tag_GetDocumentsWithTags()
        {

            string tags = "Tag #1, Tag #2, Tag #3, Tag #4, Tag #5";
            addTagsToNode(_node1.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node2.Id, tags, TEST_GROUP_NAME);
            addTagsToNode(_node3.Id, tags, TEST_GROUP_NAME);

            int testCount1 = countDocumentsWithTags(tags);

            int testCount2 = Tag.GetDocumentsWithTags(tags).ToArray().Length;

            Assert.That(testCount2, Is.EqualTo(testCount1), "GetDocumentsWithTags test failed");

            EnsureAllTestRecordsAreDeleted();
        }
        #endregion

        #region EnsureData
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
                MakeNew_PersistsNewUmbracoNodeRow();

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
                string tagName = string.Format("Tag #{0}", i);
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

        private int countTags(int? nodeId = null, string groupName = "")
        {
            var sql = @"SELECT count(cmsTagRelationShip.tagid) AS nodeCount FROM cmsTags
                  INNER JOIN cmsTagRelationship ON cmsTagRelationShip.tagId = cmsTags.id";
            if (nodeId != null && !string.IsNullOrWhiteSpace(groupName))
                return independentDatabase.ExecuteScalar<int>(sql + "WHERE cmsTags.[group] = @0 AND cmsTagRelationship.nodeid = @1", nodeId, groupName);
            else if (nodeId != null)
                return independentDatabase.ExecuteScalar<int>(sql + "WHERE cmsTagRelationship.nodeid = @0", nodeId);
            else if (!string.IsNullOrWhiteSpace(groupName))
                return independentDatabase.ExecuteScalar<int>(sql + "WHERE cmsTags.[group] = @0", groupName);
            else
                return independentDatabase.ExecuteScalar<int>(sql);
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

        private int countDocumentsWithTags(string tags)
        {
            int count = 0;
            var docs = new List<DocumentDto>();
            string sql = @"SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
                            WHERE (cmsTags.tag IN ({0})) AND nodeObjectType=@nodeType";
            foreach (var id in independentDatabase.Query<int>(string.Format(sql, getSqlStringArray(tags),
                                                   new { nodeType = Document._objectType })))
            {
                Document cnode = new Document(id);

                if (cnode != null && cnode.Published) count++;
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

            initialized = false;
        }

        #endregion
    
    }
}

//
// first run
//
//1 passed, 15 failed, 1 skipped (see 'Task List'), took 14.66 seconds (NUnit 2.6.1).
//
// second run
//
// 3 passed, 13 failed, 1 skipped (see 'Task List'), took 14.37 seconds (NUnit 2.6.1).
//------ Test started: Assembly: Umbraco.Tests.dll ------
//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_AddTagsToNodes' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTags ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(432,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(143,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_AddTagsToNodes()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_AssociateTagToNode' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTagRelationship ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(434,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(189,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_AssociateTagToNode()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetDocumentsWithTags' failed: System.ArgumentException : Parameter '@nodeType' specified but none of the passed arguments have a property with this name (in 'SELECT DISTINCT cmsTagRelationShip.nodeid from cmsTagRelationShip
//                            INNER JOIN cmsTags ON cmsTagRelationShip.tagid = cmsTags.id 
//                            INNER JOIN umbracoNode ON cmsTagRelationShip.nodeId = umbracoNode.id
//                            WHERE (cmsTags.tag IN ('Tag #1','Tag #2','Tag #3','Tag #4','Tag #5')) AND nodeObjectType=@nodeType')
//    Persistence\PetaPoco.cs(361,0): at Umbraco.Core.Persistence.Database.<>c__DisplayClass1.<ProcessParams>b__0(Match m)
//    at System.Text.RegularExpressions.RegexReplacement.Replace(MatchEvaluator evaluator, Regex regex, String input, Int32 count, Int32 startat)
//    at System.Text.RegularExpressions.Regex.Replace(String input, MatchEvaluator evaluator, Int32 count, Int32 startat)
//    at System.Text.RegularExpressions.Regex.Replace(String input, MatchEvaluator evaluator)
//    Persistence\PetaPoco.cs(330,0): at Umbraco.Core.Persistence.Database.ProcessParams(String _sql, Object[] args_src, List`1 args_dest)
//    Persistence\PetaPoco.cs(476,0): at Umbraco.Core.Persistence.Database.CreateCommand(IDbConnection connection, String sql, Object[] args)
//    Persistence\PetaPoco.cs(761,0): at Umbraco.Core.Persistence.Database.<Query>d__7`1.MoveNext()
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(477,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countDocumentsWithTags(String tags)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(308,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetDocumentsWithTags()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTagId' failed: 
//  Double-check - GetTagId - 1 failed
//  Expected: 28
//  But was:  11
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(107,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTagId()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTags ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(436,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(118,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags2' failed: 
//  IEnumerable<Tag> GetTags() test failed
//  Expected: 5
//  But was:  23
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(135,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags2()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags3' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTags ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(432,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(162,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags3()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags4' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTagRelationship ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(434,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(176,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_GetTags4()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_MergeTagsToNode' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTags ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(432,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(266,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_MergeTagsToNode()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTag' failed: 
//  RemoveTag test failed
//  Expected: 9
//  But was:  10
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(210,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTag()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagFromNode' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTagRelationship ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(434,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(252,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagFromNode()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagsFromNode' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTagRelationship ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(434,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(220,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagsFromNode()

//Test 'Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagsFromNode2' failed: System.Data.SqlServerCe.SqlCeException : There was an error parsing the query. [ Token line number = 2,Token line offset = 95,Token in error = cmsTags ]
//    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//    at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//    at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//    at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar()
//    Persistence\PetaPocoCommandExtensions.cs(237,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9()
//    Persistence\FaultHandling\RetryPolicy.cs(172,0): at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func)
//    Persistence\PetaPocoCommandExtensions.cs(231,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(215,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy)
//    Persistence\PetaPocoCommandExtensions.cs(202,0): at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command)
//    Persistence\PetaPoco.cs(575,0): at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(432,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.countTags(Nullable`1 nodeId, String groupName)
//    BusinessLogic\cms_businesslogic_Tag_Tests.cs(236,0): at Umbraco.Tests.BusinessLogic.cms_businesslogic_Tag_Tests.Test_Tag_RemoveTagsFromNode2()

//2013-11-13 17:29:39,882 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:39,907 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,201 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,238 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,272 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,281 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,303 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,322 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560
//2013-11-13 17:29:40,341 Umbraco.Core.Persistence.UmbracoDatabase: [Thread 20]    at System.Data.SqlServerCe.SqlCeCommand.ProcessResults(Int32 hr)
//   at System.Data.SqlServerCe.SqlCeCommand.CompileQueryPlan()
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteCommand(CommandBehavior behavior, String method, ResultSetOptions options)
//   at System.Data.SqlServerCe.SqlCeCommand.ExecuteScalar()
//   at StackExchange.Profiling.Data.ProfiledDbCommand.ExecuteScalar() in c:\Code\github\SamSaffron\MiniProfiler\StackExchange.Profiling\Data\ProfiledDbCommand.cs:line 299
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.<>c__DisplayClassa.<ExecuteScalarWithRetry>b__9() in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 237
//   at Umbraco.Core.Persistence.FaultHandling.RetryPolicy.ExecuteAction[TResult](Func`1 func) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\FaultHandling\RetryPolicy.cs:line 172
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy cmdRetryPolicy, RetryPolicy conRetryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 231
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command, RetryPolicy retryPolicy) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 215
//   at Umbraco.Core.Persistence.PetaPocoCommandExtensions.ExecuteScalarWithRetry(IDbCommand command) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPocoCommandExtensions.cs:line 202
//   at Umbraco.Core.Persistence.Database.ExecuteScalar[T](String sql, Object[] args) in e:\Projects\Git\Umbraco\Umbraco-CMS-My-Fork\src\Umbraco.Core\Persistence\PetaPoco.cs:line 560

//3 passed, 13 failed, 1 skipped (see 'Task List'), took 14.37 seconds (NUnit 2.6.1).


