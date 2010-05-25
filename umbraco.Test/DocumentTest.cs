using umbraco.cms.businesslogic.web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Linq;
using System.Threading;
using umbraco.cms.businesslogic.datatype;
using umbraco.editorControls.textfield;
using umbraco.cms.businesslogic.propertytype;
using umbraco.cms.businesslogic.property;
using umbraco.cms.businesslogic.language;
using umbraco.BusinessLogic.console;

namespace umbraco.Test
{
    
    
    /// <summary>
    /// Test for the Document API.
    ///</summary>
    ///<remarks>
    /// All of these tests are run in the ASP.Net context and these tests will require that there is data 
    /// in the Umbraco instance being tested including both document types and content.
    /// 
    /// This WILL make alot of SQL calls.
    /// 
    /// All of these tests will also delete any data that they create
    /// </remarks>
    [TestClass()]
    public class DocumentTest
    {

        /// <summary>
        /// Creates a bunch of nodes in a heirarchy, then deletes the top most node (moves to the recycle bin
        /// and completely deletes from system.) This should completely delete all of these nodes from the database.
        /// </summary>
        [TestMethod()]
        public void Document_DeleteHeirarchyPermanentlyTest()
        {
            var docList = new List<Document>();
            var total = 20;
            var dt = new DocumentType(GetExistingDocTypeId());
            //allow the doc type to be created underneath itself
            dt.AllowedChildContentTypeIDs = new int[] { dt.Id };
            dt.Save();

            //create 20 content nodes underneath each other, this will test deleting with heirarchy as well
            var lastParentId = -1;
            for (var i = 0; i < total; i++)
            {
                var newDoc = Document.MakeNew(i.ToString() + Guid.NewGuid().ToString("N"), dt, m_User, lastParentId);
                docList.Add(newDoc);
                Assert.IsTrue(docList[docList.Count - 1].Id > 0);
                lastParentId = newDoc.Id;
            }

            //now delete all of them permanently, since they are nested, we only need to delete one
            docList.First().delete(true);

            //make sure they are all gone
            foreach (var d in docList)
            {
                Assert.IsFalse(Document.IsNode(d.Id));
            }
            
        }

        /// <summary>
        ///A test for PublishWithResult
        ///</summary>
        [TestMethod()]
        public void Document_PublishWithResultTest()
        {
            var val = m_NewRootDoc.PublishWithResult(m_User);            
        }

        /// <summary>
        /// Creates a doc type, assigns a domain to it and removes it
        /// </summary>
        [TestMethod()]
        public void Document_AssignDomainTest()
        {
            var d = CreateNewUnderRoot(GetExistingDocType());

            var languages = Language.getAll.ToList();
            Assert.IsTrue(languages.Count > 0);

            //get all assigned domains
            var domains = Domain.GetDomains();

            var domainName = "www." + Guid.NewGuid().ToString("N") + ".com";

            //add a domain name to the node with the first language found
            Domain.MakeNew(domainName, d.Id, languages[0].id);
            Assert.IsTrue(Domain.Exists(domainName));
            Assert.AreEqual<int>(domains.Count + 1, Domain.GetDomains().Count);

            //delete the document, ensure that the domain is gone
            RecycleAndDelete(d);

            Assert.IsFalse(Domain.Exists(domainName));
            Assert.AreEqual<int>(domains.Count, Domain.GetDomains().Count);
        }

        /// <summary>
        ///A test for making a new document and deleting it which actuall first moves it to the recycle bin
        ///and then deletes it.
        ///</summary>
        [TestMethod()]
        public void Document_MakeNewTest()
        {
            Assert.IsInstanceOfType(m_NewRootDoc, typeof(Document));
        }

