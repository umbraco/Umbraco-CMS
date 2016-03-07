using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Migrations;
using Umbraco.Core.Profiling;
using Umbraco.Core.Services;
using Umbraco.Web.Strategies.Migrations;

namespace Umbraco.Tests.Persistence.Migrations
{
    [TestFixture]
    public class MigrationStartupHandlerTests
    {
        [Test]
        public void Executes_For_Any_Product_Name_When_Not_Specified()
        {
            var changed1 = new Args { CountExecuted = 0 };
            var testHandler1 = new TestMigrationHandler(changed1);
            testHandler1.OnApplicationStarting(Mock.Of<UmbracoApplicationBase>(), new ApplicationContext(CacheHelper.CreateDisabledCacheHelper(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
            
            var conn = new Mock<IDbConnection>();
            conn.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(Mock.Of<IDbTransaction>());
            var db = new Mock<Database>(conn.Object);

            var runner1 = new MigrationRunner(Mock.Of<IMigrationEntryService>(), Mock.Of<ILogger>(), new SemVersion(1), new SemVersion(2), "Test1",
                new IMigration[] { Mock.Of<IMigration>() });
            var result1 = runner1.Execute(db.Object, DatabaseProviders.SqlServerCE, false);
            Assert.AreEqual(1, changed1.CountExecuted);            
        }

        [Test]
        public void Executes_Only_For_Specified_Product_Name()
        {
            var changed1 = new Args { CountExecuted = 0};
            var testHandler1 = new TestMigrationHandler("Test1", changed1);
            testHandler1.OnApplicationStarting(Mock.Of<UmbracoApplicationBase>(), new ApplicationContext(CacheHelper.CreateDisabledCacheHelper(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));
            var changed2 = new Args { CountExecuted = 0 };
            var testHandler2 = new TestMigrationHandler("Test2", changed2);
            testHandler2.OnApplicationStarting(Mock.Of<UmbracoApplicationBase>(), new ApplicationContext(CacheHelper.CreateDisabledCacheHelper(), new ProfilingLogger(Mock.Of<ILogger>(), Mock.Of<IProfiler>())));

            var conn = new Mock<IDbConnection>();
            conn.Setup(x => x.BeginTransaction(It.IsAny<IsolationLevel>())).Returns(Mock.Of<IDbTransaction>());
            var db = new Mock<Database>(conn.Object);

            var runner1 = new MigrationRunner(Mock.Of<IMigrationEntryService>(), Mock.Of<ILogger>(), new SemVersion(1), new SemVersion(2), "Test1",
                new IMigration[] { Mock.Of<IMigration>()});
            var result1 = runner1.Execute(db.Object, DatabaseProviders.SqlServerCE, false);
            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(0, changed2.CountExecuted);

            var runner2 = new MigrationRunner(Mock.Of<IMigrationEntryService>(), Mock.Of<ILogger>(), new SemVersion(1), new SemVersion(2), "Test2",
                new IMigration[] { Mock.Of<IMigration>() });            
            var result2 = runner2.Execute(db.Object, DatabaseProviders.SqlServerCE, false);
            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(1, changed2.CountExecuted);
        }

        public class Args
        {
            public int CountExecuted { get; set; }
        }

        public class TestMigrationHandler : MigrationStartupHander
        {
            private readonly string _prodName;
            private readonly Args _changed;

            public TestMigrationHandler(Args changed)
            {
                _changed = changed;
            }

            public TestMigrationHandler(string prodName, Args changed)
            {
                _prodName = prodName;
                _changed = changed;
            }

            protected override void AfterMigration(MigrationRunner sender, MigrationEventArgs e)
            {
                _changed.CountExecuted++;
            }
            
            public override string[] TargetProductNames
            {
                get { return _prodName.IsNullOrWhiteSpace() ? new string[] {} : new[] {_prodName}; }
            }
        }
    }
}
