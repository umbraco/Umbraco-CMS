using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_19_0_0;

/// <summary>
///     Adds the <c>hasAccessToInvariantForVariant</c> column to the <c>umbracoUserGroup</c> table
///     and backfills it based on the previous <see cref="ContentSettings.AllowEditInvariantFromNonDefault"/>
///     configuration value.
/// </summary>
/// <remarks>
///     This is the last consumer of <see cref="ContentSettings.AllowEditInvariantFromNonDefault"/>.
///     Do not remove the setting until Umbraco 20 per the obsoletion schedule.
/// </remarks>
public class AddHasAccessToInvariantForVariantToUserGroup : AsyncMigrationBase
{
    private const string ColumnName = "hasAccessToInvariantForVariant";

    private readonly IOptions<ContentSettings> _contentSettings;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AddHasAccessToInvariantForVariantToUserGroup"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    /// <param name="contentSettings">The content settings used to backfill the permission from the previous config flag.</param>
    public AddHasAccessToInvariantForVariantToUserGroup(
        IMigrationContext context,
        IOptions<ContentSettings> contentSettings)
        : base(context)
    {
        _contentSettings = contentSettings;
    }

    /// <inheritdoc />
    protected override Task MigrateAsync()
    {
        if (TableExists(Constants.DatabaseSchema.Tables.UserGroup) is false)
        {
            return Task.CompletedTask;
        }

        if (ColumnExists(Constants.DatabaseSchema.Tables.UserGroup, ColumnName))
        {
            return Task.CompletedTask;
        }

        AddColumn<UserGroupDto>(Constants.DatabaseSchema.Tables.UserGroup, ColumnName);

        BackfillFromLegacySetting();

        return Task.CompletedTask;
    }

    private void BackfillFromLegacySetting()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var wasPermissive = _contentSettings.Value.AllowEditInvariantFromNonDefault;
#pragma warning restore CS0618 // Type or member is obsolete

        var userGroupTable = SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.UserGroup);
        var userGroup2LanguageTable = SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.UserGroup2Language);
        var languageTable = SqlSyntax.GetQuotedTableName(Constants.DatabaseSchema.Tables.Language);
        var columnName = SqlSyntax.GetQuotedColumnName(ColumnName);
        var hasAccessToAllLanguagesColumn = SqlSyntax.GetQuotedColumnName("hasAccessToAllLanguages");
        var idColumn = SqlSyntax.GetQuotedColumnName("id");
        var userGroupIdColumn = SqlSyntax.GetQuotedColumnName("userGroupId");
        var languageIdColumn = SqlSyntax.GetQuotedColumnName("languageId");
        var isDefaultVariantLangColumn = SqlSyntax.GetQuotedColumnName("isDefaultVariantLang");

        if (wasPermissive)
        {
            // The old setting was permissive at the tenant level: every group gets the permission.
            Database.Execute($"UPDATE {userGroupTable} SET {columnName} = 1");
            return;
        }

        // The old setting was restrictive: only grant to groups that could already edit invariant data
        // under the old default-language coupling, plus the built-in admin group (id = 1).
        Database.Execute(
            $@"UPDATE {userGroupTable}
               SET {columnName} = 0
               WHERE {hasAccessToAllLanguagesColumn} = 0
                 AND {idColumn} <> 1
                 AND {idColumn} NOT IN (
                     SELECT l.{userGroupIdColumn}
                     FROM {userGroup2LanguageTable} l
                     INNER JOIN {languageTable} lang ON l.{languageIdColumn} = lang.{idColumn}
                     WHERE lang.{isDefaultVariantLangColumn} = 1
                 )");
    }
}
