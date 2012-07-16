using umbraco.cms.businesslogic.language;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using System.Linq;
using System;
using System.Globalization;
using umbraco.DataLayer;
using umbraco.cms.businesslogic.web;
using System.Data;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for LanguageTest and is intended
    ///to contain all LanguageTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LanguageTest
    {

        /// <summary>
        /// A test to ensure you cannot delete the default language: en-US
        /// </summary>
        [ExpectedException(typeof(InvalidOperationException))]
        [TestMethod()]
        public void Language_Delete_Default_Language()
        {
            var lang = Language.GetByCultureCode("en-US");
            lang.Delete();
        }

        /// <summary>
        ///A test for getAll
        ///</summary>
        [TestMethod()]
        public void Language_GetAll()
        {
            //check with sql that it's the correct number of children
            var ids = new List<int>();
            using (var reader = Application.SqlHelper.ExecuteReader(Language.m_SQLOptimizedGetAll))
            {
                while (reader.Read())
                {
                    ids.Add(Convert.ToInt32(reader.GetShort("id")));
                }
            }

            var all = Language.GetAllAsList();

            Assert.AreEqual<int>(ids.Distinct().Count(), all.Count());
        }

        /// <summary>
        ///A test for ToXml
        ///</summary>
        [TestMethod()]
        public void Language_To_Xml()
        {
            var all = Language.GetAllAsList();

            XmlDocument xd = new XmlDocument();
            var x = all.First().ToXml(xd);

            Assert.IsNotNull(x.Attributes["Id"].Value);
            Assert.IsNotNull(x.Attributes["CultureAlias"].Value);
            Assert.IsNotNull(x.Attributes["FriendlyName"].Value);
        }

        /// <summary>
        ///A test for GetByCultureCode
        ///</summary>
        [TestMethod()]
        public void Language_Get_By_Culture_Code()
        {
            var all = Language.GetAllAsList();
            var lang = Language.GetByCultureCode(all.First().CultureAlias);
            Assert.AreEqual(all.First().CultureAlias, lang.CultureAlias);
        }

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void Language_Make_New()
        {
            var newLang = MakeNew();                             
            DeleteLanguage(newLang);
        }

        /// <summary>
        /// try to make a duplicate, this should fail with an sql exception
        /// </summary>
        [TestMethod()]
        [ExpectedException(typeof(SqlHelperException))]
        public void Language_Make_Duplicate()
        {
            var all = Language.GetAllAsList();
            Language.MakeNew(all.First().CultureAlias);
        }

        [TestMethod()]
        public void Language_Delete_With_Assigned_Domain()
        {
            var newLang = MakeNew();            

            var newDoc = DocumentTest.CreateNewUnderRoot(DocumentTest.GetExistingDocType());

            Domain.MakeNew("www.test" + Guid.NewGuid().ToString("N") + ".com", newDoc.Id, newLang.id);

            //this shouldn't delete it
            bool hasErr = false;
            try
            {
                newLang.Delete();
            }
            catch (DataException)
            {
                hasErr = true;
            }
            Assert.IsTrue(hasErr);            

            //we will need to delete the domain first, then the language
            var d = Domain.GetDomainsById(newDoc.Id).First();
            d.Delete();

            DeleteLanguage(newLang);
        }

        /// <summary>
        /// Ensure that a language that has dictionary items assigned to it with values
        /// is able to be deleted propery. Ensure that all translations for the language are 
        /// removed as well.
        /// </summary>
        [TestMethod()]
        public void Language_Delete_With_Assigned_Dictionary_Items() 
        {
            var newLang = MakeNew();



            DeleteLanguage(newLang);
        }

        #region Tests to write
        ///// <summary>
        /////A test for id
        /////</summary>
        //[TestMethod()]
        //public void idTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for FriendlyName
        /////</summary>
        //[TestMethod()]
        //public void FriendlyNameTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id); // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.FriendlyName;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for CultureAlias
        /////</summary>
        //[TestMethod()]
        //public void CultureAliasTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.CultureAlias = expected;
        //    actual = target.CultureAlias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest()
        //{
        //    XmlNode xmlData = null; // TODO: Initialize to an appropriate value
        //    Language expected = null; // TODO: Initialize to an appropriate value
        //    Language actual;
        //    actual = Language.Import(xmlData);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

       

        ///// <summary>
        /////A test for Delete
        /////</summary>
        //[TestMethod()]
        //public void DeleteTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id); // TODO: Initialize to an appropriate value
        //    target.Delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Language Constructor
        /////</summary>
        //[TestMethod()]
        //public void LanguageConstructorTest()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    Language target = new Language(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //} 
        #endregion

        #region Private methods

        /// <summary>
        /// a helper class to create a new language and validate everything. This does NOT delete it when the test is done.
        /// </summary>
        /// <returns></returns>
        private Language MakeNew()
        {             
            var all = Language.GetAllAsList();

            //get all cultures not installed
            var nonInstalled = CultureInfo.GetCultures(CultureTypes.AllCultures)
                .Select(x => x.Name)
                .Except(all.Select(x => x.CultureAlias))
                .ToList();

            Language.MakeNew(nonInstalled.First());

            //now get all installed again to make sure it's there
            var newAll = Language.GetAllAsList();

            //the new counts should be different
            Assert.AreNotEqual<int>(all.Count(), newAll.Count());
            //the differnce should be 1
            Assert.AreEqual<int>(1, newAll.Except(all).Count());

            //now we need to delete
            var newLang = newAll.Except(all).Single();
            return newLang;        
        }

        private void DeleteLanguage(Language lang)
        {
            var id = lang.id;

            lang.Delete();

            //check with sql that it is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoLanguage WHERE id=@id",
                Application.SqlHelper.CreateParameter("@id", id));

            Assert.AreEqual(0, count);
        } 
        #endregion

        #region Intitialize and cleanup
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
