using umbraco.cms.businesslogic.member;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.propertytype;
using System.Xml;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for MemberTypeTest and is intended
    ///to contain all MemberTypeTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MemberTypeTest
    {

        /// <summary>
        ///A test for MakeNew
        ///</summary>
        [TestMethod()]
        public void MemberType_Make_New()
        {
            var m = MemberType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            
            Assert.IsInstanceOfType(m, typeof(MemberType));
            Assert.IsTrue(m.Id > 0);

            //remove it
            m.delete();
            Assert.IsFalse(MemberType.IsNode(m.Id));
        }

        /// <summary>
        /// Create a member type, create some members of the member type and then delete the member type.
        /// This should also delete all of the members.
        ///</summary>
        [TestMethod()]
        public void MemberType_Delete_With_Assigned_Members()
        {
            
            var mt1 = MemberType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));

            //create the members
            var node1 = Member.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt1, m_User);
            var node2 = Member.MakeNew("TEST" + Guid.NewGuid().ToString("N"), mt1, m_User);

            //do the deletion of doc type #1
            mt1.delete();
            Assert.IsFalse(MemberType.IsNode(mt1.Id));

            //do our checks
            Assert.IsFalse(Member.IsNode(node1.Id), "node1 is not deleted");
            Assert.IsFalse(Member.IsNode(node2.Id), "node2 is not deleted");           
        }

        #region Tests to write
        ///// <summary>
        /////A test for MemberType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MemberTypeConstructorTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for MemberType Constructor
        /////</summary>
        //[TestMethod()]
        //public void MemberTypeConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for GetByAlias
        /////</summary>
        //[TestMethod()]
        //public void GetByAliasTest()
        //{
        //    string Alias = string.Empty; // TODO: Initialize to an appropriate value
        //    MemberType expected = null; // TODO: Initialize to an appropriate value
        //    MemberType actual;
        //    actual = MemberType.GetByAlias(Alias);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for MemberCanEdit
        /////</summary>
        //[TestMethod()]
        //public void MemberCanEditTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.MemberCanEdit(pt);
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
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ViewOnProfile
        /////</summary>
        //[TestMethod()]
        //public void ViewOnProfileTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.ViewOnProfile(pt);
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
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for setMemberCanEdit
        /////</summary>
        //[TestMethod()]
        //public void setMemberCanEditTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    bool value = false; // TODO: Initialize to an appropriate value
        //    target.setMemberCanEdit(pt, value);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for setMemberViewOnProfile
        /////</summary>
        //[TestMethod()]
        //public void setMemberViewOnProfileTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberType target = new MemberType(id); // TODO: Initialize to an appropriate value
        //    PropertyType pt = null; // TODO: Initialize to an appropriate value
        //    bool value = false; // TODO: Initialize to an appropriate value
        //    target.setMemberViewOnProfile(pt, value);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    MemberType[] actual;
        //    actual = MemberType.GetAll;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //} 
        #endregion

        #region Private methods

        private User m_User = new User(0); 
        
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
