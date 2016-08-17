using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Persistence.Migrations.Syntax.Alter.Expressions;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Services;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private ILogger _logger;
        private ISqlSyntaxProvider _sqlSyntax;
        private UmbracoDatabase _database;
        private MigrationContext _migrationContext;

        [SetUp]
        public void Setup()
        {
            _logger = Mock.Of<ILogger>();
            _sqlSyntax = new SqlCeSyntaxProvider();

            var dbProviderFactory = DbProviderFactories.GetFactory(Constants.DbProviderNames.SqlCe);
            _database = new UmbracoDatabase("cstr", _sqlSyntax, DatabaseType.SQLCe, dbProviderFactory, _logger);
            _migrationContext = new MigrationContext(_database, _logger);
        }

        [Test]
        public void Executes_Only_One_Migration_For_Spanning_Multiple_Targets()
        {
            var runner = new MigrationRunner(
                Mock.Of<IMigrationResolver>(),
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(4 /*, 0, 0*/), new SemVersion(6 /*, 0, 0*/), "Test");

            var context = new MigrationContext(_database, _logger);
            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(context) });
            runner.InitializeMigrations(migrations.ToList() /*, true*/);

            Assert.AreEqual(1, context.Expressions.Count);
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_1()
        {
            var runner = new MigrationRunner(
                Mock.Of<IMigrationResolver>(),
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(4 /*, 0, 0*/), new SemVersion(5 /*, 0, 0*/), "Test");

            var context = new MigrationContext(_database, _logger);
            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(context) });
            runner.InitializeMigrations(migrations.ToList() /*, true*/);

            Assert.AreEqual(1, context.Expressions.Count);
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_2()
        {
            var runner = new MigrationRunner(
                Mock.Of<IMigrationResolver>(),
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(5, 0, 1), new SemVersion(6 /*, 0, 0*/), "Test");

            var context = new MigrationContext(_database, _logger);
            var migrations = runner.OrderedUpgradeMigrations(new List<IMigration> { new MultiMigration(context) });
            runner.InitializeMigrations(migrations.ToList() /*, true*/);

            Assert.AreEqual(1, context.Expressions.Count);
        }

        [Migration("6.0.0", 1, "Test")]
        [Migration("5.0.0", 1, "Test")]
        private class MultiMigration : MigrationBase
        {
            public MultiMigration(IMigrationContext context) 
                : base(context)
            { }

            public override void Up()
            {
                Context.Expressions.Add(new AlterColumnExpression(Context, new [] { DatabaseType.SQLCe }));
            }

            public override void Down()
            {
                Context.Expressions.Add(new AlterColumnExpression(Context, new[] { DatabaseType.SQLCe }));
            }
        }
    }
}