using System;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Persistence.UnitOfWork;
using Umbraco.Core.Publishing;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Persistence
{
    [TestFixture, NUnit.Framework.Ignore]
    public class SqlTableByTableTest : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        protected override ISqlSyntaxProvider SqlSyntaxProvider
        {
            get { return new SqlServerSyntaxProvider(); }
        }

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();           

            _database = new Database(@"server=.\SQLEXPRESS;database=EmptyForTest;user id=umbraco;password=umbraco",
                                     "System.Data.SqlClient");
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