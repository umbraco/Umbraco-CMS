using umbraco.cms.businesslogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Xml;
using umbraco.cms.businesslogic.language;
using umbraco.BusinessLogic;
using System.Linq;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for Dictionary_DictionaryItemTest and is intended
    ///to contain all Dictionary_DictionaryItemTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DictionaryTest
    {
        [TestMethod()]
        public void Dictionary_Get_Top_Level_Items()
        {
            var items = Dictionary.getTopMostItems;

            var d = CreateNew();

            Assert.AreEqual<int>(items.Count() + 1, Dictionary.getTopMostItems.Count());

            DeleteItem(d);
        }

        /// <summary>
        /// Creates a new dictionary entry, adds values for all languages assigned, then deletes the 
        /// entry and ensure that all other data is gone too.
        ///</summary>
        [TestMethod()]
        public void Dictionary_Create_Add_Text_And_Delete()
        {
            var d = CreateNew();

            //set the values for all languages
            var langs = Language.GetAllAsList();
            foreach (var l in langs)
            {
                var val = "TEST" + Guid.NewGuid().ToString("N");
                d.setValue(l.id, val);
                //make sure the values are there
                Assert.AreEqual(val, d.Value(l.id));
            }

            DeleteItem(d);

        }

        /// <summary>
        ///A test for IsTopMostItem
        ///</summary>
        [TestMethod()]
        public void Dictionary_IsTopMostItem()
        {
            var parent = CreateNew();

            //create a child
            var childId = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "", parent.key);
            Assert.IsTrue(childId > 0);
            var child = new Dictionary.DictionaryItem(childId);
            Assert.IsInstanceOfType(child, typeof(Dictionary.DictionaryItem));

            Assert.IsTrue(parent.IsTopMostItem());
            Assert.IsFalse(child.IsTopMostItem());

            DeleteItem(child);
            DeleteItem(parent);
        }

        /// <summary>
        /// Test the Parent and Children properties and ensures that the relationships work both ways
        ///</summary>
        [TestMethod()]
        public void Dictionary_Parent_Child_Relationship()
        {
            var parent = CreateNew();

            //create a child
            var childId = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "", parent.key);
            Assert.IsTrue(childId > 0);
            var child = new Dictionary.DictionaryItem(childId);
            Assert.IsInstanceOfType(child, typeof(Dictionary.DictionaryItem));

            //set the parent relationship
            Assert.AreEqual(parent.id, child.Parent.id);
            Assert.AreEqual(parent.key, child.Parent.key);
            Assert.AreEqual(parent.UniqueId, child.Parent.UniqueId);

            //test the child relationship
            Assert.IsTrue(parent.hasChildren);
            Assert.AreEqual<int>(1, parent.Children.Length);
            Assert.AreEqual(child.id, parent.Children.First().id);
            Assert.AreEqual(child.key, parent.Children.First().key);
            Assert.AreEqual(child.UniqueId, parent.Children.First().UniqueId);

            DeleteItem(child);
            DeleteItem(parent);
        }

        /// <summary>
        /// Deletes a parent with existing children and ensures they are all gone.
        /// </summary>
        [TestMethod()]
        public void Dictionary_Delete_Parent_With_Children()
        {
            var parent = CreateNew();

            //create a child
            var childId1 = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "", parent.key);
            Assert.IsTrue(childId1 > 0);
            var child1 = new Dictionary.DictionaryItem(childId1);
            Assert.IsInstanceOfType(child1, typeof(Dictionary.DictionaryItem));

            //create a child
            var childId2 = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "", parent.key);
            Assert.IsTrue(childId2 > 0);
            var child2 = new Dictionary.DictionaryItem(childId2);
            Assert.IsInstanceOfType(child2, typeof(Dictionary.DictionaryItem));

            Assert.IsTrue(parent.hasChildren);
            Assert.AreEqual<int>(2, parent.Children.Length);


            DeleteItem(parent);

            //make sure kids are gone
            var notFound = false;
            try
            {
                var check = new Dictionary.DictionaryItem(childId1);
            }
            catch (ArgumentException)
            {
                notFound = true;
            }
            Assert.IsTrue(notFound);

            notFound = false;
            try
            {
                var check = new Dictionary.DictionaryItem(childId2);
            }
            catch (ArgumentException)
            {
                notFound = true;
            }
            Assert.IsTrue(notFound);

        }

        /// <summary>
        /// Guid constructor test
        ///</summary>
        [TestMethod()]
        public void Dictionary_Contructor_Guid()
        {
            var d = CreateNew();

            var same = new Dictionary.DictionaryItem(d.UniqueId);

            Assert.AreEqual(d.id, same.id);
            Assert.AreEqual(d.key, same.key);
            Assert.AreEqual(d.UniqueId, same.UniqueId);

            DeleteItem(d);
        }

        /// <summary>
        /// key constructor test
        /// </summary>
        [TestMethod()]
        public void Dictionary_Contructor_Key()
        {
            var d = CreateNew();

            var same = new Dictionary.DictionaryItem(d.key);

            Assert.AreEqual(d.id, same.id);
            Assert.AreEqual(d.key, same.key);
            Assert.AreEqual(d.UniqueId, same.UniqueId);

            DeleteItem(d);
        }

        /// <summary>
        ///A test for ToXml
        ///</summary>
        [TestMethod()]
        public void Dictionary_ToXml()
        {
            var d = CreateNew();

            //create a child
            var childId = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "", d.key);
            Assert.IsTrue(childId > 0);
            var child = new Dictionary.DictionaryItem(childId);
            Assert.IsInstanceOfType(child, typeof(Dictionary.DictionaryItem));
            
            var xml = new XmlDocument();

            var output = d.ToXml(xml);

            Assert.AreEqual("DictionaryItem", output.Name);
            Assert.IsTrue(output.HasChildNodes);
            Assert.IsNotNull(output.Attributes["Key"].Value);
            Assert.AreEqual(1, output.ChildNodes.OfType<XmlElement>().Where(x => x.Name == "DictionaryItem").Count());

            DeleteItem(child);
            DeleteItem(d);
        }

        /// <summary>
        ///A test to change the key of an element
        ///</summary>
        [TestMethod()]
        public void Dictionary_Change_Key()
        {
            //System.Diagnostics.Debugger.Break();
            
            var d = CreateNew();

            var oldKey = d.key;
            var newKey = "NEWKEY" + Guid.NewGuid().ToString("N");
            
            d.key = newKey;
            Assert.AreNotEqual(oldKey, d.key);
            Assert.AreEqual(newKey, d.key);

            //check with sql that the key is definitely changed
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsDictionary WHERE [key]=@key",
                Application.SqlHelper.CreateParameter("@key", newKey));
            Assert.AreEqual(1, count);
            
            count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsDictionary WHERE [key]=@key",
                Application.SqlHelper.CreateParameter("@key", oldKey));
            Assert.AreEqual(0, count);

            DeleteItem(d);
        }

        /// <summary>
        /// Tries to create a duplicate key and ensures it's not possible.
        /// </summary>
        [TestMethod()]
        public void Dictionary_Attempt_Duplicate_Key()
        {
            var key = "Test" + Guid.NewGuid().ToString("N");
            var d1Id = Dictionary.DictionaryItem.addKey(key, "");
            Assert.IsTrue(d1Id > 0);
            var d1 = new Dictionary.DictionaryItem(d1Id);
            Assert.IsInstanceOfType(d1, typeof(Dictionary.DictionaryItem));

            var alreadyExists = false;
            try
            {
                var d2Id = Dictionary.DictionaryItem.addKey(key, "");
                Assert.IsTrue(d2Id > 0);
                var d2 = new Dictionary.DictionaryItem(d2Id);
            }
            catch (ArgumentException)
            {
                alreadyExists = true;
            }
            Assert.IsTrue(alreadyExists);

            DeleteItem(d1);
        }

        #region Private methods
        private Dictionary.DictionaryItem CreateNew()
        {
            var id = Dictionary.DictionaryItem.addKey("Test" + Guid.NewGuid().ToString("N"), "");
            Assert.IsTrue(id > 0);

            var d = new Dictionary.DictionaryItem(id);
            Assert.IsInstanceOfType(d, typeof(Dictionary.DictionaryItem));

            return d;
        }

        private void DeleteItem(Dictionary.DictionaryItem d)
        {
            var id = d.id;

            d.delete();

            var notFound = false;
            try
            {
                var check = new Dictionary.DictionaryItem(id);
            }
            catch (ArgumentException)
            {
                notFound = true;
            }
            Assert.IsTrue(notFound, "The item with key " + d.key + " still exists!");

            //check with sql that the language text is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM cmsLanguageText WHERE uniqueId=@uniqueId",
                Application.SqlHelper.CreateParameter("@uniqueId", d.UniqueId));
            Assert.AreEqual(0, count);
        }
        #endregion

        #region Tests to write
       

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest()
        //{
        //    XmlNode xmlData = null; // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem parent = null; // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem expected = null; // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem actual;
        //    actual = Dictionary.DictionaryItem.Import(xmlData, parent);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Import
        /////</summary>
        //[TestMethod()]
        //public void ImportTest1()
        //{
        //    XmlNode xmlData = null; // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem expected = null; // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem actual;
        //    actual = Dictionary.DictionaryItem.Import(xmlData);
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
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        

        ///// <summary>
        /////A test for Value
        /////</summary>
        //[TestMethod()]
        //public void ValueTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    int languageId = 0; // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Value(languageId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Value
        /////</summary>
        //[TestMethod()]
        //public void ValueTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    actual = target.Value();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for addKey
        /////</summary>
        //[TestMethod()]
        //public void addKeyTest()
        //{
        //    string key = string.Empty; // TODO: Initialize to an appropriate value
        //    string defaultValue = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Dictionary.DictionaryItem.addKey(key, defaultValue);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for addKey
        /////</summary>
        //[TestMethod()]
        //public void addKeyTest1()
        //{
        //    string key = string.Empty; // TODO: Initialize to an appropriate value
        //    string defaultValue = string.Empty; // TODO: Initialize to an appropriate value
        //    string parentKey = string.Empty; // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = Dictionary.DictionaryItem.addKey(key, defaultValue, parentKey);
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
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for hasKey
        /////</summary>
        //[TestMethod()]
        //public void hasKeyTest()
        //{
        //    string key = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = Dictionary.DictionaryItem.hasKey(key);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for setValue
        /////</summary>
        //[TestMethod()]
        //public void setValueTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    int languageId = 0; // TODO: Initialize to an appropriate value
        //    string value = string.Empty; // TODO: Initialize to an appropriate value
        //    target.setValue(languageId, value);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for setValue
        /////</summary>
        //[TestMethod()]
        //public void setValueTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    string value = string.Empty; // TODO: Initialize to an appropriate value
        //    target.setValue(value);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Children
        /////</summary>
        //[TestMethod()]
        //public void ChildrenTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem[] actual;
        //    actual = target.Children;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Parent
        /////</summary>
        //[TestMethod()]
        //public void ParentTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem actual;
        //    actual = target.Parent;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for hasChildren
        /////</summary>
        //[TestMethod()]
        //public void hasChildrenTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.hasChildren;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for id
        /////</summary>
        //[TestMethod()]
        //public void idTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Dictionary.DictionaryItem target = new Dictionary.DictionaryItem(id); // TODO: Initialize to an appropriate value
        //    int actual;
        //    actual = target.id;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        
        #endregion

        #region Initialize and Cleanup
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
