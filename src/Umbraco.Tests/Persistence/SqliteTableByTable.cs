using System.IO;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture, RequiresSTA]
    public class SqliteTableByTable : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            string path = TestHelper.CurrentAssemblyDirectory;
            //Delete database file before continueing
            string filePath = string.Concat(path, "\\db.sqlite");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            SqlSyntaxContext.SqlSyntaxProvider = SqliteSyntax.Provider;

            _database = new Database("Data Source=|DataDirectory|db.sqlite;Version=3;New=True;Compress=True;",
                                     "System.Data.SQLite");
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
        }

        public override Database Database
        {
            get { return _database; }
        }

        #endregion
    }
}