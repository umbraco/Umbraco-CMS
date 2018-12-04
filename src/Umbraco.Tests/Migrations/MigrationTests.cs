using System;
using System.Data;
using Moq;
using NUnit.Framework;
using Semver;
using Umbraco.Core.Events;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Upgrade;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using ILogger = Umbraco.Core.Logging.ILogger;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationTests
    {
        public class TestUpgrader : Upgrader
        {
            private readonly MigrationPlan _plan;

            public TestUpgrader(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger, MigrationPlan plan)
                : base(scopeProvider, migrationBuilder, keyValueService, logger)
            {
                _plan = plan;
            }

            protected override MigrationPlan CreatePlan()
            {
                return _plan;
            }
        }

        public class TestUpgraderWithPostMigrations : TestUpgrader
        {
            private readonly PostMigrationCollection _postMigrations;

            public TestUpgraderWithPostMigrations(IScopeProvider scopeProvider, IMigrationBuilder migrationBuilder, IKeyValueService keyValueService, ILogger logger, PostMigrationCollection postMigrations, MigrationPlan plan)
                : base(scopeProvider, migrationBuilder, keyValueService, logger, plan)
            {
                _postMigrations = postMigrations;
            }

            public override void AfterMigrations(IScope scope)
            {
                // run post-migrations
                var originVersion = new SemVersion(0);
                var targetVersion = new SemVersion(0);

                foreach (var postMigration in _postMigrations)
                    postMigration.Execute(Name, scope, originVersion, targetVersion, Logger);
            }
        }

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
