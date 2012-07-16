using umbraco.cms.businesslogic.media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using System.Linq;
using umbraco.editorControls.label;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.property;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    /// This will test the Media data layer.
    /// These test assume the following criteria, if this criteria is not met, these tests will fail:
    /// - There is a Label data type assigned to one of your Media types.
    ///</summary>
    [TestClass()]
    public class MediaTest
    {
        /// <summary>
        ///A test for making a new media and deleting it which actuall first moves it to the recycle bin
        ///and then deletes it.
        ///</summary>
        [TestMethod()]
        public void Media_Make_New()
        {
            //System.Diagnostics.Debugger.Break();
            Assert.IsInstanceOfType(m_NewRootMedia, typeof(Media));
        }

        [TestMethod()]
        public void Media_Update_Data()
        {
            //System.Diagnostics.Debugger.Break();
            
            //set the value of a text property
            var m = CreateNewUnderRoot(m_ExistingMediaType);
            var p = GetLabelProperty(m_ExistingMediaType, m);
            p.Value = "HELLO!";

            Assert.AreEqual("HELLO!", m.getProperty(p.PropertyType).Value);

            //completely delete
            m.delete(true);

            Assert.IsFalse(Media.IsNode(m.Id));
        }

        [TestMethod()]
        public void Media_Delete_Heirarchy_Permanently()
        {
            var mediaList = new List<Media>();
            var total = 20;
            var mt = new MediaType(GetExistingMediaTypeId());
            //allow the doc type to be created underneath itself
            mt.AllowedChildContentTypeIDs = new int[] { mt.Id };
            mt.Save();

            //create 20 content nodes underneath each other, this will test deleting with heirarchy as well
            var lastParentId = -1;
            for (var i = 0; i < total; i++)
            {
                var newDoc = Media.MakeNew(i.ToString() + Guid.NewGuid().ToString("N"), mt, m_User, lastParentId);
                mediaList.Add(newDoc);
                Assert.IsTrue(mediaList[mediaList.Count - 1].Id > 0);
                lastParentId = newDoc.Id;
            }

            //now delete all of them permanently, since they are nested, we only need to delete one
            mediaList.First().delete(true);

            //make sure they are all gone
            foreach (var d in mediaList)
            {
                Assert.IsFalse(Media.IsNode(d.Id));
            }

        }
        
        [TestMethod]
        public void Media_Move()
        {
            //first need to document type that allows other types of document types to exist underneath it
            MediaType parent = null;
            MediaType child = null;
            var ids = MediaType.getAllUniqueNodeIdsFromObjectType(MediaType._objectType);
            foreach (var id in ids)
            {
                var dt = new MediaType(id);
                var allowed = dt.AllowedChildContentTypeIDs.ToList();
                if (allowed.Count() > 0)
                {
                    parent = dt;
                    child = new MediaType(allowed[0]);
                    break;
                }
            }
            if (parent == null || child == null)
            {
                throw new NotImplementedException("The umbraco install doesn't have document types that support a heirarchy");
            }

            //now that we have a parent and a child, we need to create some documents
            var node1 = Media.MakeNew("FromCopy" + Guid.NewGuid().ToString("N"), parent, m_User, -1);
            Assert.IsTrue(node1.Id > 0);

            var node2 = Media.MakeNew("ToCopy" + Guid.NewGuid().ToString("N"), parent, m_User, -1);
            Assert.IsTrue(node2.Id > 0);

            //we now have 2 nodes in the root of the same type, we'll create a child node under node1 and move it to node2
            var childNode = Media.MakeNew("ChildCopy" + Guid.NewGuid().ToString("N"), child, m_User, node2.Id);
            Assert.IsTrue(childNode.Id > 0);

            childNode.Move(node2.Id);
            Assert.AreEqual(node2.Id, childNode.Parent.Id);

            RecycleAndDelete(childNode);
            RecycleAndDelete(node2);
            RecycleAndDelete(node1);
        }

        [TestMethod()]
        public void Media_Delete_All_Docs_By_Document_Type()
        {
            //System.Diagnostics.Debugger.Break();

            //create a new media type
            string name = "TEST-" + Guid.NewGuid().ToString("N");
            var mt = MediaType.MakeNew(m_User, name);

            //test the media type
            Assert.AreEqual(DateTime.Now.Date, mt.CreateDateTime.Date);
            Assert.IsTrue(mt.Id > 0);

            //allow itself to be created under itself
            mt.AllowedChildContentTypeIDs = new int[] { mt.Id };
            //create a tab 
            mt.AddVirtualTab("TEST");

            //test the tab
            var tabs = mt.getVirtualTabs.ToList();
            Assert.AreEqual(1, tabs.Count);

            //create a property
            var allDataTypes = DataTypeDefinition.GetAll().ToList(); //get all definitions
            mt.AddPropertyType(allDataTypes[0], "testProperty", "Test Property"); //add a property type of the first type found in the list

            //test the prop
            var prop = mt.getPropertyType("testProperty");
            Assert.AreEqual("Test Property", prop.Name);

            //create 1st node
            var node1 = Media.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), mt, m_User, -1);
            Assert.IsTrue(node1.Id > 0);

            //create 2nd node underneath node 1
            var node2 = Media.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), mt, m_User, node1.Id);
            Assert.IsTrue(node2.Id > 0);
            Assert.AreEqual(node1.Id, node2.Parent.Id);

            //create 3rd node underneath node 2
            var node3 = Media.MakeNew("TEST-" + Guid.NewGuid().ToString("N"), mt, m_User, node2.Id);
            Assert.IsTrue(node3.Id > 0);
            Assert.AreEqual(node2.Id, node3.Parent.Id);

            Media.DeleteFromType(mt);

            Assert.IsFalse(Media.IsNode(node1.Id));
            Assert.IsFalse(Media.IsNode(node2.Id));
            Assert.IsFalse(Media.IsNode(node3.Id));

            //now remove the document type created
            mt.delete();

            Assert.IsFalse(MediaType.IsNode(mt.Id));
        }

        [TestMethod]
        public void Media_Empty_Recycle_Bin()
        {
            //System.Diagnostics.Debugger.Break();

            var mediaList = new List<Media>();
            var total = 20;
            var mt = m_ExistingMediaType;
            //allow the doc type to be created underneath itself
            mt.AllowedChildContentTypeIDs = new int[] { mt.Id };
            mt.Save();

            //create 20 media nodes underneath each other, this will test deleting with heirarchy as well
            var lastParentId = -1;
            for (var i = 0; i < total; i++)
            {
                var newMedia = Media.MakeNew("R-" + i.ToString() + Guid.NewGuid().ToString("N"), mt, m_User, lastParentId);
                mediaList.Add(newMedia);
                Assert.IsTrue(mediaList[mediaList.Count - 1].Id > 0);
                Assert.AreEqual(lastParentId, newMedia.ParentId);
                lastParentId = newMedia.Id;
            }

            //now delete all of them, since they are nested, we only need to delete one
            mediaList.First().delete();

            //a callback action for each item removed from the recycle bin
            var totalDeleted = 0;

            var bin = new RecycleBin(RecycleBin.RecycleBinType.Media);
            var totalTrashedItems = bin.GetDescendants().Cast<object>().Count();
            bin.CallTheGarbageMan(x =>
            {
                Assert.AreEqual(totalTrashedItems - (++totalDeleted), x);
            });

            Assert.AreEqual(0, RecycleBin.Count(RecycleBin.RecycleBinType.Media));
        }

        [TestMethod]
        public void Media_Undelete()
        {
            //find existing content
            var media = new Media(GetExistingNodeId());
            //create new content based on the existing content in the same heirarchy
            var mt = new MediaType(media.ContentType.Id);
            var parentId = media.ParentId;
            var newMedia = Media.MakeNew("NewMedia" + Guid.NewGuid().ToString("N"), mt, m_User, parentId);
            Assert.IsTrue(newMedia.Id > 0);

            //this will recycle the node
            newMedia.delete();
            Assert.IsTrue(newMedia.IsTrashed);
            Assert.IsTrue(newMedia.Path.Contains("," + (int)RecycleBin.RecycleBinType.Media + ","));

            //undelete the node (move it)
            newMedia.Move(parentId);
            Assert.IsFalse(newMedia.IsTrashed);
            Assert.IsFalse(newMedia.Path.Contains("," + (int)RecycleBin.RecycleBinType.Media + ","));

            //remove it completely
            RecycleAndDelete(newMedia);
        }

        #region Tests to write

        ///// <summary>
        /////A test for Children
        /////</summary>
        //[TestMethod()]
        //public void ChildrenTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Media target = new Media(id); // TODO: Initialize to an appropriate value
        //    Media[] actual;
        //    actual = target.Children;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Media target = new Media(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}      

        ///// <summary>
        /////A test for GetRootMedias
        /////</summary>
        //[TestMethod()]
        //public void GetRootMediasTest()
        //{
        //    Media[] expected = null; // TODO: Initialize to an appropriate value
        //    Media[] actual;
        //    actual = Media.GetRootMedias();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetChildrenForTree
        /////</summary>
        //[TestMethod()]
        //public void GetChildrenForTreeTest()
        //{
        //    int nodeId = 0; // TODO: Initialize to an appropriate value
        //    List<Media> expected = null; // TODO: Initialize to an appropriate value
        //    List<Media> actual;
        //    actual = Media.GetChildrenForTree(nodeId);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for DeleteFromType
        /////</summary>
        //[TestMethod()]
        //public void DeleteFromTypeTest()
        //{
        //    MediaType dt = null; // TODO: Initialize to an appropriate value
        //    Media.DeleteFromType(dt);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    Media target = new Media(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}
        #endregion

        #region Private properties and methods

        /// <summary>
        /// The user to be used to create stuff
        /// </summary>
        private User m_User = new User(0);

        /// <summary>
        /// Used for each test initialization. Before each test is run a new root media is created.
        /// </summary>
        private Media m_NewRootMedia;

        /// <summary>
        /// Returns a label property of the document type specified. This will throw an exception if one is not found.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private Property GetLabelProperty(MediaType mt, Media m)
        {
            DataTypeNoEdit lblField = new DataTypeNoEdit();
            var prop = mt.PropertyTypes
                    .Where(x => x.DataTypeDefinition.DataType.Id == lblField.Id).First();
            return m.GenericProperties.Where(x => x.PropertyType.Id == prop.Id).First();
        }

        private Property GetUploadProperty(MediaType mt, Media m)
        {
            DataTypeNoEdit lblField = new DataTypeNoEdit();
            var prop = mt.PropertyTypes
                    .Where(x => x.DataTypeDefinition.DataType.Id == lblField.Id).First();
            return m.GenericProperties.Where(x => x.PropertyType.Id == prop.Id).First();
        }

        /// <summary>
        /// Gets initialized for each test and is set to an existing document type
        /// </summary>
        private MediaType m_ExistingMediaType;

        private void RecycleAndDelete(Media m)
        {
            if (m == null)
            {
                return;
            }

            var id = m.Id;

            //check if it is already trashed
            var alreadyTrashed = m.IsTrashed;

            if (!alreadyTrashed)
            {
                //now recycle it
                m.delete();

                Assert.IsTrue(m.IsTrashed);
            }

            //now permanently delete
            m.delete(true);
            Assert.IsFalse(Media.IsNode(id));

            //check with sql that it is gone
            var count = Application.SqlHelper.ExecuteScalar<int>("SELECT COUNT(*) FROM umbracoNode WHERE id=@id",
                Application.SqlHelper.CreateParameter("@id", id));

            Assert.AreEqual(0, count);
        }

        private int GetExistingNodeId()
        {
            var ids = Media.getAllUniqueNodeIdsFromObjectType(Media._objectType).ToList();
            var r = new Random();
            var index = r.Next(0, ids.Count() - 1);
            return ids[index];
        }

        private Media CreateNewUnderRoot(MediaType mt)
        {
            string Name = "TEST-" + Guid.NewGuid().ToString("N");
            int ParentId = -1;
            Media actual = Media.MakeNew(Name, mt, m_User, ParentId);
            var id = actual.Id;
            Assert.IsTrue(actual.Id > 0);
            return actual;
        }

        private MediaType GetExistingDocType()
        {
            MediaType dct = new MediaType(GetExistingMediaTypeId());
            Assert.IsTrue(dct.Id > 0);
            return dct;
        }

        private MediaType GetExistingMediaType()
        {
            MediaType dct = new MediaType(GetExistingMediaTypeId());
            Assert.IsTrue(dct.Id > 0);
            return dct;
        }

        private int GetExistingMediaTypeId()
        {
            var types = MediaType.GetAll.ToList();
            MediaType found = null;
            DataTypeNoEdit lblField = new DataTypeNoEdit();
            foreach (var d in types)
            {
                var prop = d.PropertyTypes
                    .Where(x => x.DataTypeDefinition.DataType.Id == lblField.Id).FirstOrDefault();
                if (prop != null)
                {
                    found = d;
                    break;
                }
            }
            if (found == null)
            {
                throw new MissingMemberException("No media type was found that contains a label property");
            }
            return found.Id;
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
            m_ExistingMediaType = GetExistingMediaType();
            m_NewRootMedia = CreateNewUnderRoot(m_ExistingMediaType);
        }

        /// <summary>
        /// Makes sure the root doc is deleted
        /// </summary>
        [TestCleanup()]
        public void MyTestCleanup()
        {
            RecycleAndDelete(m_NewRootMedia);
        }
        #endregion

        
    }
}