        /// <summary>
        /// A test for Copying a node, then deleting the copied node.
        /// This does error checking on the case of when a node is being copied that is in the root and doesn't have a parent node, it will 
        /// lookup the root docs to do the test.
        ///</summary>
        [TestMethod()]
        public void Document_CopyTest()
        {
            //System.Diagnostics.Debugger.Break();
            Document target = new Document(GetExistingNodeId());
            int parentId = target.ParentId;
            bool RelateToOrignal = false;

            //get children ids for the current parent
            var childrenIds = GetChildNodesOfParent(target).Select(x => x.Id);

            //copy the node
            target.Copy(parentId, m_User, RelateToOrignal);

            //test that the child id count + 1 is equal to the total child count
            Assert.AreEqual(childrenIds.Count() + 1, GetChildNodesOfParent(target).Count(), "Child node counts do not match");

            //get the list of new child ids from the parent
            var newChildIds = GetChildNodesOfParent(target).Select(x => x.Id);
            //get the children difference which should be the new node
            var diff = newChildIds.Except(childrenIds);
            
            Assert.AreEqual(1, diff.Count());

            //get the node that is the difference to compare
            Document newDoc = new Document(diff.First());
            Assert.AreEqual<int>(parentId, newDoc.ParentId);

            RecycleAndDelete(newDoc);
        }

        /// <summary>
        /// Tests copying by relating nodes, then deleting
        /// </summary>
        [TestMethod()]
        public void Document_CopyAndRelateTest()
        {
            //System.Diagnostics.Debugger.Break();
            Document target = new Document(GetExistingNodeId());
            int parentId = target.ParentId;
            bool RelateToOrignal = true;

            //get children ids            
            var childrenIds = GetChildNodesOfParent(target).Select(x => x.Id);

            target.Copy(parentId, m_User, RelateToOrignal);

            Assert.AreEqual(childrenIds.Count() + 1, GetChildNodesOfParent(target).Count());

            Document parent = new Document(parentId);
            //get the children difference which should be the new node
            var diff = parent.Children.ToList().Select(x => x.Id).Except(childrenIds);

            Assert.AreEqual(1, diff.Count());

            Document newDoc = new Document(diff.First());

            RecycleAndDelete(newDoc);
        }

        /// <summary>
        ///Create a new document, create preview xml for it, then delete it
        ///</summary>
        [TestMethod()]
        public void Document_ToPreviewXmlTest()
        {
            //System.Diagnostics.Debugger.Break();
            var doc = m_NewRootDoc;
            var id = doc.Id;
            Assert.IsTrue(doc.Id > 0);

            XmlDocument xd = new XmlDocument();
            var xmlNode = doc.ToPreviewXml(xd);

            Assert.IsNotNull(xmlNode);
            Assert.IsTrue(xmlNode.HasChildNodes);
        }

        /// <summary>
        /// Run test to create a node, publish it and delete it. This will test the versioning too.
        /// </summary>
        [TestMethod()]
        public void Document_MakeNewAndPublishTest()
        {
            //System.Diagnostics.Debugger.Break();
            var doc = m_NewRootDoc;
            var id = doc.Id;
            Assert.IsTrue(doc.Id > 0);

            var versionCount = doc.GetVersions().Count();

            doc.Publish(m_User);

            Assert.IsTrue(doc.Published);
            Assert.AreEqual(versionCount + 1, doc.GetVersions().Count());
        }

        /// <summary>
        ///A test that creates a new document, publishes it, unpublishes it and finally deletes it
        ///</summary>
        [TestMethod()]
        public void Document_PublishThenUnPublishTest()
        {
            //System.Diagnostics.Debugger.Break();
            var doc = m_NewRootDoc;
            var id = doc.Id;
            Assert.IsTrue(doc.Id > 0);

            var versionCount = doc.GetVersions().Count();

            doc.Publish(m_User);

            Assert.IsTrue(doc.Published);
            Assert.AreEqual(versionCount + 1, doc.GetVersions().Count());

            doc.UnPublish();

            Assert.IsFalse(doc.Published);
        }

        /// <summary>
        ///A test that makes a new document, updates some properties, saves and publishes the document, then rolls the document back and finally deletes it.
        ///</summary>
        [TestMethod()]
        public void Document_SaveAndPublishThenRollBackTest()
        {
            //System.Diagnostics.Debugger.Break();

            //create new document in the root
            var doc = m_NewRootDoc;
            var id = doc.Id;
            Assert.IsTrue(doc.Id > 0);

            //get a text property
            var prop = GetTextFieldProperty(m_ExistingDocType, doc);
            var originalPropVal = prop.Value;
            var versionCount = doc.GetVersions().Count();

            //save
            //wait a sec so that there's a time delay between the update time and version time
            Thread.Sleep(1000);
            doc.Save();
            Assert.IsTrue(doc.HasPendingChanges());

            //publish and create new version
            doc.Publish(m_User);
            Assert.IsTrue(doc.Published);
            var versions = doc.GetVersions().ToList();
            Assert.AreEqual(versionCount + 1, versions.Count());

            prop.Value = "updated!"; //udpate the prop            
            Assert.AreNotEqual(originalPropVal, prop.Value);

            //rollback to first version            
            doc.RollBack(versions.OrderBy(x => x.Date).Last().Version, m_User);

            var rolledBack = new Document(id);

            Assert.AreEqual(originalPropVal, rolledBack.GenericProperties.ToList().Where(x => x.PropertyType.Alias == "headerText").First().Value);
        }

