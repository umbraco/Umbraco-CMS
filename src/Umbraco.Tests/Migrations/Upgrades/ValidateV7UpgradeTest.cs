using System.Data.Common;
using System.Linq;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSeven;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Tests.Migrations.Upgrades
{
    [TestFixture]
    public class ValidateV7UpgradeTest
    {
        private ILogger _logger;
        private ISqlSyntaxProvider _sqlSyntax;
        private UmbracoDatabase _database;

        [SetUp]
        public void Setup()
        {
            _logger = Mock.Of<ILogger>();
            _sqlSyntax = new SqlCeSyntaxProvider();

            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            _database = new UmbracoDatabase("cstr", _sqlSyntax, DatabaseType.SQLCe, dbProviderFactory, _logger);
        }

        [Test]
        public void Validate_AddIndexToCmsMacroTable()
        {
            var migrationContext = new MigrationContext(_database, _logger);
            var migration = new AddIndexToCmsMacroTable(true, migrationContext);            
            migration.Up();

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacro_Alias] ON [cmsMacro] ([macroAlias])", result);
        }

        [Test]
        public void Validate_AddIndexToCmsMacroPropertyTable()
        {
            var migrationContext = new MigrationContext(_database, _logger);
            var migration = new AddIndexToCmsMacroPropertyTable(true, migrationContext);
            migration.Up();

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacroProperty_Alias] ON [cmsMacroProperty] ([macro],[macroPropertyAlias])", result);
        }
    }
}