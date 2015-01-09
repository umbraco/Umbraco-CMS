using System;
using System.Data.SqlServerCe;
using System.IO;
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
    [TestFixture, RequiresSTA]
    public class SqlCeTableByTableTest : BaseTableByTableTest
    {
        private Database _database;

        #region Overrides of BaseTableByTableTest

        protected override ISqlSyntaxProvider SqlSyntaxProvider
        {
            get { return new SqlCeSyntaxProvider(); }
        }

        [SetUp]
        public override void Initialize()
        {
            base.Initialize();

            string path = TestHelper.CurrentAssemblyDirectory;
            //Delete database file before continueing
            string filePath = string.Concat(path, "\\test.sdf");
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            //Create the Sql CE database
            var engine = new SqlCeEngine("Datasource=|DataDirectory|test.sdf;Flush Interval=1;");
            engine.CreateDatabase();
            
            _database = new Database("Datasource=|DataDirectory|test.sdf;Flush Interval=1;",
                                     "System.Data.SqlServerCe.4.0");
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