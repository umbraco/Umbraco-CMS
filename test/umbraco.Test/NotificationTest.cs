using umbraco.cms.businesslogic.workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.interfaces;
using umbraco.cms.businesslogic.web;
using System.Linq;
using umbraco.BusinessLogic.Actions;

namespace umbraco.Test
{
    
    
    /// <summary>
    ///This is a test class for NotificationTest and is intended
    ///to contain all NotificationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NotificationTest
    {
        /// <summary>
        /// Create a new notification for the admin user for the first node found in the tree and delete it.
        /// </summary>
        [TestMethod()]
        public void Notification_Make_New()
        {
            //create a new notification
            var doc = Document.GetRootDocuments().First();
            Notification.MakeNew(m_User, doc, ActionNew.Instance.Letter);

            //get the notifications
            var n = Notification.GetUserNotifications(m_User);
            Assert.IsTrue(n.Count() > 0);
            Assert.AreEqual<int>(1, n.Where(x => x.NodeId == doc.Id && x.UserId == m_User.Id && x.ActionId == ActionNew.Instance.Letter).Count());

            //delete the notification
            Notification.DeleteNotifications(doc);

            //make sure they're gone
            Assert.AreEqual<int>(0, Notification.GetNodeNotifications(doc).Count());
            
        }

        /// <summary>
        /// Creates a new document, assigns some notifications to it, then deletes the document.
        /// Need to ensure that the notifications are removed.
        /// </summary>
        [TestMethod()]
        public void Notification_Assign_To_New_Node_Then_Delete_Node()
        {
            //create anew document
            var dt = DocumentType.GetAllAsList().First();
            var doc = Document.MakeNew(Guid.NewGuid().ToString("N"), dt, m_User, -1);
            
            //assign a notification to it
            Notification.MakeNew(m_User, doc, ActionNew.Instance.Letter);

            //delete the document permanently
            doc.delete(true);

            //make sure they're gone
            Assert.AreEqual<int>(0, Notification.GetNodeNotifications(doc).Count());
            Assert.IsFalse(Document.IsNode(doc.Id));

        }

        /// <summary>
        /// Create a new user, assign a notification to it, remove the user and ensure the notifications are gone as well.
        /// </summary>
        [TestMethod()]
        public void Notification_Assign_To_New_User_Then_Delete_User()
        {
            //create anew document
            var ut = UserType.GetAllUserTypes().First();
            var u = User.MakeNew(Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), Guid.NewGuid().ToString("N"), ut);
            //get a doc
            var doc = Document.GetRootDocuments().First();

            //assign a notification to the user
            Notification.MakeNew(u, doc, ActionNew.Instance.Letter);

            //delete the document permanently
            u.delete();

            //make sure they're gone
            Assert.AreEqual<int>(0, Notification.GetUserNotifications(u).Count());
            Assert.IsNull(User.GetUser(u.Id));

        }

        private User m_User = new User(0);

        #region Tests to write
        

        ///// <summary>
        /////A test for DeleteNotifications
        /////</summary>
        //[TestMethod()]
        //public void DeleteNotificationsTest()
        //{
        //    User user = null; // TODO: Initialize to an appropriate value
        //    CMSNode node = null; // TODO: Initialize to an appropriate value
        //    Notification.DeleteNotifications(user, node);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for DeleteNotifications
        /////</summary>
        //[TestMethod()]
        //public void DeleteNotificationsTest1()
        //{
        //    User user = null; // TODO: Initialize to an appropriate value
        //    Notification.DeleteNotifications(user);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for DeleteNotifications
        /////</summary>
        //[TestMethod()]
        //public void DeleteNotificationsTest2()
        //{
        //    CMSNode node = null; // TODO: Initialize to an appropriate value
        //    Notification.DeleteNotifications(node);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for GetNotifications
        /////</summary>
        //[TestMethod()]
        //public void GetNotificationsTest()
        //{
        //    CMSNode Node = null; // TODO: Initialize to an appropriate value
        //    User user = null; // TODO: Initialize to an appropriate value
        //    IAction Action = null; // TODO: Initialize to an appropriate value
        //    Notification.GetNotifications(Node, user, Action);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for MakeNew
        /////</summary>
        //[TestMethod()]
        //public void MakeNewTest()
        //{
        //    User User = null; // TODO: Initialize to an appropriate value
        //    CMSNode Node = null; // TODO: Initialize to an appropriate value
        //    char ActionLetter = '\0'; // TODO: Initialize to an appropriate value
        //    Notification.MakeNew(User, Node, ActionLetter);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for UpdateNotifications
        /////</summary>
        //[TestMethod()]
        //public void UpdateNotificationsTest()
        //{
        //    User User = null; // TODO: Initialize to an appropriate value
        //    CMSNode Node = null; // TODO: Initialize to an appropriate value
        //    string Notifications = string.Empty; // TODO: Initialize to an appropriate value
        //    Notification.UpdateNotifications(User, Node, Notifications);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
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
