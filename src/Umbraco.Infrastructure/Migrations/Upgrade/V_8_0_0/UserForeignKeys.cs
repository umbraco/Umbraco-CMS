using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_0_0;

/// <summary>
///     Creates/Updates non mandatory FK columns to the user table
/// </summary>
public class UserForeignKeys : MigrationBase
{
    public UserForeignKeys(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        // first allow NULL-able
        Alter.Table(ContentVersionCultureVariationDto.TableName).AlterColumn("availableUserId").AsInt32().Nullable()
            .Do();
        Alter.Table(ContentVersionDto.TableName).AlterColumn("userId").AsInt32().Nullable().Do();
        Alter.Table(Constants.DatabaseSchema.Tables.Log).AlterColumn("userId").AsInt32().Nullable().Do();
        Alter.Table(NodeDto.TableName).AlterColumn("nodeUser").AsInt32().Nullable().Do();

        // then we can update any non existing users to NULL
        Execute.Sql(
                $"UPDATE {ContentVersionCultureVariationDto.TableName} SET availableUserId = NULL WHERE availableUserId NOT IN (SELECT id FROM {UserDto.TableName})")
            .Do();
        Execute.Sql(
                $"UPDATE {ContentVersionDto.TableName} SET userId = NULL WHERE userId NOT IN (SELECT id FROM {UserDto.TableName})")
            .Do();
        Execute.Sql(
                $"UPDATE {Constants.DatabaseSchema.Tables.Log} SET userId = NULL WHERE userId NOT IN (SELECT id FROM {UserDto.TableName})")
            .Do();
        Execute.Sql(
                $"UPDATE {NodeDto.TableName} SET nodeUser = NULL WHERE nodeUser NOT IN (SELECT id FROM {UserDto.TableName})")
            .Do();

        // FKs will be created after migrations
    }
}
