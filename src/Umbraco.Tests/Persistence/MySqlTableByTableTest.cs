using System;
using NUnit.Framework;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture, NUnit.Framework.Ignore]
    public class MySqlTableByTableTest : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        [SetUp]
        public override void Initialize()
        {
            string path = TestHelper.CurrentAssemblyDirectory;
            AppDomain.CurrentDomain.SetData("DataDirectory", path);

            SyntaxConfig.SqlSyntaxProvider = MySqlSyntaxProvider.Instance;

            _database = new Database("Server = 192.168.1.108; Database = testDb; Uid = umbraco; Pwd = umbraco",
                                     "MySql.Data.MySqlClient");
        }

        [TearDown]
        public override void TearDown()
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", null);
        }

        public override Database Database
        {
            get { return _database; }
        }

        #endregion
    }
}