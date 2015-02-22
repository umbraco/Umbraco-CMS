using System.Linq;
using Moq;
using NUnit.Framework;
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
       

        [Test]
        public void Validate_AddIndexToCmsMacroTable()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var migration = new AddIndexToCmsMacroTable(true, sqlSyntax, Mock.Of<ILogger>());
            var migrationContext = new MigrationContext(DatabaseProviders.SqlServerCE, new Database("test", "System.Data.SqlClient"), Mock.Of<ILogger>(),
                sqlSyntax);
            migration.GetUpExpressions(migrationContext);

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacro_Alias] ON [cmsMacro] ([macroAlias])", result);
        }

        [Test]
        public void Validate_AddIndexToCmsMacroPropertyTable()
        {
            var sqlSyntax = new SqlCeSyntaxProvider();
            var migration = new AddIndexToCmsMacroPropertyTable(true, sqlSyntax, Mock.Of<ILogger>());
            var migrationContext = new MigrationContext(DatabaseProviders.SqlServerCE, new Database("test", "System.Data.SqlClient"), Mock.Of<ILogger>(), sqlSyntax);
            migration.GetUpExpressions(migrationContext);

            Assert.AreEqual(1, migrationContext.Expressions.Count);

            var result = migrationContext.Expressions.First().ToString();

            Assert.AreEqual("CREATE UNIQUE NONCLUSTERED INDEX [IX_cmsMacroProperty_Alias] ON [cmsMacroProperty] ([macro],[macroPropertyAlias])", result);
        }
    }
}