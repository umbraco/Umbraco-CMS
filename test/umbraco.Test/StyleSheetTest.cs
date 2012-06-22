using umbraco.cms.businesslogic.web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using System.Xml;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for StyleSheetTest and is intended
    ///to contain all StyleSheetTest Unit Tests
    ///</summary>
    [TestClass()]
    public class StyleSheetTest
    {

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void StyleSheet_Make_New()
        {
            
            var s = StyleSheet.MakeNew(m_User, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N") + ".css", Guid.NewGuid().ToString("N"));
            Assert.IsTrue(s.Id > 0);
            Assert.IsInstanceOfType(s, typeof(StyleSheet));

            //now remove it
            s.delete();
            Assert.IsFalse(StyleSheet.IsNode(s.Id));
            
        }

        [TestMethod()]
        public void StyleSheet_Make_New_AddProperty()
        {

            var s = StyleSheet.MakeNew(m_User, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N") + ".css", Guid.NewGuid().ToString("N"));
            Assert.IsTrue(s.Id > 0);
            Assert.IsInstanceOfType(s, typeof(StyleSheet));

            //add property
            var p = s.AddProperty(Guid.NewGuid().ToString("N"), m_User);
            Assert.IsTrue(p.Id > 0);
            Assert.IsInstanceOfType(p, typeof(StylesheetProperty));
            
            //now remove it
            s.delete();
            Assert.IsFalse(StyleSheet.IsNode(s.Id));

            //make sure the property is gone too
            Assert.IsFalse(StylesheetProperty.IsNode(p.Id));

        }

        private User m_User = new User(0);

        #region Tests to write
        ///// <summary>
        /////A test for StyleSheet Constructor
        /////</summary>
        //[TestMethod()]
        //public void StyleSheetConstructorTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    bool setupStyleProperties = false; // TODO: Initialize to an appropriate value
        //    bool loadContentFromFile = false; // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id, setupStyleProperties, loadContentFromFile);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for StyleSheet Constructor
        /////</summary>
        //[TestMethod()]
        //public void StyleSheetConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for StyleSheet Constructor
        /////</summary>
        //[TestMethod()]
        //public void StyleSheetConstructorTest2()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for AddProperty
        /////</summary>
        //[TestMethod()]
        //public void AddPropertyTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    string Alias = string.Empty; // TODO: Initialize to an appropriate value
        //    User u = null; // TODO: Initialize to an appropriate value
        //    StylesheetProperty expected = null; // TODO: Initialize to an appropriate value
        //    StylesheetProperty actual;
        //    actual = target.AddProperty(Alias, u);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    StyleSheet[] expected = null; // TODO: Initialize to an appropriate value
        //    StyleSheet[] actual;
        //    actual = StyleSheet.GetAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetByName
        /////</summary>
        //[TestMethod()]
        //public void GetByNameTest()
        //{
        //    string name = string.Empty; // TODO: Initialize to an appropriate value
        //    StyleSheet expected = null; // TODO: Initialize to an appropriate value
        //    StyleSheet actual;
        //    actual = StyleSheet.GetByName(name);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetStyleSheet
        /////</summary>
        //[TestMethod()]
        //public void GetStyleSheetTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    bool setupStyleProperties = false; // TODO: Initialize to an appropriate value
        //    bool loadContentFromFile = false; // TODO: Initialize to an appropriate value
        //    StyleSheet expected = null; // TODO: Initialize to an appropriate value
        //    StyleSheet actual;
        //    actual = StyleSheet.GetStyleSheet(id, setupStyleProperties, loadContentFromFile);
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
        //    StyleSheet expected = null; // TODO: Initialize to an appropriate value
        //    StyleSheet actual;
        //    actual = StyleSheet.Import(n, u);
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
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlNode expected = null; // TODO: Initialize to an appropriate value
        //    XmlNode actual;
        //    actual = target.ToXml(xd);
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
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for saveCssToFile
        /////</summary>
        //[TestMethod()]
        //public void saveCssToFileTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    target.saveCssToFile();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Content
        /////</summary>
        //[TestMethod()]
        //public void ContentTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Content = expected;
        //    actual = target.Content;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Filename
        /////</summary>
        //[TestMethod()]
        //public void FilenameTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Filename = expected;
        //    actual = target.Filename;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Properties
        /////</summary>
        //[TestMethod()]
        //public void PropertiesTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    StyleSheet target = new StyleSheet(id); // TODO: Initialize to an appropriate value
        //    StylesheetProperty[] actual;
        //    actual = target.Properties;
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