        /// <summary>
        ///Tests creating a new document type, then creating a new node with that document type, adding some data to it, then deleting the 
        ///document type which should delete all documents associated with that.
        ///This will create a document type that has it's own id allowed as children. When we create the content nodes, we'll create
        ///them as children of each other to ensure the deletion occurs correctly.
        ///</summary>
        [TestMethod()]
        public void Document_DeleteAllDocsByDocumentTypeTest()
        {
            //System.Diagnostics.Debugger.Break();

            //create a new doc type
            string name = "TEST-" + Guid.NewGuid().ToString("N");
            var dt = DocumentType.MakeNew(m_User, name);

            //test the doc type
            Assert.AreEqual(DateTime.Now.Date, dt.CreateDateTime.Date);
            Assert.IsTrue(dt.Id > 0);

            //allow itself to be created under itself
            dt.AllowedChildContentTypeIDs = new int[] { dt.Id };
            //create a tab 
            dt.AddVirtualTab("TEST");

            //test the tab
            var tabs = dt.getVirtualTabs.ToList();
            Assert.AreEqual(1, tabs.Count);

            //create a property
            var allDataTypes = DataTypeDefinition.GetAll().ToList(); //get all definitions
            dt.AddPropertyType(allDataTypes[0], "testProperty", "Test Property"); //add a property type of the first type found in the list

            //test the prop
            var prop = dt.getPropertyType("testProperty");
            Assert.AreEqual("Test Property", prop.Name);

            //create 1st node
            var node1 = Document.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), dt, m_User, -1);
            Assert.IsTrue(node1.Id > 0);

