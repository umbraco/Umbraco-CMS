// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_18_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddContentVersionKeyColumnTests : UmbracoIntegrationTestWithContent
{
    /// <summary>
    ///     The column is added to a table that already holds versions, and every one of them has to come out of
    ///     the migration with a key of its own - the unique index created straight afterwards depends on it.
    /// </summary>
    [Test]
    public async Task Gives_Every_Existing_Row_A_Distinct_Key()
    {
        if (BaseTestDatabase.IsSqlite() is false)
        {
            Assert.Ignore("The column is dropped and re-added, which this test only does on SQLite.");
        }

        RevertToPreMigrationShape();

        using (IScope scope = ScopeProvider.CreateScope())
        {
            var context = new MigrationContext(
                new PostMigrationPlan(),
                scope.Database,
                NullLogger<MigrationContext>.Instance);

            await new AddContentVersionKeyColumn(context).RunAsync();
            scope.Complete();
        }

        using (IScope scope = ScopeProvider.CreateScope())
        {
            var keys = scope.Database
                .Fetch<string>($"SELECT [{ContentVersionDto.KeyColumnName}] FROM {Constants.DatabaseSchema.Tables.ContentVersion}");
            scope.Complete();

            Assert.That(keys, Is.Not.Empty, "the fixture must leave content versions behind to migrate");
            Assert.Multiple(() =>
            {
                Assert.That(keys, Has.None.EqualTo(Guid.Empty.ToString()).IgnoreCase, "no row may be left on the placeholder default");
                Assert.That(keys.Distinct(StringComparer.OrdinalIgnoreCase).Count(), Is.EqualTo(keys.Count), "keys must be unique");
                Assert.That(keys, Has.All.Matches<string>(key => Guid.TryParse(key, out _)), "every key must be a Guid");
            });
        }
    }

    private void RevertToPreMigrationShape()
    {
        using IScope scope = ScopeProvider.CreateScope();
        scope.Database.Execute("DROP INDEX IF EXISTS [IX_umbracoContentVersion_key]");
        scope.Database.Execute(
            $"ALTER TABLE {Constants.DatabaseSchema.Tables.ContentVersion} DROP COLUMN [{ContentVersionDto.KeyColumnName}]");
        scope.Complete();
    }

    private sealed class PostMigrationPlan : MigrationPlan
    {
        public PostMigrationPlan()
            : base("AddContentVersionKeyColumnTests")
        {
        }
    }
}
