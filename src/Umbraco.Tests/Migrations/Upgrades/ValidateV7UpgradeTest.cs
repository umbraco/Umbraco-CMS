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
            var migration = new AddIndexToCmsMacroTable(true, _logger);
            var migrationContext = new MigrationContext(_database, _logger);
            migration.GetUpExpressions(migrationContext);

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacro_Alias] ON [cmsMacro] ([macroAlias])", result);
        }

        [Test]
        public void Validate_AddIndexToCmsMacroPropertyTable()
        {
            var migration = new AddIndexToCmsMacroTable(true, _logger);
            var migrationContext = new MigrationContext(_database, _logger);
            migration.GetUpExpressions(migrationContext);

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacroProperty_Alias] ON [cmsMacroProperty] ([macro],[macroPropertyAlias])", result);
        }
    }
}