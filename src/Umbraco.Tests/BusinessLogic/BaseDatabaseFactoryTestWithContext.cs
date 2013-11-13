using System.Diagnostics;
using System.Reflection;
using System;
using Umbraco.Web;
using Umbraco.Core.Persistence;
using umbraco.cms.businesslogic;
using System.Xml;
using umbraco.cms.businesslogic.web;
using NUnit.Framework;
using System.Text;
using Umbraco.Core.Models.Rdbms;

namespace Umbraco.Tests.TestHelpers
{
    public abstract partial class BaseDatabaseFactoryTestWithContext : BaseDatabaseFactoryTest
    {
        protected abstract void EnsureData();

        const bool NO_BASE_CLASS_ASSERTS = true;
        protected override DatabaseBehavior DatabaseTestBehavior
        {
            get 
            {
                return DatabaseBehavior.NoDatabasePerFixture;    
               //return DatabaseBehavior.NewSchemaPerFixture; 
            }
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

        protected string uniqueLabel
        {
            get
            {
                return string.Format("* {0} *", Guid.NewGuid().ToString());
            }
        }

        protected void Setter_Persists_Ext<T, S, U>(
              Func<T, S> getter,
              Action<T> setter,
              string tableName,
              string fieldName,
              U expected,
              string idFieldName,
              int id,
              bool useSecondGetter = false,
              Func<T, U> getter2 = null,
              U oldValue2 = default(U)
         )
        {
            UmbracoDatabase independentDatabase = (new DefaultDatabaseFactory()).CreateDatabase();
            T testORMClass = (T)Activator.CreateInstance(typeof(T), id);
            S oldValue = getter(testORMClass);
            try
            {
                setter(testORMClass);  // set new value and get it persisted via ORM
                // extract saved value via independent database
                var persisted = independentDatabase.ExecuteScalar<U>(
                    String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @0", fieldName, tableName, idFieldName), id);
                Assert.AreEqual(expected, persisted);
                if (!useSecondGetter)
                  Assert.AreEqual(expected, getter(testORMClass));
                else
                    Assert.AreEqual(expected, getter2(testORMClass));
            }
            finally
            {
                // reset to oldValue
                string sql = String.Format("Update [{0}] set [{1}] = @0 WHERE [{2}] = @1", tableName, fieldName, idFieldName);
                if (!useSecondGetter)
                    independentDatabase.Execute(sql, oldValue, id);
                else
                    independentDatabase.Execute(sql, oldValue2, id);

                // double check
                var persisted2 = independentDatabase.ExecuteScalar<U>(
                    String.Format("SELECT [{0}] FROM [{1}] WHERE [{2}] = @0", fieldName, tableName, idFieldName), id);
                if (!useSecondGetter)
                    Assert.AreEqual(oldValue, persisted2);
                else
                    Assert.AreEqual(oldValue2, persisted2);
            }
        }


        protected void l(string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        #region Helper methods borroed from umbraco
        protected string getSqlStringArray(string commaSeparatedArray)
        {
            // create array
            string[] array = commaSeparatedArray.Trim().Split(',');

            // build SQL array
            StringBuilder sqlArray = new StringBuilder();
            foreach (string item in array)
            {
                string trimmedItem = item.Trim();
                if (trimmedItem.Length > 0)
                {
                    sqlArray.Append("'").Append(escapeString(trimmedItem)).Append("',");
                }
            }

            // remove last comma
            if (sqlArray.Length > 0)
                sqlArray.Remove(sqlArray.Length - 1, 1);
            return sqlArray.ToString();
        }

        protected static string escapeString(string value)
        {
            return string.IsNullOrEmpty(value) ? string.Empty : value.Replace("'", "''");
        }

        #endregion

        #region Private Helper classes and methods from cms_businesslogic_CMSNodeTests
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
                if (!NO_BASE_CLASS_ASSERTS)  Assert.AreEqual(5, totalDocuments);
                id = _node3.Id;
                var loadedNode = new CMSNode(id);

                if (!NO_BASE_CLASS_ASSERTS)
                {
                    AssertNonEmptyNode(loadedNode);
                    Assert.AreEqual(2, loadedNode.sortOrder);
                }
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

        private void Setter_Persists<T>(int nodeId, Action<CMSNode> setter, Func<CMSNode, T> getter, T expected, string field)
        {
            var node = new CMSNode(nodeId);
            setter(node);
            var persisted = database.ExecuteScalar<T>(
                String.Format("SELECT {0} FROM cmsPropertyType WHERE id = @id", field)
                , new { id = nodeId });

            Assert.AreEqual(expected, persisted);
            Assert.AreEqual(expected, getter(node));
            //SS:ResetTestDocumentTypes();
        }

        //private void ResetTestDocumentTypes()
        //{
        //    foreach (var node in contentTypes)
        //        DeleteContentType(node.Id);
        //    initialized = false;
        //}

        //protected void deleteContent()
        //{
        //    database.Execute("DELETE cmsPreviewXml");
        //    database.Execute("DELETE cmsContentVersion");
        //    database.Execute("DELETE cmsDocument");
        //    database.Execute("DELETE cmsContent");
        //    database.Delete<NodeDto>("WHERE nodeObjectType = @ObjectTypeId", new { ObjectTypeId = Document._objectType });
        //}

        //protected void deleteContentType(int id)
        //{
        //    database.Execute("DELETE cmsContentType WHERE nodeId = @NodeId", new { NodeId = id });
        //    database.Execute("DELETE umbracoNode WHERE id = @Id", new { Id = id });
        //}

        #endregion

    }
}