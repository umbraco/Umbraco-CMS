using umbraco.cms.businesslogic.template;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Xml;
using umbraco.BusinessLogic;
using System.Collections;
using umbraco.cms.businesslogic.web;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for TemplateTest and is intended
    ///to contain all TemplateTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TemplateTest
    {

        /// <summary>
        /// create a new template
        ///</summary>
        [TestMethod()]
        public void Template_Make_New()
        {

            var t = Template.MakeNew(Guid.NewGuid().ToString("N"), m_User);
            Assert.IsTrue(t.Id > 0);
            Assert.IsInstanceOfType(t, typeof(Template));

            t.delete();
            Assert.IsFalse(Template.IsNode(t.Id));
            
        }

        /// <summary>
        /// Make a new template as a master and a child template. Then try to delete the master template and ensure that it can't be deleted
        /// without first changning the child template to have a null parent.
        /// </summary>
        [TestMethod()]
        public void Template_Make_New_With_Master_And_Remove_Heirarchy_And_Delete()
        {
            var t = Template.MakeNew(Guid.NewGuid().ToString("N"), m_User);
            var child = Template.MakeNew(Guid.NewGuid().ToString("N"), m_User, t);
            Assert.IsTrue(t.Id > 0);
            Assert.IsInstanceOfType(t, typeof(Template));

            //verify heirarchy
            Assert.IsTrue(child.HasMasterTemplate);
            Assert.IsTrue(t.HasChildren);

            //make sure we can't delete it
            var hasException = false;
            try
            {
                t.delete();
            }
            catch (InvalidOperationException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);

            //System.Diagnostics.Debugger.Launch();

            //now we need to update the heirarchy...
            
            //though, this call will make changes in the database, it won't change our child object as our data layer doesn't have object
            //persistenece.
            t.RemoveAllReferences();
            //so we'll manually update our master page reference as well (0 = null)
            child.MasterTemplate = 0;

            //verify heirarchy
            Assert.IsFalse(child.HasMasterTemplate);
            Assert.IsFalse(t.HasChildren);

            //now delete it, should work now
            t.delete();
            Assert.IsFalse(Template.IsNode(t.Id));

            //remove the child
            child.delete();
            Assert.IsFalse(Template.IsNode(child.Id));

        }

        /// <summary>
        /// Creates a new document type, new template, asssign the template to the document type, create a new document using the new template
        /// then delete the template. This should throw an exception. We will not allow deleting a template that is currently in use
        /// </summary>
        [TestMethod()]        
        public void Template_Assign_To_Document_And_Delete()
        {
            //create the doc type, template and document
            var dt = DocumentType.MakeNew(m_User, Guid.NewGuid().ToString("N"));
            var t = Template.MakeNew(Guid.NewGuid().ToString("N"), m_User);
            
            //set the allowed templates
            dt.allowedTemplates = new Template[] { t };
            //now set the default
            dt.DefaultTemplate = t.Id;

            //create the document (this should have the default template set)
            var doc = Document.MakeNew(Guid.NewGuid().ToString("N"), dt, m_User, -1);

            Assert.AreEqual<int>(t.Id, doc.Template);

            //now delete, this should throw the exception
            var hasException = false;
            try
            {
                t.delete();
            }
            catch (InvalidOperationException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);

            //ok, now that we've proved it can't be removed, we'll remove the data in order
            doc.delete(true);
            Assert.IsFalse(Document.IsNode(doc.Id));

            dt.delete();
            Assert.IsFalse(DocumentType.IsNode(dt.Id));

            t.delete();
            Assert.IsFalse(Template.IsNode(t.Id));
        }

        private User m_User = new User(0);

        #region Tests to write
        ///// <summary>
        /////A test for Template Constructor
        /////</summary>
        //[TestMethod()]
        //public void TemplateConstructorTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Template Constructor
        /////</summary>
        //[TestMethod()]
        //public void TemplateConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Template target = new Template(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for ConvertToMasterPageSyntax
        /////</summary>
        //[TestMethod()]
        //public void ConvertToMasterPageSyntaxTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string templateDesign = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.ConvertToMasterPageSyntax(templateDesign);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for EnsureMasterPageSyntax
        /////</summary>
        //[TestMethod()]
        //public void EnsureMasterPageSyntaxTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string masterPageContent = string.Empty; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.EnsureMasterPageSyntax(masterPageContent);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAllAsList
        /////</summary>
        //[TestMethod()]
        //public void GetAllAsListTest()
        //{
        //    List<Template> expected = null; // TODO: Initialize to an appropriate value
        //    List<Template> actual;
        //    actual = Template.GetAllAsList();
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
        //    Template expected = null; // TODO: Initialize to an appropriate value
        //    Template actual;
        //    actual = Template.GetByAlias(Alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetMasterContentElement
        /////</summary>
        //[TestMethod()]
        //public void GetMasterContentElementTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    int masterTemplateId = 0; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetMasterContentElement(masterTemplateId);
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
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.GetRawText();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTemplate
        /////</summary>
        //[TestMethod()]
        //public void GetTemplateTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Template expected = null; // TODO: Initialize to an appropriate value
        //    Template actual;
        //    actual = Template.GetTemplate(id);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTemplateIdFromAlias
        /////</summary>
        //[TestMethod()]
        //public void GetTemplateIdFromAliasTest()
        //{
        //    string alias = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Template.GetTemplateIdFromAlias(alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest()
        //{
        //    XmlNode n = null; // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    Template expected = null; // TODO: Initialize to an appropriate value
        //    Template actual;
        //    actual = Template.Import(n, u);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ImportDesign
        /////</summary>
        //[TestMethod()]
        //public void ImportDesignTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string design = string.Empty; // TODO: Initialize to an appropriate value
        //    target.ImportDesign(design);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

       

        ///// <summary>
        /////A test for RemoveAllReferences
        /////</summary>
        //[TestMethod()]
        //public void RemoveAllReferencesTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    target.RemoveAllReferences();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for SaveAsMasterPage
        /////</summary>
        //[TestMethod()]
        //public void SaveAsMasterPageTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    target.SaveAsMasterPage();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for SaveMasterPageFile
        /////</summary>
        //[TestMethod()]
        //public void SaveMasterPageFileTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string masterPageContent = string.Empty; // TODO: Initialize to an appropriate value
        //    target.SaveMasterPageFile(masterPageContent);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    XmlDocument doc = null; // TODO: Initialize to an appropriate value
        //    XmlNode expected = null; // TODO: Initialize to an appropriate value
        //    XmlNode actual;
        //    actual = target.ToXml(doc);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for contentPlaceholderIds
        /////</summary>
        //[TestMethod()]
        //public void contentPlaceholderIdsTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    List<string> expected = null; // TODO: Initialize to an appropriate value
        //    List<string> actual;
        //    actual = target.contentPlaceholderIds();
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
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for getAll
        /////</summary>
        //[TestMethod()]
        //public void getAllTest()
        //{
        //    Template[] expected = null; // TODO: Initialize to an appropriate value
        //    Template[] actual;
        //    actual = Template.getAll();
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
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Alias = expected;
        //    actual = target.Alias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Design
        /////</summary>
        //[TestMethod()]
        //public void DesignTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Design = expected;
        //    actual = target.Design;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for HasChildren
        /////</summary>
        //[TestMethod()]
        //public void HasChildrenTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.HasChildren = expected;
        //    actual = target.HasChildren;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for HasMasterTemplate
        /////</summary>
        //[TestMethod()]
        //public void HasMasterTemplateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.HasMasterTemplate;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MasterPageFile
        /////</summary>
        //[TestMethod()]
        //public void MasterPageFileTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.MasterPageFile;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MasterTemplate
        /////</summary>
        //[TestMethod()]
        //public void MasterTemplateTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.MasterTemplate = expected;
        //    actual = target.MasterTemplate;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for OutputContentType
        /////</summary>
        //[TestMethod()]
        //public void OutputContentTypeTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.OutputContentType = expected;
        //    actual = target.OutputContentType;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for TemplateAliases
        /////</summary>
        //[TestMethod()]
        //public void TemplateAliasesTest()
        //{
        //    Hashtable expected = null; // TODO: Initialize to an appropriate value
        //    Hashtable actual;
        //    Template.TemplateAliases = expected;
        //    actual = Template.TemplateAliases;
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
        //    Template target = new Template(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Text = expected;
        //    actual = target.Text;
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
