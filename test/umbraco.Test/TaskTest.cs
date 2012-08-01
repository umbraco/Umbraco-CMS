using umbraco.cms.businesslogic.task;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using System.Linq;
using umbraco.cms.businesslogic.web;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for TaskTest and is intended
    ///to contain all TaskTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TaskTest
    {


        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void Task_Not_Found_Constructor()
        {
            Task u = new Task(-1111);
        }

        /// <summary>
        /// Create a new task and then close it (then deletes it)
        /// </summary>
        [TestMethod()]
        public void Task_Make_New_And_Close()
        {   
            //create the task
            Task t = new Task();
            t.Comment = Guid.NewGuid().ToString("N");
            t.Node = Document.GetRootDocuments().First();
            t.ParentUser = m_User;
            t.User = m_User;
            t.Type = TaskType.GetAll().First();
            t.Save();

            Assert.IsTrue(t.Id > 0);

            t.Closed = true;
            t.Save();
            Assert.IsTrue(t.Closed);

            //re-get the task and make sure the props have been persisted to the db
            var reGet = new Task(t.Id);
            Assert.IsTrue(reGet.Closed);

            reGet.Delete();
            //re-get the task and make sure it is gone
            var isFound = true;
            try
            {
                var gone = new Task(t.Id);
            }
            catch (ArgumentException)
            {
                isFound = false;
            }
            Assert.IsFalse(isFound);
            
        }

        [TestMethod()]
        public void Task_Assign_To_New_Node_Delete_Node_And_Ensure_Tasks_Removed()
        {
            //create a new document in the root
            var dt = DocumentType.GetAllAsList().First();
            Document d = Document.MakeNew(Guid.NewGuid().ToString("N"), dt, m_User, -1);

            //create a new task assigned to the new document
            Task t = new Task();
            t.Comment = Guid.NewGuid().ToString("N");
            t.Node = d;
            t.ParentUser = m_User;
            t.User = m_User;
            t.Type = TaskType.GetAll().First();
            t.Save();

            //delete the document permanently
            d.delete(true);
            
            //ensure the task is gone
            var isFound = true;
            try
            {
                var gone = new Task(t.Id);
            }
            catch (ArgumentException)
            {
                isFound = false;
            }
            Assert.IsFalse(isFound);

            //ensure it's gone
            Assert.IsFalse(Document.IsNode(d.Id));
        }

        private User m_User = new User(0);

        #region Tests to write
        
        ///// <summary>
        /////A test for Task Constructor
        /////</summary>
        //[TestMethod()]
        //public void TaskConstructorTest1()
        //{
        //    Task target = new Task();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for GetOwnedTasks
        /////</summary>
        //[TestMethod()]
        //public void GetOwnedTasksTest()
        //{
        //    User User = null; // TODO: Initialize to an appropriate value
        //    bool IncludeClosed = false; // TODO: Initialize to an appropriate value
        //    Tasks expected = null; // TODO: Initialize to an appropriate value
        //    Tasks actual;
        //    actual = Task.GetOwnedTasks(User, IncludeClosed);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetTasks
        /////</summary>
        //[TestMethod()]
        //public void GetTasksTest()
        //{
        //    User User = null; // TODO: Initialize to an appropriate value
        //    bool IncludeClosed = false; // TODO: Initialize to an appropriate value
        //    Tasks expected = null; // TODO: Initialize to an appropriate value
        //    Tasks actual;
        //    actual = Task.GetTasks(User, IncludeClosed);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Closed
        /////</summary>
        //[TestMethod()]
        //public void ClosedTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    target.Closed = expected;
        //    actual = target.Closed;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Comment
        /////</summary>
        //[TestMethod()]
        //public void CommentTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Comment = expected;
        //    actual = target.Comment;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Date
        /////</summary>
        //[TestMethod()]
        //public void DateTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    DateTime expected = new DateTime(); // TODO: Initialize to an appropriate value
        //    DateTime actual;
        //    target.Date = expected;
        //    actual = target.Date;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.Id = expected;
        //    actual = target.Id;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Node
        /////</summary>
        //[TestMethod()]
        //public void NodeTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    CMSNode expected = null; // TODO: Initialize to an appropriate value
        //    CMSNode actual;
        //    target.Node = expected;
        //    actual = target.Node;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for ParentUser
        /////</summary>
        //[TestMethod()]
        //public void ParentUserTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    User expected = null; // TODO: Initialize to an appropriate value
        //    User actual;
        //    target.ParentUser = expected;
        //    actual = target.ParentUser;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Type
        /////</summary>
        //[TestMethod()]
        //public void TypeTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    TaskType expected = null; // TODO: Initialize to an appropriate value
        //    TaskType actual;
        //    target.Type = expected;
        //    actual = target.Type;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for User
        /////</summary>
        //[TestMethod()]
        //public void UserTest()
        //{
        //    Task target = new Task(); // TODO: Initialize to an appropriate value
        //    User expected = null; // TODO: Initialize to an appropriate value
        //    User actual;
        //    target.User = expected;
        //    actual = target.User;
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
