// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Migrations;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade;
using Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Scoping;
using IScope = Umbraco.Cms.Infrastructure.Scoping.IScope;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Infrastructure.Migrations.Upgrade.V_19_0_0;

[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.NewSchemaPerTest)]
internal sealed class AddHasAccessToInvariantForVariantToUserGroupTests : UmbracoIntegrationTest
{
    private const string ColumnName = "hasAccessToInvariantForVariant";

    private const string PremigrationStateBeforeColumn = "{31C0D92A-49DD-47EC-B2A7-932A58FF224E}";

    private IUserGroupService UserGroupService => GetRequiredService<IUserGroupService>();

    private ILanguageService LanguageService => GetRequiredService<ILanguageService>();

    [Test]
    public async Task Grants_Access_Only_To_Groups_That_Could_Edit_The_Default_Language_When_Legacy_Setting_Is_Restrictive()
    {
        var groups = await CreateUserGroupsAsync();

        await ExecuteMigrationAsync(allowEditInvariantFromNonDefault: false);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(GetColumnValue(Constants.Security.AdminGroupKey), "admin group");
            Assert.IsTrue(GetColumnValue(groups.AllLanguages), "group with access to all languages");
            Assert.IsTrue(GetColumnValue(groups.DefaultLanguage), "group with access to the default language");
            Assert.IsFalse(GetColumnValue(groups.NonDefaultLanguage), "group with access to a non-default language only");
            Assert.IsFalse(GetColumnValue(groups.NoLanguages), "group without language access");
        });
    }

    [Test]
    public async Task Grants_Access_To_All_Groups_When_Legacy_Setting_Is_Permissive()
    {
        var groups = await CreateUserGroupsAsync();

        await ExecuteMigrationAsync(allowEditInvariantFromNonDefault: true);

        Assert.Multiple(() =>
        {
            Assert.IsTrue(GetColumnValue(Constants.Security.AdminGroupKey), "admin group");
            Assert.IsTrue(GetColumnValue(groups.AllLanguages), "group with access to all languages");
            Assert.IsTrue(GetColumnValue(groups.DefaultLanguage), "group with access to the default language");
            Assert.IsTrue(GetColumnValue(groups.NonDefaultLanguage), "group with access to a non-default language only");
            Assert.IsTrue(GetColumnValue(groups.NoLanguages), "group without language access");
        });
    }

    [Test]
    public async Task Earlier_User_Group_Migrations_Can_Run_When_Upgrading_From_Before_The_Column_Existed()
    {
        DropColumn();

        await ExecutePlanAsync(new UmbracoPremigrationPlan(), PremigrationStateBeforeColumn);
        await ExecutePlanAsync(
            new MigrationPlan(nameof(AddHasAccessToInvariantForVariantToUserGroupTests))
                .From(string.Empty)
                .To<global::Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_18_0_0.AddElementSectionForAdmins>("elements")
                .To<AddHasAccessToInvariantForVariantToUserGroup>("done"),
            string.Empty);

        Assert.IsTrue(GetColumnValue(Constants.Security.AdminGroupKey), "admin group");
    }

    private async Task<(Guid AllLanguages, Guid DefaultLanguage, Guid NonDefaultLanguage, Guid NoLanguages)> CreateUserGroupsAsync()
    {
        ILanguage defaultLanguage = (await LanguageService.GetDefaultLanguageAsync())!;
        ILanguage nonDefaultLanguage = new LanguageBuilder().WithCultureInfo("da-DK").Build();
        await LanguageService.CreateAsync(nonDefaultLanguage, Constants.Security.SuperUserKey);

        return (
            await CreateUserGroupAsync("AllLanguages", group => group.HasAccessToAllLanguages = true),
            await CreateUserGroupAsync("DefaultLanguage", group => group.AddAllowedLanguage(defaultLanguage.Id)),
            await CreateUserGroupAsync("NonDefaultLanguage", group => group.AddAllowedLanguage(nonDefaultLanguage.Id)),
            await CreateUserGroupAsync("NoLanguages", _ => { }));
    }

    private async Task<Guid> CreateUserGroupAsync(string suffix, Action<IUserGroup> configure)
    {
        IUserGroup userGroup = UserGroupBuilder.CreateUserGroup(suffix: suffix);
        userGroup.HasAccessToAllLanguages = false;
        configure(userGroup);

        var result = await UserGroupService.CreateAsync(userGroup, Constants.Security.SuperUserKey);
        Assert.IsTrue(result.Success, $"Could not create user group {suffix}: {result.Status}");
        return userGroup.Key;
    }

    private async Task ExecuteMigrationAsync(bool allowEditInvariantFromNonDefault)
    {
        DropColumn();

#pragma warning disable CS0618 // Type or member is obsolete
        GetRequiredService<IOptions<ContentSettings>>().Value.AllowEditInvariantFromNonDefault = allowEditInvariantFromNonDefault;
#pragma warning restore CS0618 // Type or member is obsolete

        await ExecutePlanAsync(new UmbracoPremigrationPlan(), PremigrationStateBeforeColumn);
        await ExecutePlanAsync(
            new MigrationPlan(nameof(AddHasAccessToInvariantForVariantToUserGroupTests))
                .From(string.Empty)
                .To<AddHasAccessToInvariantForVariantToUserGroup>("done"),
            string.Empty);
    }

    private async Task ExecutePlanAsync(MigrationPlan plan, string fromState)
    {
        var executor = new MigrationPlanExecutor(
            GetRequiredService<ICoreScopeProvider>(),
            ScopeAccessor,
            LoggerFactory,
            GetRequiredService<IMigrationBuilder>(),
            GetRequiredService<IUmbracoDatabaseFactory>(),
            Mock.Of<IDatabaseCacheRebuilder>(),
            GetRequiredService<DistributedCache>(),
            Mock.Of<IKeyValueService>(),
            GetRequiredService<IServiceScopeFactory>(),
            GetRequiredService<AppCaches>(),
            GetRequiredService<IPublishedContentTypeFactory>());

        ExecutedMigrationPlan result = await executor.ExecutePlanAsync(plan, fromState);

        Assert.That(result.Successful, Is.True, result.Exception?.ToString());
    }

    private void DropColumn()
    {
        using IScope scope = ScopeProvider.CreateScope();
        var table = scope.SqlContext.SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.UserGroup);
        var column = scope.SqlContext.SqlSyntax.GetQuotedColumnName(ColumnName);

        if (BaseTestDatabase.IsSqlite() is false)
        {
            scope.Database.Execute($"ALTER TABLE {table} DROP CONSTRAINT {scope.SqlContext.SqlSyntax.GetQuotedName($"DF_{Constants.DatabaseSchema.Tables.UserGroup}_{ColumnName}")}");
        }

        scope.Database.Execute($"ALTER TABLE {table} DROP COLUMN {column}");
        scope.Complete();
    }

    private bool GetColumnValue(Guid userGroupKey)
    {
        using IScope scope = ScopeProvider.CreateScope(autoComplete: true);
        var table = scope.SqlContext.SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.UserGroup);
        var column = scope.SqlContext.SqlSyntax.GetQuotedColumnName(ColumnName);
        var keyColumn = scope.SqlContext.SqlSyntax.GetQuotedColumnName("key");

        return scope.Database.ExecuteScalar<bool>($"SELECT {column} FROM {table} WHERE {keyColumn} = @0", userGroupKey);
    }
}
