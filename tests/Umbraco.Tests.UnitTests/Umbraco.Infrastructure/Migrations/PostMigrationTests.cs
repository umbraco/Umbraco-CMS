// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Migrations;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class PostMigrationTests
{
    private static readonly ILoggerFactory s_loggerFactory = NullLoggerFactory.Instance;

    private IMigrationPlanExecutor GetMigrationPlanExecutor(
        ICoreScopeProvider scopeProvider,
        IScopeAccessor scopeAccessor,
        IMigrationBuilder builder)
        => new MigrationPlanExecutor(scopeProvider, scopeAccessor, s_loggerFactory, builder);

    [Test]
    public void ExecutesPlanPostMigration()
    {
        var builder = Mock.Of<IMigrationBuilder>();
        Mock.Get(builder)
            .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
            .Returns<Type, IMigrationContext>((t, c) =>
            {
                switch (t.Name)
                {
                    case nameof(NoopMigration):
                        return new NoopMigration(c);
                    case nameof(TestPostMigration):
                        return new TestPostMigration(c);
                    default:
                        throw new NotSupportedException();
                }
            });

        var database = new TestDatabase();
        var scope = Mock.Of<IScope>(x => x.Notifications == Mock.Of<IScopedNotificationPublisher>());
        Mock.Get(scope)
            .Setup(x => x.Database)
            .Returns(database);

        var sqlContext = new SqlContext(
            new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())),
            DatabaseType.SQLCe,
            Mock.Of<IPocoDataFactory>());
        var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

        var plan = new MigrationPlan("Test")
            .From(string.Empty).To("done");

        plan.AddPostMigration<TestPostMigration>();
        TestPostMigration.MigrateCount = 0;

        var upgrader = new Upgrader(plan);
        var executor = GetMigrationPlanExecutor(scopeProvider, scopeProvider, builder);
        upgrader.Execute(
            executor,
            scopeProvider,
            Mock.Of<IKeyValueService>());

        Assert.AreEqual(1, TestPostMigration.MigrateCount);
    }

    [Test]
    public void MigrationCanAddPostMigration()
    {
        var builder = Mock.Of<IMigrationBuilder>();
        Mock.Get(builder)
            .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
            .Returns<Type, IMigrationContext>((t, c) =>
            {
                switch (t.Name)
                {
                    case nameof(NoopMigration):
                        return new NoopMigration(c);
                    case nameof(TestMigration):
                        return new TestMigration(c);
                    case nameof(TestPostMigration):
                        return new TestPostMigration(c);
                    default:
                        throw new NotSupportedException();
                }
            });

        var database = new TestDatabase();
        var scope = Mock.Of<IScope>(x => x.Notifications == Mock.Of<IScopedNotificationPublisher>());
        Mock.Get(scope)
            .Setup(x => x.Database)
            .Returns(database);

        var sqlContext = new SqlContext(
            new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())),
            DatabaseType.SQLCe,
            Mock.Of<IPocoDataFactory>());
        var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

        var plan = new MigrationPlan("Test")
            .From(string.Empty).To<TestMigration>("done");

        TestMigration.MigrateCount = 0;
        TestPostMigration.MigrateCount = 0;

        new MigrationContext(plan, database, s_loggerFactory.CreateLogger<MigrationContext>());

        var upgrader = new Upgrader(plan);
        var executor = GetMigrationPlanExecutor(scopeProvider, scopeProvider, builder);
        upgrader.Execute(
            executor,
            scopeProvider,
            Mock.Of<IKeyValueService>());

        Assert.AreEqual(1, TestMigration.MigrateCount);
        Assert.AreEqual(1, TestPostMigration.MigrateCount);
    }

    public class TestMigration : MigrationBase
    {
        public TestMigration(IMigrationContext context)
            : base(context)
        {
        }

        public static int MigrateCount { get; set; }

        protected override void Migrate()
        {
            MigrateCount++;

            Context.AddPostMigration<TestPostMigration>();
        }
    }

    public class TestPostMigration : MigrationBase
    {
        public TestPostMigration(IMigrationContext context)
            : base(context)
        {
        }

        public static int MigrateCount { get; set; }

        protected override void Migrate() => MigrateCount++;
    }
}
