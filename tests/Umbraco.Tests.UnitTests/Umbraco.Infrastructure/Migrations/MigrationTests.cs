// Copyright (c) Umbraco.
// See LICENSE for more details.

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

/// <summary>
/// Provides unit tests for the migration-related functionality within the Umbraco CMS infrastructure layer.
/// </summary>
[TestFixture]
public class MigrationTests
{
    /// <summary>
    /// Unit test that verifies the behavior of the scope provider used during migration operations in Umbraco.
    /// Ensures that the scope provider integrates correctly with migration logic.
    /// </summary>
    public class TestScopeProvider : IScopeProvider, IScopeAccessor
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestScopeProvider"/> class with the specified scope.
    /// </summary>
    /// <param name="scope">The <see cref="IScope"/> instance to be used by the provider for managing scope operations.</param>
        public TestScopeProvider(IScope scope) => AmbientScope = scope;

    /// <summary>
    /// Returns the ambient scope instance, ignoring all provided parameters.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the scope. This parameter is ignored in this implementation.</param>
    /// <param name="repositoryCacheMode">The repository cache mode to use. This parameter is ignored in this implementation.</param>
    /// <param name="eventDispatcher">The event dispatcher to use within the scope. This parameter is ignored in this implementation.</param>
    /// <param name="notificationPublisher">The scoped notification publisher to use. This parameter is ignored in this implementation.</param>
    /// <param name="scopeFileSystems">Indicates whether to scope file systems. This parameter is ignored in this implementation.</param>
    /// <param name="callContext">Indicates whether to use call context. This parameter is ignored in this implementation.</param>
    /// <param name="autoComplete">Indicates whether the scope should auto-complete. This parameter is ignored in this implementation.</param>
    /// <returns>The ambient <see cref="IScope"/> instance.</returns>
        public IScope CreateScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            IScopedNotificationPublisher notificationPublisher = null,
            bool? scopeFileSystems = null,
            bool callContext = false,
            bool autoComplete = false) => AmbientScope;

    /// <summary>
    /// Creates a detached scope with the specified options.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the scope.</param>
    /// <param name="repositoryCacheMode">The repository cache mode to use.</param>
    /// <param name="eventDispatcher">The event dispatcher to use within the scope.</param>
    /// <param name="notificationPublisher">The scoped notification publisher to use.</param>
    /// <param name="scopeFileSystems">Indicates whether to scope file systems.</param>
    /// <returns>An <see cref="IScope"/> instance representing the detached scope.</returns>
        public IScope CreateDetachedScope(
            IsolationLevel isolationLevel = IsolationLevel.Unspecified,
            RepositoryCacheMode repositoryCacheMode = RepositoryCacheMode.Unspecified,
            IEventDispatcher eventDispatcher = null,
            IScopedNotificationPublisher notificationPublisher = null,
            bool? scopeFileSystems = null) => throw new NotImplementedException();

    /// <summary>
    /// Attaches the specified <paramref name="scope"/> to the current context.
    /// </summary>
    /// <param name="scope">The <see cref="IScope"/> instance to attach.</param>
    /// <param name="callContext">If set to <c>true</c>, attaches the scope using the call context; otherwise, attaches it to the current thread context.</param>
        public void AttachScope(IScope scope, bool callContext = false) => throw new NotImplementedException();

    /// <summary>
    /// Detaches the current scope.
    /// </summary>
    /// <returns>The detached <see cref="IScope"/>.</returns>
        public IScope DetachScope() => throw new NotImplementedException();

    /// <summary>
    /// Gets or sets the <see cref="IScopeContext"/> used for managing scope state within the test scope provider.
    /// </summary>
        public IScopeContext Context { get; set; }

    /// <summary>
    /// Creates a new query for the specified entity type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of entity to query.</typeparam>
    /// <returns>An <see cref="IQuery{T}"/> instance for the specified entity type.</returns>
        public IQuery<T> CreateQuery<T>() => SqlContext.Query<T>();

        /// <summary>
        /// Gets or sets the <see cref="ISqlContext"/> instance used by the test scope provider.
        /// This context is typically used for executing SQL operations within migration tests.
        /// </summary>
        public ISqlContext SqlContext { get; set; }

    /// <summary>
    /// Gets the current ambient scope.
    /// </summary>
        public IScope AmbientScope { get; }

#if DEBUG_SCOPES
        public IEnumerable<ScopeInfo> ScopeInfos => throw new NotImplementedException();

        public ScopeInfo GetScopeInfo(IScope scope) => throw new NotImplementedException();
#endif

    }

    private class TestPlan : MigrationPlan
    {
    /// <summary>
    /// Initializes a new, empty instance of the <see cref="TestPlan"/> class.
    /// </summary>
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

    /// <summary>
    /// Runs a migration that is expected to complete successfully.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    [Test]
    public async Task RunGoodMigration()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new GoodMigration(migrationContext);
        await migration.RunAsync();
    }

    /// <summary>
    /// Tests that the BadMigration1 migration throws an IncompleteMigrationExpressionException when run.
    /// </summary>
    [Test]
    public void DetectBadMigration1()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new BadMigration1(migrationContext);
        Assert.ThrowsAsync<IncompleteMigrationExpressionException>(migration.RunAsync);
    }

    /// <summary>
    /// Tests that the BadMigration2 migration throws an IncompleteMigrationExpressionException when run.
    /// </summary>
    [Test]
    public void DetectBadMigration2()
    {
        var migrationContext = GetMigrationContext();
        MigrationBase migration = new BadMigration2(migrationContext);
        Assert.ThrowsAsync<IncompleteMigrationExpressionException>(migration.RunAsync);
    }

    /// <summary>
    /// Represents a well-formed migration used for testing successful migration scenarios.
    /// </summary>
    public class GoodMigration : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="GoodMigration"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
        public GoodMigration(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Execute.Sql(string.Empty).Do();
    }

    /// <summary>
    /// Represents a bad migration for testing purposes.
    /// </summary>
    public class BadMigration1 : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="BadMigration1"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
        public BadMigration1(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Alter.Table("foo"); // stop here, don't Do it
    }

    /// <summary>
    /// Represents a bad migration used for testing purposes.
    /// </summary>
    public class BadMigration2 : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="BadMigration2"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
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