            //create 2nd node underneath node 1
            var node2 = Document.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), dt, m_User, node1.Id);
            Assert.IsTrue(node2.Id > 0);
            Assert.AreEqual(node1.Id, node2.Parent.Id);

            //create 3rd node underneath node 2
            var node3 = Document.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), dt, m_User, node2.Id);
            Assert.IsTrue(node3.Id > 0);
            Assert.AreEqual(node2.Id, node3.Parent.Id);

            Document.DeleteFromType(dt);

            Assert.IsFalse(Document.IsNode(node1.Id));
            Assert.IsFalse(Document.IsNode(node2.Id));
            Assert.IsFalse(Document.IsNode(node3.Id));

            //now remove the document type created
            dt.delete();

            Assert.IsFalse(DocumentType.IsNode(dt.Id));
        }

        /// <summary>
        /// This will find a document type that supports a heirarchy, create 2 root nodes, then create a child node under the first one,
        /// then move it to the second one and finally delete everything that was created.
        /// </summary>
        [TestMethod]
        public void Document_MoveTest()
        {
            //first need to document type that allows other types of document types to exist underneath it
            DocumentType parent = null;
            DocumentType child = null;
            var ids = DocumentType.getAllUniqueNodeIdsFromObjectType(DocumentType._objectType);
            foreach (var id in ids)
            {
                var dt = new DocumentType(id);
                var allowed = dt.AllowedChildContentTypeIDs.ToList();
                if (allowed.Count() > 0)
                {
                    parent = dt;
                    child = new DocumentType(allowed[0]);
                    break;
                }
            }
            if (parent == null || child == null)
            {
                throw new NotImplementedException("The umbraco install doesn't have document types that support a heirarchy");
            }

            //now that we have a parent and a child, we need to create some documents
            var node1 = Document.MakeNew("FromCopy" + Guid.NewGuid().ToString("N"), parent, m_User, -1);
            Assert.IsTrue(node1.Id > 0);

            var node2 = Document.MakeNew("ToCopy" + Guid.NewGuid().ToString("N"), parent, m_User, -1);
            Assert.IsTrue(node2.Id > 0);

            //we now have 2 nodes in the root of the same type, we'll create a child node under node1 and move it to node2
            var childNode = Document.MakeNew("ChildCopy" + Guid.NewGuid().ToString("N"), child, m_User, node2.Id);
            Assert.IsTrue(childNode.Id > 0);

            childNode.Move(node2.Id);
            Assert.AreEqual(node2.Id, childNode.Parent.Id);

            RecycleAndDelete(childNode);
            RecycleAndDelete(node2);
            RecycleAndDelete(node1);
        }

        /// <summary>
        /// This will find an existing node, copy it to the same parent, delete the copied node and restore it, then finally completley remove it.
        /// </summary>
        [TestMethod]
        public void Document_UndeleteTest()
        {
            //find existing content
            var doc = new Document(GetExistingNodeId());
            //create new content based on the existing content in the same heirarchy
            var dt = new DocumentType(doc.ContentType.Id);
            var parentId = doc.ParentId;
            var newDoc = Document.MakeNew("NewDoc" + Guid.NewGuid().ToString("N"), dt, m_User, parentId);
            Assert.IsTrue(newDoc.Id > 0);

            //this will recycle the node
            newDoc.delete();
            Assert.IsTrue(newDoc.IsTrashed);
            Assert.IsTrue(newDoc.Path.Contains("," + (int)RecycleBin.RecycleBinType.Content + ","));

            //undelete the node (move it)
            newDoc.Move(parentId);
            Assert.IsFalse(newDoc.IsTrashed);
            Assert.IsFalse(newDoc.Path.Contains("," + (int)RecycleBin.RecycleBinType.Content + ","));

            //remove it completely
            RecycleAndDelete(newDoc);
        }

        /// <summary>
        /// This method will create 20 content nodes, send them to the recycle bin and then empty the recycle bin
        /// </summary>
        [TestMethod]
        public void Document_EmptyRecycleBinTest()
        {
            var docList = new List<Document>();
            var total = 20;
            var dt = m_ExistingDocType;
            //allow the doc type to be created underneath itself
            dt.AllowedChildContentTypeIDs = new int[] { dt.Id };
            dt.Save();

            //create 20 content nodes underneath each other, this will test deleting with heirarchy as well
            var lastParentId = -1;
            for (var i = 0; i < total; i++)
            {
                var newDoc = Document.MakeNew("R-" + i.ToString() + Guid.NewGuid().ToString("N"), dt, m_User, lastParentId);
                docList.Add(newDoc);
                Assert.IsTrue(docList[docList.Count - 1].Id > 0);
                Assert.AreEqual(lastParentId, newDoc.ParentId);
                lastParentId = newDoc.Id;
            }

            //now delete all of them, since they are nested, we only need to delete one
            docList.First().delete();

            //a callback action for each item removed from the recycle bin
            var totalDeleted = 0;

            var bin = new RecycleBin(RecycleBin.RecycleBinType.Content);
            var totalTrashedItems = bin.GetDescendants().Cast<object>().Count();
            bin.CallTheGarbageMan(x =>
            {
                Assert.AreEqual(totalTrashedItems - (++totalDeleted), x);
            });

            Assert.AreEqual(0, RecycleBin.Count(RecycleBin.RecycleBinType.Content));
        }


        #region TEST TO BE WRITTEN

        ///// <summary>
        /////A test for XmlPopulate
        /////</summary>
        //[TestMethod()]
        //public void XmlPopulateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlNode x = null; // TODO: Initialize to an appropriate value
        //    XmlNode xExpected = null; // TODO: Initialize to an appropriate value
        //    bool Deep = false; // TODO: Initialize to an appropriate value
        //    target.XmlPopulate(xd, ref x, Deep);
        //    Assert.AreEqual(xExpected, x);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for XmlNodeRefresh
        /////</summary>
        //[TestMethod()]
        //public void XmlNodeRefreshTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlNode x = null; // TODO: Initialize to an appropriate value
        //    XmlNode xExpected = null; // TODO: Initialize to an appropriate value
        //    target.XmlNodeRefresh(xd, ref x);
        //    Assert.AreEqual(xExpected, x);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for XmlGenerate
        /////</summary>
        //[TestMethod()]
        //public void XmlGenerateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    target.XmlGenerate(xd);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    bool Deep = false; // TODO: Initialize to an appropriate value
        //    XmlNode expected = null; // TODO: Initialize to an appropriate value
        //    XmlNode actual;
        //    actual = target.ToXml(xd, Deep);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}


        ///// <summary>
        /////A test for SendToPublication
        /////</summary>
        //[TestMethod()]
        //public void SendToPublicationTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.SendToPublication(u);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RePublishAll
        /////</summary>
        //[TestMethod()]
        //public void RePublishAllTest()
        //{
        //    Document.RePublishAll();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for RegeneratePreviews
        /////</summary>
        //[TestMethod()]
        //public void RegeneratePreviewsTest()
        //{
        //    Document.RegeneratePreviews();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for refreshXmlSortOrder
        /////</summary>
        //[TestMethod()]
        //public void refreshXmlSortOrderTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    target.refreshXmlSortOrder();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for PublishWithSubs
        /////</summary>
        //[TestMethod()]
        //public void PublishWithSubsTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    target.PublishWithSubs(u);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        

        ///// <summary>
        /////A test for PublishWithChildrenWithResult
        /////</summary>
        //[TestMethod()]
        //public void PublishWithChildrenWithResultTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.PublishWithChildrenWithResult(u);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Publish
        /////</summary>
        //[TestMethod()]
        //public void PublishTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    target.Publish(u);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest()
        //{
        //    int ParentId = 0; // TODO: Initialize to an appropriate value
        //    User Creator = null; // TODO: Initialize to an appropriate value
        //    XmlElement Source = null; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Document.Import(ParentId, Creator, Source);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTextPath
        /////</summary>
        //[TestMethod()]
        //public void GetTextPathTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetTextPath();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetRootDocuments
        /////</summary>
        //[TestMethod()]
        //public void GetRootDocumentsTest()
        //{
        //    Document[] expected = null; // TODO: Initialize to an appropriate value
        //    Document[] actual;
        //    actual = Document.GetRootDocuments();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetNodesForPreview
        /////</summary>
        //[TestMethod()]
        //public void GetNodesForPreviewTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    bool childrenOnly = false; // TODO: Initialize to an appropriate value
        //    List<CMSPreviewNode> expected = null; // TODO: Initialize to an appropriate value
        //    List<CMSPreviewNode> actual;
        //    actual = target.GetNodesForPreview(childrenOnly);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetDocumentsForRelease
        /////</summary>
        //[TestMethod()]
        //public void GetDocumentsForReleaseTest()
        //{
        //    Document[] expected = null; // TODO: Initialize to an appropriate value
        //    Document[] actual;
        //    actual = Document.GetDocumentsForRelease();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetDocumentsForExpiration
        /////</summary>
        //[TestMethod()]
        //public void GetDocumentsForExpirationTest()
        //{
        //    Document[] expected = null; // TODO: Initialize to an appropriate value
        //    Document[] actual;
        //    actual = Document.GetDocumentsForExpiration();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetChildrenForTree
        /////</summary>
        //[TestMethod()]
        //public void GetChildrenForTreeTest()
        //{
        //    int NodeId = 0; // TODO: Initialize to an appropriate value
        //    Document[] expected = null; // TODO: Initialize to an appropriate value
        //    Document[] actual;
        //    actual = Document.GetChildrenForTree(NodeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for CountSubs
        /////</summary>
        //[TestMethod()]
        //public void CountSubsTest()
        //{
        //    int parentId = 0; // TODO: Initialize to an appropriate value
        //    bool publishedOnly = false; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Document.CountSubs(parentId, publishedOnly);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Copy
        /////</summary>
        //[TestMethod()]
        //public void CopyTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Document target = new Document(id); // TODO: Initialize to an appropriate value
        //    int CopyTo = 0; // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    target.Copy(CopyTo, u);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        #endregion

        #region Private properties and methods

        /// <summary>
        /// The user to be used to create stuff
        /// </summary>
        private static User m_User = new User(0);

        /// <summary>
        /// Used for each test initialization. Before each test is run a new root doc is created.
        /// </summary>
        private Document m_NewRootDoc;

        /// <summary>
        /// Gets initialized for each test and is set to an existing document type
        /// </summary>
        private DocumentType m_ExistingDocType;

        /// <summary>
        /// Completely remove the document, this will first recycle it and then delete it (the api doesn't directly support deleting completey in one step)
        /// </summary>
        /// <param name="d"></param>
        internal static void RecycleAndDelete(Document d)
        {
            if (d == null)
            {
                return;
            }

            var id = d.Id;

            //check if it is already trashed
            var alreadyTrashed = d.IsTrashed;

            if (!alreadyTrashed)
            {
                //now recycle it
                d.delete();

                Assert.IsTrue(d.IsTrashed);                
            }

            //now permanently delete
            d.delete(true);
            Assert.IsFalse(Document.IsNode(id));

            //check with sql that it is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id=@id",
                Application.SqlHelper.CreateParameter("@id", id));

            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Returns a random docuemnt type that supports a text property
        /// </summary>
        /// <returns></returns>
        internal static int GetExistingDocTypeId()
        {
            var types = DocumentType.GetAllAsList();
            DocumentType found = null;
            TextFieldDataType txtField = new TextFieldDataType();
            foreach (var d in types)
            {
                var prop = d.PropertyTypes
                    .Where(x => x.DataTypeDefinition.DataType.Id == txtField.Id).FirstOrDefault();
                if (prop != null)
                {
                    found = d;
                    break;
                }
            }
            if (found == null)
            {
                throw new MissingMemberException("No document type was found that contains a text field property");
            }
            return found.Id;
        }

        /// <summary>
        /// Returns a text field property of the document type specified. This will throw an exception if one is not found.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        internal static Property GetTextFieldProperty(DocumentType dt, Document d)
        {
            TextFieldDataType txtField = new TextFieldDataType();
            var prop = dt.PropertyTypes
                    .Where(x => x.DataTypeDefinition.DataType.Id == txtField.Id).First();
            return d.GenericProperties.Where(x => x.PropertyType.Id == prop.Id).First();
        }

        /// <summary>
        /// Returns a content node
        /// </summary>
        /// <returns></returns>
        internal static int GetExistingNodeId()
        {
            var ids = Document.getAllUniqueNodeIdsFromObjectType(Document._objectType).ToList();
            var r = new Random();
            var index = r.Next(0, ids.Count() - 1);
            return ids[index];
        }

        /// <summary>
        /// A helper method to get the parent node.
        /// The reason we need this is because the API currently throws an exception if we access the Parent property
        /// of a node and the node is on level 1. This also causes issues if the node is in level 1 in the recycle bin.
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        internal static IEnumerable<IconI> GetChildNodesOfParent(Document d)
        {
            if (d.ParentId == (int)RecycleBin.RecycleBinType.Content)
            {
                return new RecycleBin(RecycleBin.RecycleBinType.Content).Children.ToList();
            }
            else
            {
                if (d.Level == 1)
                {
                    return Document.GetRootDocuments();
                }
                else
                {
                    return d.Parent.Children;
                }
            }
        }

        internal static DocumentType GetExistingDocType()
        {
            DocumentType dct = new DocumentType(GetExistingDocTypeId());
            Assert.IsTrue(dct.Id > 0);
            return dct;
        }

        /// <summary>
        /// Creates a new node based on an existing doc type
        /// </summary>
        /// <returns></returns>
        internal static Document CreateNewUnderRoot(DocumentType dt)
        {
            string Name = "TEST-" + Guid.NewGuid().ToString("N");            
            int ParentId = -1;
            Document actual = Document.MakeNew(Name, dt, m_User, ParentId);
            var id = actual.Id;
            Assert.IsTrue(actual.Id > 0);
            return actual;
        }
        
        #endregion

        #region Test Context
        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        } 
        #endregion

        #region Initialize and cleanup
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
        
        
        /// <summary>
        /// Creates a new root document to use for each test if required
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_ExistingDocType = GetExistingDocType();
            m_NewRootDoc = CreateNewUnderRoot(m_ExistingDocType);
        }
        
        /// <summary>
        /// Makes sure the root doc is deleted
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            RecycleAndDelete(m_NewRootDoc);
        }
        
        #endregion
    }
}
