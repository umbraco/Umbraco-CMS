using umbraco.cms.businesslogic.macro;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for MacroTest and is intended
    ///to contain all MacroTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MacroTest
    {

        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Macro_Not_Found_Constructor()
        {
            Macro target = new Macro(-1111);  
        }

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void Macro_Make_New()
        {
            var m = Macro.MakeNew(Guid.NewGuid().ToString("N"));
            Assert.IsTrue(m.Id > 0);
            Assert.IsInstanceOfType(m, typeof(Macro));

            m.Delete();
            var isfound = false;
            try
            {
                var asdf = new Macro(m.Id);
                isfound = true;
            }
            catch (ArgumentException)
            {
                isfound = false;
            }

            Assert.IsFalse(isfound);
        }

        /// <summary>
        /// Creates a new macro, add a property to it and delete the macro ensuring the properties are all gone.
        /// </summary>
        [TestMethod()]
        public void Macro_Make_New_Add_Property()
        {
            var m = Macro.MakeNew(Guid.NewGuid().ToString("N"));
            Assert.IsTrue(m.Id > 0);
            Assert.IsInstanceOfType(m, typeof(Macro));

            //now, add a property...
            
            //get the first macro property type we can find
            var mpt = MacroPropertyType.GetAll.First();
            var mp = MacroProperty.MakeNew(m, false, Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), mpt);
            Assert.IsTrue(mp.Id > 0);
            Assert.IsInstanceOfType(mp, typeof(MacroProperty));

            m.Delete();
            
            //make sure the property is gone 
            var props = MacroProperty.GetProperties(m.Id);
            Assert.IsFalse(props.Select(x => x.Id).Contains(mp.Id));
            
            var isfound = false;
            try
            {
                var asdf = new Macro(m.Id);
                isfound = true;
            }
            catch (ArgumentException)
            {
                isfound = false;
            }

            Assert.IsFalse(isfound);

        }


        #region Tests to write
        

        ///// <summary>
        /////A test for Macro Constructor
        /////</summary>
        //[TestMethod()]
        //public void MacroConstructorTest1()
        //{
        //    int Id = 0; // TODO: Initialize to an appropriate value
        //    Macro target = new Macro(Id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Macro Constructor
        /////</summary>
        //[TestMethod()]
        //public void MacroConstructorTest2()
        //{
        //    Macro target = new Macro();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    Macro[] expected = null; // TODO: Initialize to an appropriate value
        //    Macro[] actual;
        //    actual = Macro.GetAll();
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
        //    Macro expected = null; // TODO: Initialize to an appropriate value
        //    Macro actual;
        //    actual = Macro.GetByAlias(Alias);
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
        //    Macro expected = null; // TODO: Initialize to an appropriate value
        //    Macro actual;
        //    actual = Macro.Import(n);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for RefreshProperties
        /////</summary>
        //[TestMethod()]
        //public void RefreshPropertiesTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    target.RefreshProperties();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ToXml
        /////</summary>
        //[TestMethod()]
        //public void ToXmlTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    XmlDocument xd = null; // TODO: Initialize to an appropriate value
        //    XmlNode expected = null; // TODO: Initialize to an appropriate value
        //    XmlNode actual;
        //    actual = target.ToXml(xd);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Alias
        /////</summary>
        //[TestMethod()]
        //public void AliasTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Alias = expected;
        //    actual = target.Alias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Assembly
        /////</summary>
        //[TestMethod()]
        //public void AssemblyTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Assembly = expected;
        //    actual = target.Assembly;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for CacheByPage
        /////</summary>
        //[TestMethod()]
        //public void CacheByPageTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.CacheByPage = expected;
        //    actual = target.CacheByPage;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for CachePersonalized
        /////</summary>
        //[TestMethod()]
        //public void CachePersonalizedTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.CachePersonalized = expected;
        //    actual = target.CachePersonalized;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.Id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Name
        /////</summary>
        //[TestMethod()]
        //public void NameTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Name = expected;
        //    actual = target.Name;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Properties
        /////</summary>
        //[TestMethod()]
        //public void PropertiesTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    MacroProperty[] actual;
        //    actual = target.Properties;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RefreshRate
        /////</summary>
        //[TestMethod()]
        //public void RefreshRateTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.RefreshRate = expected;
        //    actual = target.RefreshRate;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for RenderContent
        /////</summary>
        //[TestMethod()]
        //public void RenderContentTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.RenderContent = expected;
        //    actual = target.RenderContent;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ScriptingFile
        /////</summary>
        //[TestMethod()]
        //public void ScriptingFileTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.ScriptingFile = expected;
        //    actual = target.ScriptingFile;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Type
        /////</summary>
        //[TestMethod()]
        //public void TypeTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Type = expected;
        //    actual = target.Type;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for UseInEditor
        /////</summary>
        //[TestMethod()]
        //public void UseInEditorTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.UseInEditor = expected;
        //    actual = target.UseInEditor;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Xslt
        /////</summary>
        //[TestMethod()]
        //public void XsltTest()
        //{
        //    Macro target = new Macro(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Xslt = expected;
        //    actual = target.Xslt;
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
