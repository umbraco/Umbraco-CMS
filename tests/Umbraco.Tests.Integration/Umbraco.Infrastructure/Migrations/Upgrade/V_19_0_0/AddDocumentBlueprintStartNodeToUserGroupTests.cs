using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
/// Tests that <see cref="AddDocumentBlueprintStartNodeToUserGroup" /> grants document blueprint access to
/// exactly the groups that could reach blueprints while they lived in the Settings section.
/// </summary>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddDocumentBlueprintStartNodeToUserGroupTests : UmbracoIntegrationTest
{
    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private IShortStringHelper ShortStringHelper => GetRequiredService<IShortStringHelper>();

    [Test]
    public async Task Grants_Access_To_A_Group_With_Settings_Access()
    {
        IUserGroup group = await CreateUserGroupAsync("hasSettings", Constants.Applications.Settings);

        await ExecuteMigrationAsync();

        IUserGroup? migrated = await UserGroupService.GetAsync(group.Key);
        Assert.AreEqual(Constants.System.Root, migrated?.StartDocumentBlueprintId);
    }

    [Test]
    public async Task Does_Not_Grant_Access_To_A_Group_Without_Settings_Access()
    {
        IUserGroup group = await CreateUserGroupAsync("noSettings", Constants.Applications.Content, Constants.Applications.Library);

        await ExecuteMigrationAsync();

        IUserGroup? migrated = await UserGroupService.GetAsync(group.Key);
        Assert.IsNull(migrated?.StartDocumentBlueprintId);
    }

    [Test]
    public async Task Leaves_An_Existing_Start_Node_Alone()
    {
        // Administrators are seeded with root access on a clean install, so the migration must not
        // overwrite a start node that is already set.
        IUserGroup? administrators = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);
        Assert.AreEqual(Constants.System.Root, administrators?.StartDocumentBlueprintId);

        await ExecuteMigrationAsync();

        administrators = await UserGroupService.GetAsync(Constants.Security.AdminGroupKey);
        Assert.AreEqual(Constants.System.Root, administrators?.StartDocumentBlueprintId);
    }

    private async Task<IUserGroup> CreateUserGroupAsync(string alias, params string[] allowedSections)
    {
        var userGroup = new UserGroup(ShortStringHelper)
        {
            Name = alias,
            Alias = alias,
            Icon = "icon-users",
            HasAccessToAllLanguages = true,
        };

        foreach (var allowedSection in allowedSections)
        {
            userGroup.AddAllowedSection(allowedSection);
        }

        Attempt<IUserGroup, UserGroupOperationStatus> result =
            await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, result.Status.ToString());

        return result.Result;
    }

    private async Task ExecuteMigrationAsync()
    {
        MigrationPlan plan = new MigrationPlan(nameof(AddDocumentBlueprintStartNodeToUserGroupTests))
            .From(string.Empty)
            .To<AddDocumentBlueprintStartNodeToUserGroup>("done");

        var executor = new MigrationPlanExecutor(
            GetRequiredService<ICoreScopeProvider>(),
            ScopeAccessor,
            LoggerFactory,
            GetRequiredService<IMigrationBuilder>(),
            GetRequiredService<IUmbracoDatabaseFactory>(),
            new NoopDatabaseCacheRebuilder(),
            GetRequiredService<DistributedCache>(),
            Mock.Of<IKeyValueService>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<AppCaches>(),
            GetRequiredService<IPublishedContentTypeFactory>());

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, string.Empty);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }

    private sealed class NoopDatabaseCacheRebuilder : IDatabaseCacheRebuilder
    {
        public Task<Attempt<DatabaseCacheRebuildResult>> RebuildAsync(bool useBackgroundThread)
            => Task.FromResult(Attempt.Succeed(DatabaseCacheRebuildResult.Success));

        public Task RebuildDatabaseCacheIfSerializerChangedAsync() => throw new NotSupportedException();
    }
}
