using System;
using System.Data;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade.TargetVersionEight;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Tests.Testing;
using Umbraco.Web.Strategies.Migrations;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class PostMigrationTests
    {
        private class NopMigration : MigrationBase
        {
            public NopMigration(IMigrationContext context) : base(context)
            { }

            public override void Up()
            { }

            public override void Down()
            { }
        }

        [Test]
        public void Executes_For_Any_Product_Name_When_Not_Specified()
        {
            var logger = Mock.Of<ILogger>();

            var changed1 = new Args { CountExecuted = 0 };
            var testHandler1 = new TestMigrationHandler(changed1);
            MigrationRunner.Migrated += testHandler1.Migrated;

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "DeleteRedirectUrlTable":
                            return new MigrationIssuesTests.DeleteRedirectUrlTable(c);
                        case "AddRedirectUrlTable":
                            return new AddRedirectUrlTable(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            var scopeProvider = new MigrationRunnerTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            var runner1 = new MigrationRunner(
                scopeProvider,
                builder,
                Mock.Of<IMigrationEntryService>(),
                logger,
                new SemVersion(1), new SemVersion(2), "Test1",
                typeof(NopMigration));

            var result1 = runner1.Execute();
            Assert.AreEqual(1, changed1.CountExecuted);
        }

        [Test]
        public void Executes_Only_For_Specified_Product_Name()
        {
            var logger = Mock.Of<ILogger>();

            var changed1 = new Args { CountExecuted = 0};
            var testHandler1 = new TestMigrationHandler("Test1", changed1);
            MigrationRunner.Migrated += testHandler1.Migrated;

            var changed2 = new Args { CountExecuted = 0 };
            var testHandler2 = new TestMigrationHandler("Test2", changed2);
            MigrationRunner.Migrated += testHandler2.Migrated;

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "DeleteRedirectUrlTable":
                            return new MigrationIssuesTests.DeleteRedirectUrlTable(c);
                        case "AddRedirectUrlTable":
                            return new AddRedirectUrlTable(c);
                        default:
                            throw new NotSupportedException();
                    }
                });

            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var sqlContext = new SqlContext(new SqlCeSyntaxProvider(), DatabaseType.SQLCe, Mock.Of<IPocoDataFactory>());
            var scopeProvider = new MigrationRunnerTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            var runner1 = new MigrationRunner(
                scopeProvider,
                builder,
                Mock.Of<IMigrationEntryService>(),
                logger,
                new SemVersion(1), new SemVersion(2), "Test1",
                typeof(NopMigration));

            var result1 = runner1.Execute();
            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(0, changed2.CountExecuted);

            var runner2 = new MigrationRunner(
                scopeProvider,
                builder,
                Mock.Of<IMigrationEntryService>(),
                logger,
                new SemVersion(1), new SemVersion(2), "Test2",
                typeof(NopMigration));

            var result2 = runner2.Execute();
            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(1, changed2.CountExecuted);
        }

        public class Args
        {
            public int CountExecuted { get; set; }
        }

        public class TestMigrationHandler : IPostMigration
        {
            private readonly string _prodName;
            private readonly Args _changed;

            // need that one else it breaks IoC
            public TestMigrationHandler()
            {
                _changed = new Args();
            }

            public TestMigrationHandler(Args changed)
            {
                _changed = changed;
            }

            public TestMigrationHandler(string prodName, Args changed)
            {
                _prodName = prodName;
                _changed = changed;
            }

            public void Migrated(MigrationRunner sender, MigrationEventArgs args)
            {
                if (_prodName.IsNullOrWhiteSpace() == false && args.ProductName != _prodName) return;
                _changed.CountExecuted++;
            }
        }
    }
}
