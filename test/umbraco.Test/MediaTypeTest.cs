using umbraco.cms.businesslogic.media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using System.Collections.Generic;
using System.Linq;
using umbraco.cms.businesslogic.datatype;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for MediaTypeTest and is intended
    ///to contain all MediaTypeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MediaTypeTest
    {
        /// <summary>
        ///A test for GetAll
        ///</summary>
        [TestMethod()]
        public void MediaType_Get_All()
        {
            //check with sql that it's the correct number of children
            var ids = new List<int>();
            using (var reader = Application.SqlHelper.ExecuteReader(MediaType.m_SQLOptimizedGetAll.Trim(),
                Application.SqlHelper.CreateParameter("@nodeObjectType", MediaType._objectType)))
            {
                while (reader.Read())
                {
                    ids.Add(reader.Get<int>("id"));
                }
            }

            var all = MediaType.GetAllAsList();

            Assert.AreEqual<int>(ids.Distinct().Count(), all.Count());
        }

        /// <summary>
        /// This will create 3 media types, and create nodes in the following structure:
        /// - root
        /// -- node1 (of media type #1)
        /// --- node 2 (of media type #2)
        /// ---- node 3 (of media type #1)
        /// ----- node 4 (of media type #3)
        /// 
        /// Then we'll delete media type #1. The result should be that node1 and node3 are completely deleted from the database and node2 and node4 are
        /// moved to the recycle bin.
        /// </summary>
        [TestMethod()]
        public void MediaType_Delete_Media_Type_With_Media_And_Children_Of_Diff_Media_Types()
        {
            //System.Diagnostics.Debugger.Break();

            //create the doc types 
            var mt1 = CreateNewMediaType();
            var mt2 = CreateNewMediaType();
            var mt3 = CreateNewMediaType();

            //create the heirarchy
            mt1.AllowedChildContentTypeIDs = new int[] { mt2.Id, mt3.Id };
            mt1.Save();
            mt2.AllowedChildContentTypeIDs = new int[] { mt1.Id };
            mt2.Save();

            //create the content tree
            var node1 = Media.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt1, m_User, -1);
            var node2 = Media.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt2, m_User, node1.Id);
            var node3 = Media.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt1, m_User, node2.Id);
            var node4 = Media.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt3, m_User, node3.Id);

            //do the deletion of doc type #1
            DeleteMediaType(mt1);

            //do our checks
            Assert.IsFalse(Media.IsNode(node1.Id), "node1 is not deleted"); //this was of doc type 1, should be gone
            Assert.IsFalse(Media.IsNode(node3.Id), "node3 is not deleted"); //this was of doc type 1, should be gone

            Assert.IsTrue(Media.IsNode(node2.Id), "node2 is deleted");
            Assert.IsTrue(Media.IsNode(node4.Id), "node4 is deleted");

            node2 = new Media(node2.Id);//need to re-query the node
            Assert.IsTrue(node2.IsTrashed, "node2 is not in the trash");
            node4 = new Media(node4.Id); //need to re-query the node
            Assert.IsTrue(node4.IsTrashed, "node 4 is not in the trash");

            //remove the old data
            DeleteMediaType(mt2);
            DeleteMediaType(mt3);

        }

        /// <summary>
        /// Tests adding every type of property to a new media type on a new tab, then delete the tab, then the media type
        /// </summary>
        [TestMethod()]
        public void MediaType_Add_Properties_To_Tab_Then_Delete_It()
        {
            //System.Diagnostics.Debugger.Break();

            //allow itself to be created under itself
            m_NewMediaType.AllowedChildContentTypeIDs = new int[] { m_NewMediaType.Id };
            //create a tab 
            m_NewMediaType.AddVirtualTab("TEST");

            //test the tab
            var tabs = m_NewMediaType.getVirtualTabs.ToList();
            Assert.AreEqual(1, tabs.Count);

            //create a property
            var allDataTypes = DataTypeDefinition.GetAll().ToList(); //get all definitions
            var i = 0;
            foreach (var dataType in allDataTypes)
            {
                //add a property type of the first type found in the list
                m_NewMediaType.AddPropertyType(dataType, "testProperty" + (++i).ToString(), "Test Property" + i.ToString());
                //test the prop
                var prop = m_NewMediaType.getPropertyType("testProperty" + i.ToString());
                Assert.IsTrue(prop.Id > 0);
                Assert.AreEqual("Test Property" + i.ToString(), prop.Name);
                //put the properties to the tab
                m_NewMediaType.SetTabOnPropertyType(prop, tabs[0].Id);
                //re-get the property since data is cached in the object
                prop = m_NewMediaType.getPropertyType("testProperty" + i.ToString());
                Assert.AreEqual<int>(tabs[0].Id, prop.TabId);
            }

            //now we need to delete the tab
            m_NewMediaType.DeleteVirtualTab(tabs[0].Id);
        }

        #region Test to write
        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MediaType target = new MediaType(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest()
        //{
        //    User u = null; // TODO: Initialize to an appropriate value
        //    string Text = string.Empty; // TODO: Initialize to an appropriate value
        //    MediaType expected = null; // TODO: Initialize to an appropriate value
        //    MediaType actual;
        //    actual = MediaType.MakeNew(u, Text);
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
        //    MediaType expected = null; // TODO: Initialize to an appropriate value
        //    MediaType actual;
        //    actual = MediaType.GetByAlias(Alias);
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
        //    MediaType target = new MediaType(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for MediaType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MediaTypeConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    MediaType target = new MediaType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for MediaType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MediaTypeConstructorTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MediaType target = new MediaType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //} 
        #endregion

        #region Private properties and methods

        private User m_User = new User(0);

        /// <summary>
        /// before each test starts, this object is created so it can be used for testing.
        /// </summary>
        private MediaType m_NewMediaType;

        /// <summary>
        /// Create a brand new media type
        /// </summary>
        /// <returns></returns>
        private MediaType CreateNewMediaType()
        {
            var mt = MediaType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            Assert.IsTrue(mt.Id > 0);
            Assert.AreEqual(DateTime.Now.Date, mt.CreateDateTime.Date);
            return mt;
        }

        private void DeleteMediaType(MediaType mt)
        {
            var id = mt.Id;

            mt.delete();

            //check with sql that it is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id=@id",
                Application.SqlHelper.CreateParameter("@id", id));

            Assert.AreEqual(0, count);
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
            m_NewMediaType = CreateNewMediaType();
        }

        /// <summary>
        /// Remove the created document type
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            DeleteMediaType(m_NewMediaType);
        }
        #endregion

    }
}
