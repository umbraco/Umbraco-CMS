using umbraco.cms.businesslogic.task;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using System.Linq;
using umbraco.DataLayer;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for TaskTypeTest and is intended
    ///to contain all TaskTypeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TaskTypeTest
    {

        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void TaskType_Not_Found_Constructor1()
        {
            TaskType u = new TaskType(-1111);
        }

        /// <summary>
        /// Test the constructor to throw an exception when the object is not found by id
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ArgumentException))]
        public void TaskType_Not_Found_Constructor2()
        {
            TaskType u = new TaskType(Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Creates a new task type, creates some tasks with it, then deletes the type. ensure that all tasks associated are removed.
        /// </summary>
        [TestMethod()]
        public void TaskType_Make_New_Assign_Tasks_And_Remove()
        {
            TaskType tt = new TaskType();
            tt.Alias = Guid.NewGuid().ToString("N");
            tt.Save();
            Assert.IsTrue(tt.Id > 0);

            Task t = new Task();
            t.Comment = Guid.NewGuid().ToString("N");
            t.Node = Document.GetRootDocuments().First();
            t.ParentUser = m_User;
            t.User = m_User;
            t.Type = TaskType.GetAll().First();
            t.Save();

            //delete the task type
            tt.Delete();

            //ensure they're gone
            Assert.AreEqual(0, Task.GetTasksByType(tt.Id).Count);

            //ensure the type is gone
            Assert.AreEqual(0, TaskType.GetAll().Where(x => x.Id == tt.Id).Count());

        }

        /// <summary>
        /// Ensures that duplicate task type names are not allowed either by an update or an insert
        /// </summary>
        [TestMethod()]
        public void TaskType_Make_Duplicate()
        {
            var alias = Guid.NewGuid().ToString("N");

            var tt = new TaskType();
            tt.Alias = alias;
            tt.Save();

            //try to insert a duplicate
            var tt2 = new TaskType();
            tt2.Alias = alias;
            var hasException = false;
            try
            {
                tt2.Save();
            }
            catch (SqlHelperException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);

            //try to update to a duplicate
            var tt3 = new TaskType();
            tt3.Alias = Guid.NewGuid().ToString("N");
            tt3.Save();
            tt3.Alias = alias;
            hasException = false;
            try
            {
                tt3.Save();
            }
            catch (SqlHelperException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);

            //now remove the ones we've created
            tt.Delete();
            tt3.Delete();

        }

        private User m_User = new User(0);

        #region Tests to write

        

        ///// <summary>
        /////A test for TaskType Constructor
        /////</summary>
        //[TestMethod()]
        //public void TaskTypeConstructorTest1()
        //{
        //    string TypeAlias = string.Empty; // TODO: Initialize to an appropriate value
        //    TaskType target = new TaskType(TypeAlias);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for TaskType Constructor
        /////</summary>
        //[TestMethod()]
        //public void TaskTypeConstructorTest2()
        //{
        //    TaskType target = new TaskType();
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    IEnumerable<TaskType> expected = null; // TODO: Initialize to an appropriate value
        //    IEnumerable<TaskType> actual;
        //    actual = TaskType.GetAll();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Save
        /////</summary>
        //[TestMethod()]
        //public void SaveTest()
        //{
        //    TaskType target = new TaskType(); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Alias
        /////</summary>
        //[TestMethod()]
        //public void AliasTest()
        //{
        //    TaskType target = new TaskType(); // TODO: Initialize to an appropriate value
        //    string expected = string.Empty; // TODO: Initialize to an appropriate value
        //    string actual;
        //    target.Alias = expected;
        //    actual = target.Alias;
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Id
        /////</summary>
        //[TestMethod()]
        //public void IdTest()
        //{
        //    TaskType target = new TaskType(); // TODO: Initialize to an appropriate value
        //    int expected = 0; // TODO: Initialize to an appropriate value
        //    int actual;
        //    target.Id = expected;
        //    actual = target.Id;
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
