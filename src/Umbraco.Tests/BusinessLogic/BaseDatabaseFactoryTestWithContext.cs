using System.Diagnostics;
using System.Reflection;
using System;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using System.Xml;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;

namespace Umbraco.Tests.TestHelpers
{
    public abstract partial class BaseDatabaseFactoryTestWithContext : BaseDatabaseFactoryTest
    {
        protected abstract void EnsureData();

        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get { return DatabaseBehavior.NewSchemaPerFixture; }
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!initialized) CreateContext(); 
            EnsureData();
        }

        protected bool initialized;

        protected UmbracoContext context;
        protected UmbracoDatabase database;

        protected void CreateContext()
        {
            context = GetUmbracoContext("http://localhost", 0);
            database = context.Application.DatabaseContext.Database;
        }

        protected UmbracoDatabase independentDatabase { get { return (new DefaultDatabaseFactory()).CreateDatabase(); }}
        protected T getPersistedTestDto<T>(int id, string idKeyName = "id")
        {
            return independentDatabase.SingleOrDefault<T>(string.Format("where {0} = @0", idKeyName), id);
        }


        protected void l(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        #region Private Helper classes and methods
        protected CMSNode _node1;
        protected CMSNode _node2;
        protected CMSNode _node3;
        protected CMSNode _node4;
        protected CMSNode _node5;

        private class TestCMSNode : CMSNode
        {
            public TestCMSNode(int id)
                : base(id)
            {
            }

            private TestCMSNode(int id, bool nosetup)
                : base(id, nosetup)
            {
            }

            public static CMSNode MakeNew(
                int parentId,
                int level,
                string text,
                Guid objectType)
            {
                return CMSNode.MakeNew(parentId, objectType, 0, level, text, Guid.NewGuid());
            }

            public void ExecuteSavePreviewXml(XmlDocument xd, Guid versionId)
            {
                SavePreviewXml(ToXml(xd, false), versionId);
            }

            public XmlNode ExecuteGetPreviewXml(XmlDocument xd, Guid versionId)
            {
                return GetPreviewXml(xd, versionId);
            }

            public static int[] ExecuteGetUniquesFromObjectTypeAndFirstLetter(Guid objectType, char letter)
            {
                return getUniquesFromObjectTypeAndFirstLetter(objectType, letter);
            }

            public static CMSNode CreateUsingSetupNode(int id)
            {
                var node = new TestCMSNode(id, true);
                node.setupNode();
                return node;
            }
        }

        public void MakeNew_PersistsNewUmbracoNodeRow()
        {
            // Testing Document._objectType, since it has exclusive use of GetNewDocumentSortOrder. :)

            int id = 0;
            try
            {
                _node1 = TestCMSNode.MakeNew(-1, 1, "TestContent 1", Document._objectType);
                _node2 = TestCMSNode.MakeNew(-1, 1, "TestContent 2", Document._objectType);
                _node3 = TestCMSNode.MakeNew(-1, 1, "TestContent 3", Document._objectType);
                _node4 = TestCMSNode.MakeNew(-1, 1, "TestContent 4", Document._objectType);
                _node5 = TestCMSNode.MakeNew(-1, 1, "TestContent 5", Document._objectType);
                var totalDocuments = independentDatabase.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM umbracoNode WHERE nodeObjectType = @ObjectTypeId",
                    new { ObjectTypeId = Document._objectType });
                Assert.AreEqual(5, totalDocuments);
                id = _node3.Id;
                var loadedNode = new CMSNode(id);
                AssertNonEmptyNode(loadedNode);
                Assert.AreEqual(2, loadedNode.sortOrder);
            }
            finally
            {
                //DeleteContent();
            }
        }

        private void AssertNonEmptyNode(CMSNode node)
        {
            Assert.AreNotEqual(0, node.Id);
            Assert.AreNotEqual(Guid.Empty, node.UniqueId);
            Assert.AreNotEqual(Guid.Empty, node.nodeObjectType);
            Assert.IsNotNullOrEmpty(node.Path);
            Assert.AreNotEqual(0, node.ParentId);
            Assert.IsNotNullOrEmpty(node.Text);
            Assert.AreEqual(DateTime.Today, node.CreateDateTime.Date);
            Assert.IsFalse(node.IsTrashed);
        }

        #endregion

    }
}