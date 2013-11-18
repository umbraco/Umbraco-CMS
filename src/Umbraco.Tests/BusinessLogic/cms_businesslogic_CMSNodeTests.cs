using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Xml;
using NUnit.Framework;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.packager;
using umbraco.cms.businesslogic.web;
using umbraco.cms.presentation;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Core.Persistence;
using umbraco.NodeFactory;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web;
using ContentType = Umbraco.Core.Models.ContentType;

namespace Umbraco.Tests.BusinessLogic
{
    [TestFixture]
    public class cms_businesslogic_CMSNodeTests : BaseDatabaseFactoryTest
    {
        private const int TextStringDataTypeId = -88;
        private const string RootObjectTypeId = "EA7D8624-4CFE-4578-A871-24AA946BF34D";
        private const string MemberObjectTypeId = "9B5416FB-E72F-45A9-A07B-5A9A2709CE43";
        private const string RecycleBin1ObjectTypeId = "CF3D8E34-1C1C-41E9-AE56-878B57B32113";
        private const string MediaTypeObjectTypeId = "4EA4382B-2F5A-4C2B-9587-AE9B3CF3602E";
        private const string DocumentTypeObjectTypeId = "A2CB7800-F571-4787-9638-BC48539A0EFB";
        private const string RecycleBin2ObjectTypeId = "01BB7FF2-24DC-4C0C-95A2-C24EF72BBAC8";
        private const string DataTypeObjectTypeId = "30a2a501-1978-4ddb-a57b-f7efed43ba3c";
        private readonly Guid textStringDataTypeUniqueId = new Guid("0CC0EBA1-9960-42C9-BF9B-60E150B429AE");
        private readonly Guid dataTypeObjectTypeId = new Guid(DataTypeObjectTypeId);
        private bool initialized;
        private ContentType testContentType1;
        private ContentType testContentType2;
        private ContentType testContentType3;
        private ContentType testContentType4;
        private ContentType testContentType5;
        private ContentType[] contentTypes;
        private UmbracoContext context;
        private UmbracoDatabase database;

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        [Test]
        public void Ctor_Int_PopulatesNode_CallsSetupNode()
        {
            var node = new TestCMSNode(TextStringDataTypeId);
            AssertTextStringDataTypeNode(node);
            Assert.IsTrue(node.SetupNodeCalled);
        }

        [Test]
        public void Ctor_Int_NoSetup_AssignsIdOnly()
        {
            var node = new TestCMSNode(TextStringDataTypeId, true);
            AssertTextStringDataTypeIdOnly(node);
            Assert.IsFalse(node.SetupNodeCalled);
        }

        [Test]
        public void Ctor_Int_NoSetup_False_PopulatesNode_CallsSetupNode()
        {
            var node = new TestCMSNode(TextStringDataTypeId, false);
            AssertTextStringDataTypeNode(node);
            Assert.IsTrue(node.SetupNodeCalled);
        }

        [Test]
        public void Ctor_Guid_PopulatesNode_CallsSetupNode()
        {
            var node = new TestCMSNode(textStringDataTypeUniqueId);
            AssertTextStringDataTypeNode(node);
            Assert.IsTrue(node.SetupNodeCalled);
        }

        [Test]
        public void Ctor_Guid_NoSetup_AssignsIdOnly()
        {
            var node = new TestCMSNode(textStringDataTypeUniqueId, true);
            AssertTextStringDataTypeIdOnly(node);
            Assert.IsFalse(node.SetupNodeCalled);
        }

