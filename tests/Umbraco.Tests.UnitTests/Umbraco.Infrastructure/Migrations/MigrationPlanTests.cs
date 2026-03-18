// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NPoco;
using NUnit.Framework;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Persistence.SqlServer.Services;
using Umbraco.Cms.Tests.Common.TestHelpers;
using Umbraco.Cms.Tests.UnitTests.TestHelpers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Migrations;

/// <summary>
/// Provides unit tests for the <see cref="MigrationPlan"/> class within the Umbraco infrastructure migrations namespace.
/// These tests verify the behavior and correctness of migration planning functionality.
/// </summary>
[TestFixture]
public class MigrationPlanTests
{
    /// <summary>
    /// Tests that a migration plan can be executed successfully.
    /// Verifies that the final migration state is as expected and that the correct database operations are performed.
    /// </summary>
    /// <returns>A task representing the asynchronous test operation.</returns>
    [Test]
    public async Task CanExecute()
    {
        var loggerFactory = NullLoggerFactory.Instance;

        var database = new TestDatabase();
        var scope = Mock.Of<IScope>(x => x.Notifications == Mock.Of<IScopedNotificationPublisher>());
        Mock.Get(scope)
            .Setup(x => x.Database)
            .Returns(database);

        var databaseFactory = Mock.Of<IUmbracoDatabaseFactory>();
        Mock.Get(databaseFactory)
            .Setup(x => x.CreateDatabase())
            .Returns(database);

        var sqlContext = new SqlContext(
            new SqlServerSyntaxProvider(Options.Create(new GlobalSettings())),
            DatabaseType.SQLCe,
            Mock.Of<IPocoDataFactory>());
        var scopeProvider = new MigrationTests.TestScopeProvider(scope) { SqlContext = sqlContext };

        var migrationBuilder = Mock.Of<IMigrationBuilder>();
        Mock.Get(migrationBuilder)
            .Setup(x => x.Build(It.IsAny<Type>(), It.IsAny<IMigrationContext>()))
            .Returns<Type, IMigrationContext>((t, c) =>
            {
                switch (t.Name)
                {
                    case "DeleteRedirectUrlTable":
                        return new DeleteRedirectUrlTable(c);
                    case "NoopMigration":
                        return new NoopMigration(c);
                    default:
                        throw new NotSupportedException();
                }
            });

        var distributedCache = new DistributedCache(
            Mock.Of<IServerMessenger>(),
            new CacheRefresherCollection(() => Enumerable.Empty<ICacheRefresher>()));

        var isolatedCaches = new IsolatedCaches(type => NoAppCache.Instance);

        var appCaches = new AppCaches(Mock.Of<IAppPolicyCache>(), Mock.Of<IRequestCache>(), isolatedCaches);

        var executor = new MigrationPlanExecutor(
            scopeProvider,
            scopeProvider,
            loggerFactory,
            migrationBuilder,
            databaseFactory,
            Mock.Of<IDatabaseCacheRebuilder>(),
            distributedCache,
            Mock.Of<IKeyValueService>(),
            Mock.Of<IServiceScopeFactory>(),
            appCaches,
            Mock.Of<IPublishedContentTypeFactory>());

        var plan = new MigrationPlan("default")
            .From(string.Empty)
            .To<DeleteRedirectUrlTable>("{4A9A1A8F-0DA1-4BCF-AD06-C19D79152E35}")
            .To<NoopMigration>("VERSION.33");

        var kvs = Mock.Of<IKeyValueService>();
        Mock.Get(kvs).Setup(x => x.GetValue(It.IsAny<string>()))
            .Returns<string>(k => k == "Umbraco.Tests.MigrationPlan" ? string.Empty : null);

        string state;
        using (var s = scopeProvider.CreateScope())
        {
            // read current state
            var sourceState = kvs.GetValue("Umbraco.Tests.MigrationPlan") ?? string.Empty;

            // execute plan
            var result = await executor.ExecutePlanAsync(plan, sourceState).ConfigureAwait(false);
            state = result.FinalState;

            // save new state
            kvs.SetValue("Umbraco.Tests.MigrationPlan", sourceState, state);

            s.Complete();
        }

        Assert.AreEqual("VERSION.33", state);
        Assert.AreEqual(1, database.Operations.Count);
        Assert.AreEqual("DROP TABLE [umbracoRedirectUrl]", database.Operations[0].Sql);
    }

    /// <summary>
    /// Tests that migrations can be added to a migration plan.
    /// </summary>
    [Test]
    public void CanAddMigrations()
    {
        var plan = new MigrationPlan("default");
        plan
            .From(string.Empty)
            .To("aaa")
            .To("bbb")
            .To("ccc");
    }

