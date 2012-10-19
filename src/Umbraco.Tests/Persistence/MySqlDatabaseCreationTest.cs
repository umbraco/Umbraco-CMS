using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class MySqlDatabaseCreationTest : BaseDatabaseTest
    {
        #region Overrides of BaseDatabaseTest

        public override string ConnectionString
        {
            get { return "Server = 192.168.1.108; Database = testDb; Uid = umbraco; Pwd = umbraco"; }
        }

        public override string ProviderName
        {
            get { return "MySql.Data.MySqlClient"; }
        }

        public override ISqlSyntaxProvider SyntaxProvider
        {
            get { return MySqlSyntaxProvider.Instance; }
        }

        #endregion

        [Test]
        public void Can_Assert_Created_Database()
        {
            bool umbracoNodeTable = Database.TableExist("umbracoNode");
            bool umbracoUserTable = Database.TableExist("umbracoUser");
            bool cmsTagsTable = Database.TableExist("cmsTags");

            Assert.That(umbracoNodeTable, Is.True);
            Assert.That(umbracoUserTable, Is.True);
            Assert.That(cmsTagsTable, Is.True);
        }
    }
}