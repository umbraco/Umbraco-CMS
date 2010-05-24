using umbraco.cms.businesslogic.web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using System.Xml;
using System.Linq;
using umbraco.cms.businesslogic.template;
using umbraco.cms.businesslogic.datatype;
using System.Data.SqlClient;
using umbraco.cms.businesslogic;

namespace umbraco.Test
{
    
    
    /// <summary>
    /// Tests DocumentType apis.
    ///</summary>
    ///<remarks>
    /// After each test is run, any document type created is removed
    /// </remarks>
    [TestClass()]
    public class DocumentTypeTest
    {
        [TestMethod()]
        public void DocumentType_DeleteDocTypeWithContent()
        {
            var dt = CreateNewDocType();
            var doc = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), dt, m_User, -1);
            Assert.IsInstanceOfType(doc, typeof(Document));
            Assert.IsTrue(doc.Id > 0);

            DeleteDocType(dt);

            Assert.IsFalse(Document.IsNode(doc.Id));
        }

        /// <summary>
        /// This will create 3 document types, and create nodes in the following structure:
        /// - root
        /// -- node1 (of doc type #1)
        /// --- node 2 (of doc type #2)
        /// ---- node 3 (of doc type #1)
        /// ----- node 4 (of doc type #3)
        /// 
        /// Then we'll delete doc type #1. The result should be that node1 and node3 are completely deleted from the database and node2 and node4 are
        /// moved to the recycle bin.
        /// </summary>
        [TestMethod()]
        public void DocumentType_DeleteDocTypeWithContentAndChildrenOfDifferentDocTypes()
        {
            //System.Diagnostics.Debugger.Break();

            //create the doc types 
            var dt1 = CreateNewDocType();
            var dt2 = CreateNewDocType();
            var dt3 = CreateNewDocType();

            //create the heirarchy
            dt1.AllowedChildContentTypeIDs = new int[] { dt2.Id, dt3.Id };
            dt1.Save();
            dt2.AllowedChildContentTypeIDs = new int[] { dt1.Id };
            dt2.Save();

            //create the content tree
            var node1 = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), dt1, m_User, -1);
            var node2 = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), dt2, m_User, node1.Id);
            var node3 = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), dt1, m_User, node2.Id);
            var node4 = Document.MakeNew("TEST" + Guid.NewGuid().ToString("N"), dt3, m_User, node3.Id);

            //do the deletion of doc type #1
            DeleteDocType(dt1);

            //do our checks
            Assert.IsFalse(Document.IsNode(node1.Id), "node1 is not deleted"); //this was of doc type 1, should be gone
            Assert.IsFalse(Document.IsNode(node3.Id), "node3 is not deleted"); //this was of doc type 1, should be gone

            Assert.IsTrue(Document.IsNode(node2.Id), "node2 is deleted");
            Assert.IsTrue(Document.IsNode(node4.Id), "node4 is deleted");

            node2 = new Document(node2.Id);//need to re-query the node
            Assert.IsTrue(node2.IsTrashed, "node2 is not in the trash");
            node4 = new Document(node4.Id); //need to re-query the node
            Assert.IsTrue(node4.IsTrashed, "node 4 is not in the trash");

            //remove the old data
            DeleteDocType(dt2);
            DeleteDocType(dt3);

        }

        /// <summary>
        ///A test for creating a new document type
        ///</summary>
        [TestMethod()]
        public void DocumentType_MakeNewTest()
        {
            Assert.IsInstanceOfType(m_NewDocType, typeof(DocumentType));
        }

        /// <summary>
        /// Tests adding every type of property to a new document type on a new tab, then delete the tab, then the document type
        /// </summary>
        [TestMethod()]
        public void DocumentType_AddPropertiesToTabThenDeleteItTest()
        {
            //System.Diagnostics.Debugger.Break();
            
            //allow itself to be created under itself
            m_NewDocType.AllowedChildContentTypeIDs = new int[] { m_NewDocType.Id };
            //create a tab 
            m_NewDocType.AddVirtualTab("TEST");

            //test the tab
            var tabs = m_NewDocType.getVirtualTabs.ToList();
            Assert.AreEqual(1, tabs.Count);

            //create a property
            var allDataTypes = DataTypeDefinition.GetAll().ToList(); //get all definitions
            var i = 0;
            foreach (var dataType in allDataTypes)
            {
                //add a property type of the first type found in the list
                m_NewDocType.AddPropertyType(dataType, "testProperty" + (++i).ToString(), "Test Property" + i.ToString());
                //test the prop
                var prop = m_NewDocType.getPropertyType("testProperty" + i.ToString());
                Assert.IsTrue(prop.Id > 0);
                Assert.AreEqual("Test Property" + i.ToString(), prop.Name);
                //put the properties to the tab
                m_NewDocType.SetTabOnPropertyType(prop, tabs[0].Id);                
                //re-get the property since data is cached in the object
                prop = m_NewDocType.getPropertyType("testProperty" + i.ToString());
                Assert.AreEqual<int>(tabs[0].Id, prop.TabId);
            }

            //now we need to delete the tab
            m_NewDocType.DeleteVirtualTab(tabs[0].Id);
        }

        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void DocumentType_GetAllTest()
        {
            //check with sql that it's the correct number of children
            var ids = new List<int>();
            using (var reader = Application.SqlHelper.ExecuteReader(DocumentType.m_SQLOptimizedGetAll,
                Application.SqlHelper.CreateParameter("@nodeObjectType", DocumentType._objectType)))
            {
                while (reader.Read())
                {
                    ids.Add(reader.Get<int>("id"));
                }                
            }

            var all = DocumentType.GetAllAsList();

            Assert.AreEqual<int>(ids.Distinct().Count(), all.Count);
        }

        /// <summary>
        ///A test for HasChildren
        ///</summary>
        [TestMethod()]        
        public void DocumentType_HasChildrenTest()
        {
            //System.Diagnostics.Debugger.Break();

            var dt1 = DocumentType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));

            Assert.IsTrue(dt1.Id > 0);
            Assert.AreEqual(DateTime.Now.Date, dt1.CreateDateTime.Date);
            Assert.IsFalse(dt1.HasChildren);

            var dt2 = DocumentType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));

            Assert.IsTrue(dt2.Id > 0);
            Assert.AreEqual(DateTime.Now.Date, dt2.CreateDateTime.Date);
            Assert.IsFalse(dt2.HasChildren);
          
            dt2.MasterContentType = dt1.Id;
            dt2.Save();

            //unfortunately this won't work! because the HasChildren property is cached
            //Assert.IsTrue(dt1.HasChildren);
            var reloaded = new DocumentType(dt1.Id);
            Assert.IsTrue(reloaded.HasChildren);

            var hasError = false;
            try
            {
                DeleteDocType(dt1);
            }
            catch (ArgumentException)
            {
                hasError = true;
            }
            Assert.IsTrue(hasError);

            DeleteDocType(dt2);
            DeleteDocType(dt1);
        }

        #region Tests to write
        ///// <summary>
        /////A test for allowedTemplates
        /////</summary>
        //[TestMethod()]
        //public void allowedTemplatesTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    Template[] expected = null; // TODO: Initialize to an appropriate value
        //    Template[] actual;
        //    target.allowedTemplates = expected;
        //    actual = target.allowedTemplates;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for DefaultTemplate
        /////</summary>
        //[TestMethod()]
        //public void DefaultTemplateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.DefaultTemplate = expected;
        //    actual = target.DefaultTemplate;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}


        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlElement expected = null; // TODO: Initialize to an appropriate value
        //    XmlElement actual;
        //    actual = target.ToXml(xd);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for RemoveDefaultTemplate
        /////</summary>
        //[TestMethod()]
        //public void RemoveDefaultTemplateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    target.RemoveDefaultTemplate();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        

        ///// <summary>
        /////A test for HasTemplate
        /////</summary>
        //[TestMethod()]
        //public void HasTemplateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.HasTemplate();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetByAlias
        /////</summary>
        //[TestMethod()]
        //public void GetByAliasTest()
        //{
        //    string Alias = string.Empty; // TODO: Initialize to an appropriate value
        //    DocumentType expected = null; // TODO: Initialize to an appropriate value
        //    DocumentType actual;
        //    actual = DocumentType.GetByAlias(Alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAllAsList
        /////</summary>
        //[TestMethod()]
        //public void GetAllAsListTest()
        //{
        //    List<DocumentType> expected = null; // TODO: Initialize to an appropriate value
        //    List<DocumentType> actual;
        //    actual = DocumentType.GetAllAsList();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GenerateDtd
        /////</summary>
        //[TestMethod()]
        //public void GenerateDtdTest()
        //{
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = DocumentType.GenerateDtd();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for clearTemplates
        /////</summary>
        //[TestMethod()]
        //public void clearTemplatesTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    DocumentType target = new DocumentType(id); // TODO: Initialize to an appropriate value
        //    target.clearTemplates();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //} 

        ///// <summary>
        /////A test for Thumbnail
        /////</summary>
        //[TestMethod()]
        //public void ThumbnailTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Thumbnail = expected;
        //    actual = target.Thumbnail;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Text
        /////</summary>
        //[TestMethod()]
        //public void TextTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Text = expected;
        //    actual = target.Text;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for PropertyTypes
        /////</summary>
        //[TestMethod()]
        //public void PropertyTypesTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    List<PropertyType> actual;
        //    actual = target.PropertyTypes;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MasterContentType
        /////</summary>
        //[TestMethod()]
        //public void MasterContentTypeTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.MasterContentType = expected;
        //    actual = target.MasterContentType;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for IconUrl
        /////</summary>
        //[TestMethod()]
        //public void IconUrlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.IconUrl = expected;
        //    actual = target.IconUrl;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getVirtualTabs
        /////</summary>
        //[TestMethod()]
        //public void getVirtualTabsTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    ContentType.TabI[] actual;
        //    actual = target.getVirtualTabs;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Description
        /////</summary>
        //[TestMethod()]
        //public void DescriptionTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Description = expected;
        //    actual = target.Description;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for AllowedChildContentTypeIDs
        /////</summary>
        //[TestMethod()]
        //public void AllowedChildContentTypeIDsTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    int[] expected = null; // TODO: Initialize to an appropriate value
        //    int[] actual;
        //    target.AllowedChildContentTypeIDs = expected;
        //    actual = target.AllowedChildContentTypeIDs;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Alias
        /////</summary>
        //[TestMethod()]
        //public void AliasTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Alias = expected;
        //    actual = target.Alias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for SetTabSortOrder
        /////</summary>
        //[TestMethod()]
        //public void SetTabSortOrderTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    int tabId = 0; // TODO: Initialize to an appropriate value
        //    int sortOrder = 0; // TODO: Initialize to an appropriate value
        //    target.SetTabSortOrder(tabId, sortOrder);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for SetTabOnPropertyType
        /////</summary>
        //[TestMethod()]
        //public void SetTabOnPropertyTypeTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    int TabId = 0; // TODO: Initialize to an appropriate value
        //    target.SetTabOnPropertyType(pt, TabId);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for SetTabName
        /////</summary>
        //[TestMethod()]
        //public void SetTabNameTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    int tabId = 0; // TODO: Initialize to an appropriate value
        //    string Caption = string.Empty; // TODO: Initialize to an appropriate value
        //    target.SetTabName(tabId, Caption);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for removePropertyTypeFromTab
        /////</summary>
        //[TestMethod()]
        //public void removePropertyTypeFromTabTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    target.removePropertyTypeFromTab(pt);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for getTabIdFromPropertyType
        /////</summary>
        //[TestMethod()]
        //public void getTabIdFromPropertyTypeTest()
        //{
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = ContentType.getTabIdFromPropertyType(pt);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetRawText
        /////</summary>
        //[TestMethod()]
        //public void GetRawTextTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetRawText();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for getPropertyType
        /////</summary>
        //[TestMethod()]
        //public void getPropertyTypeTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
        //    PropertyType expected = null; // TODO: Initialize to an appropriate value
        //    PropertyType actual;
        //    actual = target.getPropertyType(alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetContentType
        /////</summary>
        //[TestMethod()]
        //public void GetContentTypeTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    ContentType expected = null; // TODO: Initialize to an appropriate value
        //    ContentType actual;
        //    actual = ContentType.GetContentType(id);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetByAlias
        /////</summary>
        //[TestMethod()]
        //public void GetByAliasTest()
        //{
        //    string Alias = string.Empty; // TODO: Initialize to an appropriate value
        //    ContentType expected = null; // TODO: Initialize to an appropriate value
        //    ContentType actual;
        //    actual = ContentType.GetByAlias(Alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    ContentType[] expected = null; // TODO: Initialize to an appropriate value
        //    ContentType[] actual;
        //    actual = target.GetAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for FlushTabCache
        /////</summary>
        //[TestMethod()]
        //public void FlushTabCacheTest()
        //{
        //    int TabId = 0; // TODO: Initialize to an appropriate value
        //    int ContentTypeId = 0; // TODO: Initialize to an appropriate value
        //    ContentType.FlushTabCache(TabId, ContentTypeId);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for AddVirtualTab
        /////</summary>
        //[TestMethod()]
        //public void AddVirtualTabTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    string Caption = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.AddVirtualTab(Caption);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for AddPropertyType
        /////</summary>
        //[TestMethod()]
        //public void AddPropertyTypeTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id); // TODO: Initialize to an appropriate value
        //    DataTypeDefinition dt = null; // TODO: Initialize to an appropriate value
        //    string Alias = string.Empty; // TODO: Initialize to an appropriate value
        //    string Name = string.Empty; // TODO: Initialize to an appropriate value
        //    target.AddPropertyType(dt, Alias, Name);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ContentType Constructor
        /////</summary>
        //[TestMethod()]
        //public void ContentTypeConstructorTest2()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for ContentType Constructor
        /////</summary>
        //[TestMethod()]
        //public void ContentTypeConstructorTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for ContentType Constructor
        /////</summary>
        //[TestMethod()]
        //public void ContentTypeConstructorTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    bool noSetup = false; // TODO: Initialize to an appropriate value
        //    ContentType target = new ContentType(id, noSetup);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //} 
        #endregion

        #region Private properties and methods
        
        private User m_User = new User(0);

        /// <summary>
        /// before each test starts, this object is created so it can be used for testing.
        /// </summary>
        private DocumentType m_NewDocType;

        /// <summary>
        /// Create a brand new document type
        /// </summary>
        /// <returns></returns>
        private DocumentType CreateNewDocType()
        {
            var dt = DocumentType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            Assert.IsTrue(dt.Id > 0);
            Assert.AreEqual(DateTime.Now.Date, dt.CreateDateTime.Date);
            return dt;
        }

        private void DeleteDocType(DocumentType dt)
        {
            var id = dt.Id;

            dt.delete();

            //check with sql that it is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id=@id",
                Application.SqlHelper.CreateParameter("@id", id));

            Assert.AreEqual(0, count);
        }
        
        #endregion

        #region Test context
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
        /// Create a new document type for use in tests
        /// </summary>
        [TestInitialize()]
        public void MyTestInitialize()
        {
            m_NewDocType = CreateNewDocType();
        }
        
        /// <summary>
        /// Remove the created document type
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            DeleteDocType(m_NewDocType);
        }
        
        #endregion

    }
}
