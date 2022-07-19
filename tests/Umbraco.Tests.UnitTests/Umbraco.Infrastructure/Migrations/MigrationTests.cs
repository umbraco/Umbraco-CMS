// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Data;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using IScopeProvider = Umbraco.Cms.Infrastructure.Scoping.IScopeProvider;
#if DEBUG_SCOPES
using System.Collections.Generic;
#endif

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

[TestFixture]
public class MigrationTests
{
    public class TestScopeProvider : IScopeProvider, IScopeAccessor
    {
        public TestScopeProvider(IScope scope) => AmbientScope = scope;

        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            IScopedNotificationPublisher notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false) => AmbientScope;

        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            IScopedNotificationPublisher notificationPublisher = null,
            bool? scopeFileSystems = null) => throw new NotImplementedException();

        public void AttachScope(IScope scope, bool callContext = false) => throw new NotImplementedException();

        public IScope DetachScope() => throw new NotImplementedException();

        public IScopeContext Context { get; set; }

        public IQuery<T> CreateQuery<T>() => SqlContext.Query<T>();

        public ISqlContext SqlContext { get; set; }

#if DEBUG_SCOPES
            public ScopeInfo GetScopeInfo(IScope scope)
            {
                throw new NotImplementedException();
            }
            public IEnumerable<ScopeInfo> ScopeInfos => throw new NotImplementedException();
#endif
        public IScope AmbientScope { get; }
    }

    private class TestPlan : MigrationPlan
    {
        public TestPlan()
            : base("Test")
        {
        }
    }

    private MigrationContext GetMigrationContext() =>
        new(
            new TestPlan(),
            Mock.Of<IUmbracoDatabase>(),
            Mock.Of<ILogger<MigrationContext>>());

    [Test]
    public void RunGoodMigration()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new GoodMigration(migrationContext);
        migration.Run();
    }

    [Test]
    public void DetectBadMigration1()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new BadMigration1(migrationContext);
        Assert.Throws<IncompleteMigrationExpressionException>(() => migration.Run());
    }

    [Test]
    public void DetectBadMigration2()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new BadMigration2(migrationContext);
        Assert.Throws<IncompleteMigrationExpressionException>(() => migration.Run());
    }

    public class GoodMigration : MigrationBase
    {
        public GoodMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Execute.Sql(string.Empty).Do();
    }

    public class BadMigration1 : MigrationBase
    {
        public BadMigration1(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Alter.Table("foo"); // stop here, don't Do it
    }

    public class BadMigration2 : MigrationBase
    {
        public BadMigration2(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate()
        {
            Alter.Table("foo"); // stop here, don't Do it

            // and try to start another one
            Alter.Table("bar");
        }
    }
}