    /// <summary>
    /// Tests that transitioning to the same state throws an ArgumentException.
    /// </summary>
    [Test]
    public void CannotTransitionToSameState()
    {
        var plan = new MigrationPlan("default");
        Assert.Throws<ArgumentException>(() => plan.From("aaa").To("aaa"));
    }

    /// <summary>
    /// Tests that only one transition is allowed per state in the migration plan.
    /// </summary>
    [Test]
    public void OnlyOneTransitionPerState()
    {
        var plan = new MigrationPlan("default");
        plan.From("aaa").To("bbb");
        Assert.Throws<InvalidOperationException>(() => plan.From("aaa").To("ccc"));
    }

    /// <summary>
    /// Tests that a migration plan cannot contain two or more heads.
    /// </summary>
    [Test]
    public void CannotContainTwoMoreHeads()
    {
        var plan = new MigrationPlan("default");
        plan
            .From(string.Empty)
            .To("aaa")
            .To("bbb")
            .From("ccc")
            .To("ddd");
        Assert.Throws<InvalidOperationException>(() => plan.Validate());
    }

    /// <summary>
    /// Tests that the migration plan cannot contain loops and throws an exception if a loop is detected.
    /// </summary>
    [Test]
    public void CannotContainLoops()
    {
        var plan = new MigrationPlan("default");
        plan
            .From("aaa")
            .To("bbb")
            .To("ccc")
            .To("aaa");
        Assert.Throws<InvalidOperationException>(() => plan.Validate());
    }

    /// <summary>
    /// Validates the Umbraco migration plan to ensure it is correctly configured and its final state is valid.
    /// </summary>
    [Test]
    public void ValidateUmbracoPlan()
    {
        var plan = new UmbracoPlan(TestHelper.GetUmbracoVersion());
        plan.Validate();
        Console.WriteLine(plan.FinalState);
        Assert.IsFalse(plan.FinalState.IsNullOrWhiteSpace());
    }

    /// <summary>
    /// Tests that a MigrationPlan can be cloned correctly and that the cloned plan
    /// maintains the expected migration paths.
    /// </summary>
    [Test]
    public void CanClone()
    {
        var plan = new MigrationPlan("default");
        plan
            .From(string.Empty)
            .To("aaa")
            .To("bbb")
            .To("ccc")
            .To("ddd")
            .To("eee");

        plan
            .From("xxx")
            .ToWithClone("bbb", "ddd", "yyy")
            .To("eee");

        WritePlanToConsole(plan);

        plan.Validate();
        Assert.AreEqual("eee", plan.FollowPath("xxx").Last());
        Assert.AreEqual("yyy", plan.FollowPath("xxx", "yyy").Last());
    }

    /// <summary>
    /// Tests that migration plans can be merged correctly and the resulting plan follows the expected path.
    /// </summary>
    [Test]
    public void CanMerge()
    {
        var plan = new MigrationPlan("default");
        plan
            .From(string.Empty)
            .To("aaa")
            .Merge()
            .To("bbb")
            .To("ccc")
            .With()
            .To("ddd")
            .To("eee")
            .As("fff")
            .To("ggg");

        WritePlanToConsole(plan);

        plan.Validate();
        AssertList(plan.FollowPath(), string.Empty, "aaa", "bbb", "ccc", "*", "*", "fff", "ggg");
        AssertList(plan.FollowPath("ccc"), "ccc", "*", "*", "fff", "ggg");
        AssertList(plan.FollowPath("eee"), "eee", "*", "*", "fff", "ggg");
    }

    private void AssertList(IReadOnlyList<string> states, params string[] expected)
    {
        Assert.AreEqual(expected.Length, states.Count, string.Join(", ", states));
        for (var i = 0; i < expected.Length; i++)
        {
            if (expected[i] != "*")
            {
                Assert.AreEqual(expected[i], states[i], "at:" + i);
            }
        }
    }

    private void WritePlanToConsole(MigrationPlan plan)
    {
        var final = plan.Transitions.First(x => x.Value == null).Key;

        Console.WriteLine("plan \"{0}\" to final state \"{1}\":", plan.Name, final);
        foreach ((var _, var transition) in plan.Transitions)
        {
            if (transition != null)
            {
                Console.WriteLine(transition);
            }
        }
    }

    /// <summary>
    /// Tests the migration plan responsible for deleting the RedirectUrl table.
    /// </summary>
    public class DeleteRedirectUrlTable : MigrationBase
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="DeleteRedirectUrlTable"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
        public DeleteRedirectUrlTable(IMigrationContext context)
            : base(context)
        {
        }

        protected override void Migrate() => Delete.Table("umbracoRedirectUrl").Do();
    }
}
