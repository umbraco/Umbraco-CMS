using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Migrations.Expressions.Alter.Expressions;
using Umbraco.Core.Persistence;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class MigrationRunnerTests
    {
        private ILogger _logger;

        [SetUp]
        public void Setup()
        {
            _logger = Mock.Of<ILogger>();
        }

        [Test]
        public void Executes_Only_One_Migration_For_Spanning_Multiple_Targets()
        {
            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(MultiMigration))
                        throw new NotSupportedException();
                    return new MultiMigration(c) as IMigration;
                });

            var runner = new MigrationRunner(
                new TestScopeProvider(scope), 
                builder,
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(4 /*, 0, 0*/), new SemVersion(6 /*, 0, 0*/), "Test");

            var types = runner.OrderedUpgradeMigrations(new[] { typeof(MultiMigration) }).ToArray();
            Assert.AreEqual(1, types.Length);

            var context = new MigrationContext(database, Mock.Of<ILogger>());
            var migrations = types.Select(x => builder.Instanciate(x, context));

            runner.ExecuteMigrations(migrations /*, true*/);
            Assert.AreEqual(1, database.Operations.Count);
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_1()
        {
            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(MultiMigration))
                        throw new NotSupportedException();
                    return new MultiMigration(c) as IMigration;
                });

            var runner = new MigrationRunner(
                new TestScopeProvider(scope),
                builder,
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(4 /*, 0, 0*/), new SemVersion(5 /*, 0, 0*/), "Test");

            var types = runner.OrderedUpgradeMigrations(new[] { typeof(MultiMigration) }).ToArray();

            var context = new MigrationContext(database, Mock.Of<ILogger>());
            var migrations = types.Select(x => builder.Instanciate(x, context));

            runner.ExecuteMigrations(migrations /*, true*/);

            Assert.AreEqual(1, database.Operations.Count);
        }

        [Test]
        public void Executes_Migration_For_Spanning_One_Target_2()
        {
            var database = new TestDatabase();
            var scope = Mock.Of<IScope>();
            Mock.Get(scope)
                .Setup(x => x.Database)
                .Returns(database);

            var scopeProvider = Mock.Of<IScopeProvider>();
            Mock.Get(scopeProvider)
                .Setup(x => x.CreateScope(It.IsAny<IsolationLevel>(), It.IsAny<RepositoryCacheMode>(), It.IsAny<IEventDispatcher>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(scope);

            var builder = Mock.Of<IMigrationCollectionBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Instanciate(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    if (t != typeof(MultiMigration))
                        throw new NotSupportedException();
                    return new MultiMigration(c);
                });

            var runner = new MigrationRunner(
                scopeProvider,
                builder,
                Mock.Of<IMigrationEntryService>(),
                _logger, new SemVersion(5, 0, 1), new SemVersion(6 /*, 0, 0*/), "Test");

            var types = runner.OrderedUpgradeMigrations(new[] { typeof(MultiMigration) }).ToArray();

            var context = new MigrationContext(database, Mock.Of<ILogger>());
            var migrations = types.Select(x => builder.Instanciate(x, context));

            runner.ExecuteMigrations(migrations /*, true*/);

            Assert.AreEqual(1, database.Operations.Count);
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
                Alter.Table("table").AlterColumn("column").AsString().Do();
            }

            public override void Down()
            {
                Alter.Table("table").AlterColumn("column").AsString().Do();
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
    }
}