        [Test]
        public void Ctor_Guid_NoSetup_False_PopulatesNode_CallsSetupNode()
        {
            var node = new TestCMSNode(textStringDataTypeUniqueId, false);
            AssertTextStringDataTypeNode(node);
            Assert.IsTrue(node.SetupNodeCalled);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException), ExpectedMessage = "No node exists with id '999'")]
        public void Ctor_InvalidId_ThrowsArgumentException()
        {
            new CMSNode(999);
        }

        [Test]
        [TestCase(RootObjectTypeId, 1, "Root")]
        [TestCase(MemberObjectTypeId, 1, "Member")]
        [TestCase(RecycleBin1ObjectTypeId, 1, "Recycle bin 1")]
        [TestCase(MediaTypeObjectTypeId, 3, "MediaType")]
        [TestCase(DocumentTypeObjectTypeId, 5, "DocumentType")]
        [TestCase(RecycleBin2ObjectTypeId, 1, "Recycle Bin 2")]
        [TestCase(DataTypeObjectTypeId, 24, "DataType")]
        public void CountByObjectType_ReturnsCount(string objectTypeId, int expected, string description)
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountByObjectType(new Guid(objectTypeId));
            Assert.AreEqual(expected, actual, "There should be {0} of nodeObjectType {1}", expected, description);
        }

        [Test]
        [TestCase(RootObjectTypeId, -1, 1, "Root is its own child")]
        [TestCase(DocumentTypeObjectTypeId, -1, 2, "Two root document types")]
        public void CountLeafNodes_Roots_ReturnsCount(string objectTypeId, int parentId, int expected, string description)
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountLeafNodes(parentId, new Guid(objectTypeId));
            Assert.AreEqual(expected, actual, description);
        }

        [Test]
        public void CountLeafNodes_DocumentTypeChild_ReturnsCount()
        {
            EnsureTestDocumentTypes();
            var actual = CMSNode.CountLeafNodes(testContentType1.Id, new Guid(DocumentTypeObjectTypeId));
            Assert.AreEqual(2, actual);
        }

        [Test]
        public void CountSubs_ReturnsCountOfAncestorsOrSomething()
        {
            const int expectedRootCount = 36;
            const int expectedAncestorOfThreeCount = 4;
            const int expectedLeafCount = 1;

            EnsureTestDocumentTypes();
            Assert.AreEqual(expectedRootCount, CMSNode.CountSubs(-1));
            Assert.AreEqual(expectedAncestorOfThreeCount, CMSNode.CountSubs(testContentType1.Id));
            Assert.AreEqual(expectedLeafCount, CMSNode.CountSubs(testContentType2.Id));
        }

        [Test]
        public void Delete_DeletesRowFromUmbracoNode()
        {
            EnsureTestDocumentTypes();
            var originalCount = database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode");
            try
            {
                var node = new CMSNode(testContentType2.Id);
                database.Execute("DELETE cmsContentType WHERE nodeId = @NodeId", new { NodeId = node.Id });
                node.delete();
                Assert.AreEqual(0, database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id = @id", new { id = node.Id })); ;
                var newCount = database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode");
                Assert.AreEqual(originalCount - 1, newCount);
            }
            finally
            {
                ResetTestDocumentTypes();
            }
        }

        [Test]
        public void ChildCount_ReturnsCountOfChildren()
        {
            EnsureTestDocumentTypes();
            var contentTypeNode = new CMSNode(testContentType1.Id);
            var rootNode = new CMSNode(-1);
            Assert.AreEqual(2, contentTypeNode.ChildCount);
            Assert.AreEqual(33, rootNode.ChildCount);
        }

        [Test]
        public void Children_ReturnsAllChildren()
        {
            EnsureTestDocumentTypes();
            Assert.AreEqual(2, database.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE ParentId = @id", new { id = testContentType1.Id }));
            var parentNode = new CMSNode(testContentType1.Id);
            var children = parentNode.Children;
            Assert.AreEqual(2, children.Count());
            Assert.AreEqual(testContentType3.Id, children[0].Id);
            Assert.AreEqual(testContentType4.Id, children[1].Id);
            AssertNonEmptyNode((CMSNode)children[0]);
            AssertNonEmptyNode((CMSNode)children[1]);
            for (var i = 0; i < children.Count() - 1; i++)
                Assert.LessOrEqual(((CMSNode)children[i]).sortOrder, ((CMSNode)children[i + 1]).sortOrder);
        }

        [Test]
        public void Children_OfRoot_ReturnsItselfOnlySinceNoSimilarObjectTypes()
        {
            CreateContext();
            var root = new CMSNode(-1);
            var actual = root.Children;
            Assert.AreEqual(1, actual.Count());
            Assert.AreEqual(-1, actual[0].Id);
        }

        [Test]
        public void ChildrenOfAllObjectTypes_OfRoot_ReturnsAllDirectChildren()
        {
            EnsureTestDocumentTypes();
            var root = new CMSNode(-1);
            var actual = root.ChildrenOfAllObjectTypes;
            Assert.AreEqual(33, actual.Count());
            Assert.AreEqual(-1, actual[0].Id);
            for (var i = 0; i < actual.Count() - 1; i++)
                Assert.LessOrEqual(((CMSNode)actual[i]).sortOrder, ((CMSNode)actual[i + 1]).sortOrder);
            foreach (var node in actual)
                AssertNonEmptyNode((CMSNode)node);
        }

        [Test]
        public void HasChildren_WhenParent_ReturnsTrue()
        {
            EnsureTestDocumentTypes();
            var parent = new CMSNode(testContentType1);
            Assert.IsTrue(parent.HasChildren);
        }

        [Test]
        public void HasChildren_WhenLeaf_ReturnsFalse()
        {
            EnsureTestDocumentTypes();
            var leaf = new CMSNode(testContentType3);
            Assert.IsFalse(leaf.HasChildren);
        }

        [Test]
        public void HasChildren_WhenSet_ReturnsValue()
        {
            EnsureTestDocumentTypes();
            var leaf = new CMSNode(testContentType3);
            leaf.HasChildren = true;
            Assert.IsTrue(leaf.HasChildren);
        }

        [Test]
        public void IsTrashed_New_ReturnsFalse()
        {
            EnsureTestDocumentTypes();
            var node = new CMSNode(testContentType3);
            Assert.IsFalse(node.IsTrashed);
        }

        [Test]
        public void IsTrashed_WhenSet_Persists()
        {
            EnsureTestDocumentTypes();
            var node = new CMSNode(testContentType3);
            node.IsTrashed = true;
            var actual = database.ExecuteScalar<bool>(
                "SELECT trashed FROM umbracoNode WHERE id = @id",
                new { id = testContentType3.Id });
            Assert.IsTrue(actual);
        }

        [Test]
        public void IsTrashed_WhenTrashed_ReturnsTrue()
        {
            EnsureTestDocumentTypes();
            database.Execute(
                "UPDATE umbracoNode SET trashed = 1 WHERE id = @id",
                new { id = testContentType3.Id });
            var node = new CMSNode(testContentType3);
            Assert.IsTrue(node.IsTrashed);
        }

        [Test]
        public void getAllUniqueNodeIdsFromObjectType_ReturnsIds()
        {
            EnsureTestDocumentTypes();
            var ids = CMSNode.getAllUniqueNodeIdsFromObjectType(new Guid(DocumentTypeObjectTypeId));
            var expectedIds = contentTypes.Select(c => c.Id).OrderBy(id => id);
            var actualIds = ids.OrderBy(id => id);
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds));
        }

        [Test]
        public void getAllUniquesFromObjectType_ReturnsGuids()
        {
            EnsureTestDocumentTypes();
            var guids = CMSNode.getAllUniquesFromObjectType(new Guid(DocumentTypeObjectTypeId));
            var expectedIds = database.Fetch<Guid>(
                "SELECT uniqueId FROM umbracoNode WHERE nodeObjectType = @ObjectTypeId",
                new { ObjectTypeId = DocumentTypeObjectTypeId })
                .OrderBy(id => id);
            var actualIds = guids.OrderBy(id => id);
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds));
        }

        [Test]
        public void GetDescendants_Root_ReturnsNone()
        {
            var root = new CMSNode(-1);
            var rootDescs = root.GetDescendants().Cast<CMSNode>();
            Assert.AreEqual(0, rootDescs.Count());
        }

        [Test]
        public void GetDescendants_ReturnsAllDescendants()
        {
            EnsureTestDocumentTypes();
            var parent = new CMSNode(testContentType1);
            var parentDescs = parent.GetDescendants().Cast<CMSNode>();
            Assert.AreEqual(3, parentDescs.Count());
            foreach (var desc in parentDescs)
                AssertNonEmptyNode(desc);
        }

        [Test]
        public void MakeNew_PersistsNewUmbracoNodeRow()
        {
            // Testing Document._objectType, since it has exclusive use of GetNewDocumentSortOrder. :)

            int id = 0;
            try
            {
                CreateContext();
                TestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                TestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                var node3 = TestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                var totalDocuments = database.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @ObjectTypeId",
                    new { ObjectTypeId = Document._objectType });
                Assert.AreEqual(3, totalDocuments);
                id = node3.Id;
                var loadedNode = new CMSNode(id);
                AssertNonEmptyNode(loadedNode);
                Assert.AreEqual(2, loadedNode.sortOrder);
            }
            finally
            {
                DeleteContent();
            }
        }

        [Test]
        public void SavePreviewXml_WhenNew_PersistsXml()
        {
            EnsureTestDocumentTypes();
            try
            {
                var node = Document.MakeNew("Test content", new DocumentType(testContentType1), new User(0), -1);
                var asTestNode = new TestCMSNode(node.Id);
                //var xmlNode = node.ToPreviewXml(new XmlDocument());
                asTestNode.ExecuteSavePreviewXml(new XmlDocument(), node.Version);
                var persistedXml = database.ExecuteScalar<string>("SELECT xml FROM cmsPreviewXml WHERE nodeId = @id",
                    new { id = node.Id });

                Console.WriteLine(persistedXml);

                Assert.IsNotNullOrEmpty(persistedXml);
            }
            finally
            {
                DeleteContent();
            }
        }

        [Test]
        public void SavePreviewXml_WhenExists_UpdatesXml()
        {
            EnsureTestDocumentTypes();
            try
            {
                var node = Document.MakeNew("Test content", new DocumentType(testContentType1), new User(0), -1);
                node.ToPreviewXml(new XmlDocument());
                var asTestNode = new TestCMSNode(node.Id);
                asTestNode.Text = "Updated test content";
                asTestNode.ExecuteSavePreviewXml(new XmlDocument(), node.Version);
                var persistedXml = database.ExecuteScalar<string>("SELECT xml FROM cmsPreviewXml WHERE nodeId = @id",
                    new { id = node.Id });

                Console.WriteLine(persistedXml);

                Assert.IsNotNullOrEmpty(persistedXml);
                Assert.IsTrue(persistedXml.Contains(asTestNode.Text));
            }
            finally
            {
                DeleteContent();
            }
        }

        [Test]
        public void GetPreviewXml_ReturnsPersistedXml()
        {
            EnsureTestDocumentTypes();
            try
            {
                var node = Document.MakeNew("Test content", new DocumentType(testContentType1), new User(0), -1);
                node.ToPreviewXml(new XmlDocument());
                var asTestNode = new TestCMSNode(node.Id);

                var expected = database.ExecuteScalar<string>("SELECT xml FROM cmsPreviewXml WHERE nodeId = @id",
                    new { id = node.Id });

                var result = asTestNode.ExecuteGetPreviewXml(new XmlDocument(), node.Version);
                var actual = result.OuterXml;

                Assert.AreEqual(expected, actual);
            }
            finally
            {
                DeleteContent();
            }
        }

        [Test]
        public void GetNodesForPreview_NotChildrenOnly_ReturnsItself()
        {
            EnsureTestDocumentTypes();
            var nodes = CreateContent();
            try
            {
                // oh yes, GetNodesForPreview in base returns both draft and not draft as draft
                var expectedNodes = new[] { nodes[0], nodes[0] };
                var result = new CMSNode(nodes[0].Id).GetNodesForPreview(false);
                Assert.AreEqual(expectedNodes.Length, result.Count);
                for (var i = 0; i < expectedNodes.Length; i++)
                    AssertXmlPreviewNode(expectedNodes, result, i);
            }
            finally
            {
                for (var i = nodes.Count - 1; i >= 0; i--)
                    nodes[i].delete();
                DeleteContent();
            }
        }

        [Test]
        public void GetNodesForPreview_ChildrenOnly_ReturnsChildren()
        {
            EnsureTestDocumentTypes();
            var nodes = CreateContent();
            try
            {
                var expectedNodes = new[] { nodes[2], nodes[2], nodes[1], nodes[1], nodes[3], nodes[3] };
                var result = new CMSNode(nodes[0].Id).GetNodesForPreview(true);
                Assert.AreEqual(expectedNodes.Length, result.Count);
                for (var i = 0; i < expectedNodes.Length; i++)
                    AssertXmlPreviewNode(expectedNodes, result, i);
            }
            finally
            {
                for (var i = nodes.Count - 1; i >= 0; i--)
                    nodes[i].delete();
                DeleteContent();
            }
        }

        [Test]
        public void getUniquesFromObjectTypeAndFirstLetter_ReturnsUniqueIdsWhereTextStartsWith()
        {
            EnsureTestDocumentTypes();
            var nodes = CreateContent();
            try
            {
                var expected = new[] { nodes[1].Id, nodes[2].Id };
                var result = TestCMSNode.ExecuteGetUniquesFromObjectTypeAndFirstLetter(Document._objectType, 'B');
                Assert.IsTrue(expected.SequenceEqual(result));
            }
            finally
            {
                for (var i = nodes.Count - 1; i >= 0; i--)
                    nodes[i].delete();
                DeleteContent();
            }
        }

        [Test]
        [TestCase(-1, true)]
        [TestCase(1044, true)]
        [TestCase(-21, true)]
        [TestCase(1031, true)]
        [TestCase(0, false)]
        [TestCase(999, false)]
        public void IsNode_Int_ReturnsBoolIndicatingExistence(int id, bool expected)
        {
            Assert.AreEqual(expected, CMSNode.IsNode(id));
        }

        [Test]
        [TestCase("916724A5-173D-4619-B97E-B9DE133DD6F5", true)]
        [TestCase("D59BE02F-1DF9-4228-AA1E-01917D806CDA", true)]
        [TestCase("BF7C7CBC-952F-4518-97A2-69E9C7B33842", true)]
        [TestCase("F38BD2D7-65D0-48E6-95DC-87CE06EC2D3D", true)]
        [TestCase("B4172458-572A-4757-9810-17D204C96F61", false)]
        [TestCase("CF096FFC-794F-49A7-A8DC-FAA5CC7E334F", false)]
        public void IsNode_Guid_ReturnsBoolIndicatingExistence(string id, bool expected)
        {
            Assert.AreEqual(expected, CMSNode.IsNode(new Guid(id)));
        }

        [Test]
        public void CreateDateTime_Set_Persists()
        {
            var newDate = new DateTime(2013, 1, 1);
            EnsureTestDocumentTypes();
            Setter_Persists(
                testContentType1.Id,
                n => n.CreateDateTime = newDate,
                n => n.CreateDateTime,
                newDate,
                "createDate"
            );
        }

        [Test]
        public void Level_Set_Persists()
        {
            // this is silly :)
            var newLevel = 5;
            EnsureTestDocumentTypes();
            Setter_Persists(
                testContentType1.Id,
                n => n.Level = newLevel,
                n => n.Level,
                newLevel,
                "level"
            );
        }

        [Test]
        public void Parent_Set_Persists()
        {
            EnsureTestDocumentTypes();
            var newParentNode = new CMSNode(testContentType2.Id);
            Setter_Persists(
                testContentType4.Id,
                n => n.Parent = newParentNode,
                n => n.Parent.Id,
                newParentNode.Id,
                "ParentId"
                );
        }

        [Test]
        public void Path_Set_Persists()
        {
            // this is sillier :)
            EnsureTestDocumentTypes();
            var newPath = "abc";
            Setter_Persists(
                testContentType1.Id,
                n => n.Path = newPath,
                n => n.Path,
                newPath,
                "Path");
        }

        [Test]
        public void SortOrder_Set_Persists()
        {
            EnsureTestDocumentTypes();
            var newSortOrder = 99;
            Setter_Persists(
                testContentType1.Id,
                n => n.sortOrder = newSortOrder,
                n => n.sortOrder,
                newSortOrder,
                "sortOrder");
        }

        [Test]
        public void Text_Set_Persists()
        {
            EnsureTestDocumentTypes();
            var newText = "\tA new text  ";
            var expectedText = "A new text";
            Setter_Persists(
                testContentType1.Id,
                n => n.Text = newText,
                n => n.Text,
                expectedText,
                "text");
        }

        private void Setter_Persists<T>(int nodeId, Action<CMSNode> setter, Func<CMSNode, T> getter, T expected, string field)
        {
            var node = new CMSNode(nodeId);
            setter(node);
            var persisted = database.ExecuteScalar<T>(
                String.Format("SELECT {0} FROM umbracoNode WHERE id = @id", field)
                , new { id = nodeId });
            Assert.AreEqual(expected, persisted);
            Assert.AreEqual(expected, getter(node));
            ResetTestDocumentTypes();
        }

        [Test]
        public void Move_MovesNodeAndUpdatesSortOrder()
        {
            Assert.Inconclusive("Ain't bothering with this test, weird errors deep in the Move call tree");

            //EnsureTestDocumentTypes();
            //var nodes = CreateContent();
            //try
            //{
            //    var node = new CMSNode(nodes[3].Id);
            //    node.Move(nodes[0].Id);
            //    Assert.AreEqual(2, node.sortOrder);
            //    Assert.AreEqual(nodes[0].Id, node.ParentId);
            //}
            //finally
            //{
            //    for (var i = nodes.Count - 1; i >= 0; i--)
            //        nodes[i].delete();
            //    DeleteContent();
            //}
        }

        public void TopMostNodeIds_ReturnsUniqueIdsOfRootNodes()
        {
            // Lacks testing of sorting

            // Root
            Assert.IsTrue(
                new[] { new Guid("916724A5-173D-4619-B97E-B9DE133DD6F5") }
                .SequenceEqual(CMSNode.TopMostNodeIds(new Guid(RootObjectTypeId)))
                );

            // Content
            EnsureTestDocumentTypes();
            var nodes = CreateContent();
            try
            {
                Assert.IsTrue(
                    new[] { nodes[0].UniqueId }
                    .SequenceEqual(CMSNode.TopMostNodeIds(Document._objectType))
                    );
            }
            finally
            {
                for (var i = nodes.Count - 1; i >= 0; i--)
                    nodes[i].delete();
                DeleteContent();
            }
        }

        private static void AssertXmlPreviewNode(Document[] expectedNodes, List<CMSPreviewNode> result, int index)
        {
            Assert.AreEqual(expectedNodes[index].Id, result[index].NodeId);
            Assert.IsFalse(result[index].IsDraft);
            Assert.AreEqual(expectedNodes[index].Level, result[index].Level);
            Assert.AreEqual(expectedNodes[index].ParentId, result[index].ParentId);
            Assert.AreEqual(expectedNodes[index].sortOrder, result[index].SortOrder);
            Assert.AreEqual(expectedNodes[index].UniqueId, result[index].Version);
            Assert.IsNotNullOrEmpty(result[index].Xml);
            // can't compare with ToXml 'cause of date variance
        }

        private List<Document> CreateContent()
        {
            var documentType = new DocumentType(testContentType1);
            var user = new User(0);
            var nodes = new List<Document>
            {
                Document.MakeNew("Test content 1", documentType, user, -1),
            };
            nodes.Add(Document.MakeNew("B Test content 1.1", documentType, user, nodes[0].Id));
            nodes.Add(Document.MakeNew("B Test content 1.2", documentType, user, nodes[0].Id));
            nodes.Add(Document.MakeNew("Test content 1.2.1", documentType, user, nodes[2].Id));
            nodes[1].sortOrder = 3;
            nodes.ForEach(n =>
            {
                n.Publish(user);
            });
            return nodes;
        }

        private void EnsureTestDocumentTypes()
        {
            CreateContext();
            if (initialized) return;
            testContentType1 = new ContentType(-1) { Alias = "Test1", Name = "Test 1" };
            testContentType2 = new ContentType(-1) { Alias = "Test2", Name = "Test 2" };
            var contentTypeService = context.Application.Services.ContentTypeService;
            contentTypeService.Save(testContentType1);
            contentTypeService.Save(testContentType2);
            testContentType3 = new ContentType(testContentType1.Id) { Alias = "Test1.1", Name = "Test 1.1", SortOrder = 1 };
            testContentType4 = new ContentType(testContentType1.Id) { Alias = "Test1.2", Name = "Test 1.2", SortOrder = 2 };
            contentTypeService.Save(testContentType3);
            contentTypeService.Save(testContentType4);
            testContentType5 = new ContentType(testContentType4.Id) { Alias = "Test1.2.1", Name = "Test 1.2.1" };
            contentTypeService.Save(testContentType5);
            contentTypes = new[] { testContentType5, testContentType4, testContentType3, testContentType2, testContentType1 };
            initialized = true;
        }

        private void ResetTestDocumentTypes()
        {
            foreach (var node in contentTypes)
                DeleteContentType(node.Id);
            initialized = false;
        }

        private void DeleteContent()
        {
            database.Execute("DELETE cmsPreviewXml");
            database.Execute("DELETE cmsContentVersion");
            database.Execute("DELETE cmsDocument");
            database.Execute("DELETE cmsContent");
            database.Delete<NodeDto>("WHERE nodeObjectType = @ObjectTypeId", new { ObjectTypeId = Document._objectType });
        }

        private void DeleteContentType(int id)
        {
            database.Execute("DELETE cmsContentType WHERE nodeId = @NodeId", new { NodeId = id });
            database.Execute("DELETE umbracoNode WHERE id = @Id", new { Id = id });
        }

        private void CreateContext()
        {
            context = GetUmbracoContext("http://localhost", 0);
            database = context.Application.DatabaseContext.Database;
        }

        private void AssertTextStringDataTypeNode(CMSNode node)
        {
            Assert.AreEqual(TextStringDataTypeId, node.Id);
            Assert.AreEqual(textStringDataTypeUniqueId, node.UniqueId);
            Assert.AreEqual(dataTypeObjectTypeId, node.nodeObjectType);
            Assert.AreEqual(1, node.Level);
            Assert.AreEqual("-1,-88", node.Path);
            Assert.AreEqual(-1, node.ParentId);
            Assert.AreEqual("Textstring", node.Text);
            Assert.AreEqual(32, node.sortOrder);
            Assert.AreEqual(0, node.User.Id);
            Assert.AreEqual(DateTime.Today, node.CreateDateTime.Date);
            Assert.IsFalse(node.IsTrashed);
        }

        private void AssertNonEmptyNode(CMSNode node)
        {
            Assert.AreNotEqual(0, node.Id);
            Assert.AreNotEqual(Guid.Empty, node.UniqueId);
            Assert.AreNotEqual(Guid.Empty, node.nodeObjectType);
            Assert.IsNotNullOrEmpty(node.Path);
            Assert.AreNotEqual(0, node.ParentId);
            Assert.IsNotNullOrEmpty(node.Text);
            Assert.AreEqual(DateTime.Today, node.CreateDateTime.Date);
            Assert.IsFalse(node.IsTrashed);
        }

        private static void AssertTextStringDataTypeIdOnly(CMSNode node)
        {
            Assert.AreEqual(TextStringDataTypeId, node.Id);
            Assert.AreEqual(Guid.Empty, node.UniqueId); // ??
            Assert.AreEqual(Guid.Empty, node.nodeObjectType);
            Assert.AreEqual(0, node.Level);
            Assert.IsNull(node.Path);
            Assert.AreEqual(0, node.ParentId);
            Assert.IsNull(node.Text);
            Assert.AreEqual(0, node.sortOrder);
            Assert.AreEqual(0, node.User.Id);
            Assert.AreEqual(DateTime.MinValue, node.CreateDateTime);
            Assert.IsFalse(node.IsTrashed);

        }

        private class TestCMSNode : CMSNode
        {
            public bool SetupNodeCalled;

            public TestCMSNode(int id)
                : base(id)
            {
            }

            public TestCMSNode(int id, bool nosetup)
                : base(id, nosetup)
            {
            }

            public TestCMSNode(Guid id)
                : base(id)
            {
            }

            public TestCMSNode(Guid id, bool nosetup)
                : base(id, nosetup)
            {
            }

            protected override void setupNode()
            {
                base.setupNode();

                SetupNodeCalled = true;
            }

            public static CMSNode MakeNew(
                int parentId,
                int level,
                string text,
                Guid objectType)
            {
                return CMSNode.MakeNew(parentId, objectType, 0, level, text, Guid.NewGuid());
            }

            public void ExecuteSavePreviewXml(XmlDocument xd, Guid versionId)
            {
                SavePreviewXml(ToXml(xd, false), versionId);
            }

            public XmlNode ExecuteGetPreviewXml(XmlDocument xd, Guid versionId)
            {
                return GetPreviewXml(xd, versionId);
            }

            public static int[] ExecuteGetUniquesFromObjectTypeAndFirstLetter(Guid objectType, char letter)
            {
                return getUniquesFromObjectTypeAndFirstLetter(objectType, letter);
            }

            public static CMSNode CreateUsingSetupNode(int id)
            {
                var node = new TestCMSNode(id, true);
                node.setupNode();
                return node;
            }
        }
    }
}
