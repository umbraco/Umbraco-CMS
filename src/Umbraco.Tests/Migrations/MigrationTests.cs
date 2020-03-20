using System;
using System.Data;
using Moq;
using NUnit.Framework;
using Umbraco.Core.Events;
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationTests
    {
        public class TestScopeProvider : IScopeProvider
        {
            private readonly IScope _scope;

            public TestScopeProvider(IScope scope)
            {
                _scope = scope;
            }

            public IScope CreateScope(IsolationLevel isolationLevel = IsolationLevel.Unspecified, RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, IEventDispatcher eventDispatcher = null, bool? scopeFileSystems = null, bool callContext = false, bool autoComplete = false)
            {
                return _scope;
            }

            public IScope CreateDetachedScope(IsolationLevel isolationLevel = IsolationLevel.Unspecified, RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified, IEventDispatcher eventDispatcher = null, bool? scopeFileSystems = null)
            {
                throw new NotImplementedException();
            }

            public void AttachScope(IScope scope, bool callContext = false)
            {
                throw new NotImplementedException();
            }

            public IScope DetachScope()
            {
                throw new NotImplementedException();
            }

            public IScopeContext Context { get; set; }
            public ISqlContext SqlContext { get; set;  }
        }

        [Test]
        public void RunGoodMigration()
        {
            var migrationContext = new MigrationContext(Mock.Of<IUmbracoDatabase>(), Mock.Of<ILogger>());
            IMigration migration = new GoodMigration(migrationContext);
            migration.Migrate();
        }

        [Test]
        public void DetectBadMigration1()
        {
            var migrationContext = new MigrationContext(Mock.Of<IUmbracoDatabase>(), Mock.Of<ILogger>());
            IMigration migration = new BadMigration1(migrationContext);
            Assert.Throws<IncompleteMigrationExpressionException>(() => migration.Migrate());
        }

        [Test]
        public void DetectBadMigration2()
        {
            var migrationContext = new MigrationContext(Mock.Of<IUmbracoDatabase>(), Mock.Of<ILogger>());
            IMigration migration = new BadMigration2(migrationContext);
            Assert.Throws<IncompleteMigrationExpressionException>(() => migration.Migrate());
        }

        public class GoodMigration : MigrationBase
        {
            public GoodMigration(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                Execute.Sql("").Do();
            }
        }

        public class BadMigration1 : MigrationBase
        {
            public BadMigration1(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                Alter.Table("foo"); // stop here, don't Do it
            }
        }

        public class BadMigration2 : MigrationBase
        {
            public BadMigration2(IMigrationContext context)
                : base(context)
            { }

            public override void Migrate()
            {
                Alter.Table("foo"); // stop here, don't Do it

                // and try to start another one
                Alter.Table("bar");
            }
        }
    }
}
