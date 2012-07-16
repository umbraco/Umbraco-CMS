using umbraco.cms.businesslogic.member;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using System.Collections;
using System.Linq;

namespace Umbraco.LegacyTests
{
    
    
    /// <summary>
    ///This is a test class for MemberGroupTest and is intended
    ///to contain all MemberGroupTest Unit Tests
    ///</summary>
    [TestClass()]
    public class MemberGroupTest
    {

        /// <summary>
        /// Make a new member group and delete it
        ///</summary>
        [TestMethod()]
        public void MemberGroup_Make_New()
        {
            var m = MemberGroup.MakeNew(Guid.NewGuid().ToString("N"), m_User);
            Assert.IsTrue(m.Id > 0);
            Assert.IsInstanceOfType(m, typeof(MemberGroup));

            m.delete();
            //make sure its gone
            Assert.IsFalse(MemberGroup.IsNode(m.Id));

        }

        /// <summary>
        /// Create a new member group, put a member in it, then delete the group and ensure the member is gone
        /// </summary>
        [TestMethod()]
        public void MemberGroup_Add_Member_To_Group_And_Delete_Group()
        {
            var mt = MemberType.MakeNew(m_User, "TEST" + Guid.NewGuid().ToString("N"));
            var m = Member.MakeNew("TEST" + Guid.NewGuid().ToString("N"),
                "TEST" + Guid.NewGuid().ToString("N") + "@test.com", mt, m_User);

            var mg = MemberGroup.MakeNew("TEST" + Guid.NewGuid().ToString("N"), m_User);
            Assert.IsInstanceOfType(mg, typeof(MemberGroup));
            Assert.IsTrue(mg.Id > 0);          

            //add the member to the group
            m.AddGroup(mg.Id);

            //ensure they are added
            Assert.AreEqual(1, m.Groups.Count);
            Assert.AreEqual<int>(mg.Id, ((MemberGroup)m.Groups.Cast<DictionaryEntry>().First().Value).Id);

            //delete the group
            mg.delete();

            //make sure the member is no longer associated
            m = new Member(m.Id); //need to re-get the member
            Assert.AreEqual<int>(0, m.Groups.Count);
            
            //now cleanup...

            m.delete();
            Assert.IsFalse(Member.IsNode(m.Id));

            mt.delete();
            Assert.IsFalse(MemberType.IsNode(mt.Id));
        }

        private User m_User = new User(0);

        #region Tests to write

        ///// <summary>
        /////A test for MemberGroup Constructor
        /////</summary>
        //[TestMethod()]
        //public void MemberGroupConstructorTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for MemberGroup Constructor
        /////</summary>
        //[TestMethod()]
        //public void MemberGroupConstructorTest1()
        //{
        //    int id = 0; // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id);
        //    Assert.Inconclusive("TODO: Implement code to verify target");
        //}

        ///// <summary>
        /////A test for GetByName
        /////</summary>
        //[TestMethod()]
        //public void GetByNameTest()
        //{
        //    string Name = string.Empty; // TODO: Initialize to an appropriate value
        //    MemberGroup expected = null; // TODO: Initialize to an appropriate value
        //    MemberGroup actual;
        //    actual = MemberGroup.GetByName(Name);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetMembers
        /////</summary>
        //[TestMethod()]
        //public void GetMembersTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    Member[] expected = null; // TODO: Initialize to an appropriate value
        //    Member[] actual;
        //    actual = target.GetMembers();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetMembers
        /////</summary>
        //[TestMethod()]
        //public void GetMembersTest1()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    string usernameToMatch = string.Empty; // TODO: Initialize to an appropriate value
        //    Member[] expected = null; // TODO: Initialize to an appropriate value
        //    Member[] actual;
        //    actual = target.GetMembers(usernameToMatch);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetMembersAsIds
        /////</summary>
        //[TestMethod()]
        //public void GetMembersAsIdsTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    int[] expected = null; // TODO: Initialize to an appropriate value
        //    int[] actual;
        //    actual = target.GetMembersAsIds();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for HasMember
        /////</summary>
        //[TestMethod()]
        //public void HasMemberTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    int memberId = 0; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = target.HasMember(memberId);
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
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    target.Save();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for delete
        /////</summary>
        //[TestMethod()]
        //public void deleteTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
        //    target.delete();
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetAll
        /////</summary>
        //[TestMethod()]
        //public void GetAllTest()
        //{
        //    MemberGroup[] actual;
        //    actual = MemberGroup.GetAll;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for Text
        /////</summary>
        //[TestMethod()]
        //public void TextTest()
        //{
        //    Guid id = new Guid(); // TODO: Initialize to an appropriate value
        //    MemberGroup target = new MemberGroup(id); // TODO: Initialize to an appropriate value
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
