using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Services;
using Umbraco.Tests.TestHelpers;
using Umbraco.Web.Strategies.Migrations;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class PostMigrationTests
    {
        private TestObjects TestObjects = new TestObjects(null);

        [Test]
        public void Executes_For_Any_Product_Name_When_Not_Specified()
        {
            var logger = Mock.Of<ILogger>();

            var changed1 = new Args { CountExecuted = 0 };
            var testHandler1 = new TestMigrationHandler(changed1);
            MigrationRunner.Migrated += testHandler1.Migrated;

            var db = TestObjects.GetUmbracoSqlCeDatabase(logger);
            var migrationContext = new MigrationContext(db, logger);
            var runner1 = new MigrationRunner(Mock.Of<IMigrationCollectionBuilder>(), Mock.Of<IMigrationEntryService>(), logger, new SemVersion(1), new SemVersion(2), "Test1",
                new IMigration[] { Mock.Of<IMigration>() });
            var result1 = runner1.Execute(migrationContext /*, false*/);
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

            var db = TestObjects.GetUmbracoSqlCeDatabase(logger);
            var migrationContext = new MigrationContext(db, logger);
            var runner1 = new MigrationRunner(Mock.Of<IMigrationCollectionBuilder>(), Mock.Of<IMigrationEntryService>(), logger, new SemVersion(1), new SemVersion(2), "Test1",
                new IMigration[] { Mock.Of<IMigration>()});
            var result1 = runner1.Execute(migrationContext /*, false*/);
            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(0, changed2.CountExecuted);

            var runner2 = new MigrationRunner(Mock.Of<IMigrationCollectionBuilder>(), Mock.Of<IMigrationEntryService>(), logger, new SemVersion(1), new SemVersion(2), "Test2",
                new IMigration[] { Mock.Of<IMigration>() });
            var result2 = runner2.Execute(migrationContext /*, false*/);
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
