using System;
using Moq;
using NPoco;
using NUnit.Framework;
using Semver;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Migrations;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;
using Umbraco.Core.Services;
using Umbraco.Tests.Testing;

namespace Umbraco.Tests.Migrations
{
    [TestFixture]
    public class PostMigrationTests
    {
        [Test]
        public void Executes_For_Any_Product_Name_When_Not_Specified()
        {
            var logger = Mock.Of<ILogger>();

            var changed1 = new Args { CountExecuted = 0 };
            var post1 = new TestPostMigration(changed1);

            var posts = new PostMigrationCollection(new [] { post1 });

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "NoopMigration":
                            return new NoopMigration();
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
            var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            var u1 = new MigrationTests.TestUpgraderWithPostMigrations(scopeProvider, builder, Mock.Of<IKeyValueService>(), logger, posts,
                new MigrationPlan("Test",  builder, logger).From(string.Empty).To("done"));
            u1.Execute();

            Assert.AreEqual(1, changed1.CountExecuted);
        }

        [Test]
        public void Executes_Only_For_Specified_Product_Name()
        {
            var logger = Mock.Of<ILogger>();

            var changed1 = new Args { CountExecuted = 0};
            var post1 = new TestPostMigration("Test1", changed1);

            var changed2 = new Args { CountExecuted = 0 };
            var post2 = new TestPostMigration("Test2", changed2);

            var posts = new PostMigrationCollection(new [] { post1, post2 });

            var builder = Mock.Of<IMigrationBuilder>();
            Mock.Get(builder)
                .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
                .Returns<Type, IMigrationContext>((t, c) =>
                {
                    switch (t.Name)
                    {
                        case "NoopMigration":
                            return new NoopMigration();
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
            var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

            var u1 = new MigrationTests.TestUpgraderWithPostMigrations(scopeProvider, builder, Mock.Of<IKeyValueService>(), logger, posts,
                new MigrationPlan("Test1", builder, logger).From(string.Empty).To("done"));
            u1.Execute();

            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(0, changed2.CountExecuted);

            var u2 = new MigrationTests.TestUpgraderWithPostMigrations(scopeProvider, builder, Mock.Of<IKeyValueService>(), logger, posts,
                new MigrationPlan("Test2", builder, logger).From(string.Empty).To("done"));
            u2.Execute();

            Assert.AreEqual(1, changed1.CountExecuted);
            Assert.AreEqual(1, changed2.CountExecuted);
        }

        public class Args
        {
            public int CountExecuted { get; set; }
        }

        public class TestPostMigration : IPostMigration
        {
            private readonly string _prodName;
            private readonly Args _changed;

            // need that one else it breaks IoC
            public TestPostMigration()
            {
                _changed = new Args();
            }

            public TestPostMigration(Args changed)
            {
                _changed = changed;
            }

            public TestPostMigration(string prodName, Args changed)
            {
                _prodName = prodName;
                _changed = changed;
            }

            public void Execute(string name, IScope scope, SemVersion originVersion, SemVersion targetVersion, ILogger logger)
            {
                if (_prodName.IsNullOrWhiteSpace() == false && name != _prodName) return;
                _changed.CountExecuted++;
            }
        }
    }
}
