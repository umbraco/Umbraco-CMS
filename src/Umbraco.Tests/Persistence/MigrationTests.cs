using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Tests.TestHelpers.MockedMigrations;

namespace Umbraco.Tests.Persistence
{
    [TestFixture]
    public class MigrationTests
    {
        [Test]
        public void Can_Verify_Add_ColumnChange()
        {
            var columnChange = new AddAllowAtRootColumn();
            Sql sql = columnChange.ToSql();

            Assert.AreEqual(sql.SQL, "ALTER TABLE [cmsContentType] ADD [allowAtRoot] [bit] NOT NULL CONSTRAINT [df_cmsContentType_allowAtRoot] DEFAULT (0);");
        }

        [Test]
        public void Can_Verify_Drop_ColumnChange()
        {
            var columnChange = new DropMasterContentTypeColumn();
            Sql sql = columnChange.ToSql();

            Assert.AreEqual(sql.SQL, "ALTER TABLE [cmsContentType] DROP COLUMN [masterContentType];");
        }
    }
}