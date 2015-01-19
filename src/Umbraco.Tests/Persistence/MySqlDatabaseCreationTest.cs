using Moq;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture, NUnit.Framework.Ignore]
    public class MySqlDatabaseCreationTest : BaseDatabaseTest
    {
        #region Overrides of BaseDatabaseTest

        public override string ConnectionString
        {
            get { return "Server = 169.254.120.3; Database = testdb; Uid = umbraco; Pwd = umbraco"; }
        }

        public override string ProviderName
        {
            get { return "MySql.Data.MySqlClient"; }
        }

        public override ISqlSyntaxProvider SyntaxProvider
        {
            get { return new MySqlSyntaxProvider(Mock.Of<ILogger>()); }
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