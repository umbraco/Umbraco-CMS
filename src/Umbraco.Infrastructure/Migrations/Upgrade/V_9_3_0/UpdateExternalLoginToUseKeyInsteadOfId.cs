using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_3_0;

public class UpdateExternalLoginToUseKeyInsteadOfId : MigrationBase
{
    public UpdateExternalLoginToUseKeyInsteadOfId(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!ColumnExists(ExternalLoginDto.TableName, "userOrMemberKey"))
        {
            var indexNameToRecreate = "IX_" + ExternalLoginDto.TableName + "_LoginProvider";
            var indexNameToDelete = "IX_" + ExternalLoginDto.TableName + "_userId";

            if (IndexExists(indexNameToRecreate))
            {
                // drop it since the previous migration index was wrong, and we
                // need to modify a column that belons to it
                Delete.Index(indexNameToRecreate).OnTable(ExternalLoginDto.TableName).Do();
            }

            if (IndexExists(indexNameToDelete))
            {
                // drop it since the previous migration index was wrong, and we
                // need to modify a column that belons to it
                Delete.Index(indexNameToDelete).OnTable(ExternalLoginDto.TableName).Do();
            }

            // special trick to add the column without constraints and return the sql to add them later
            AddColumn<ExternalLoginDto>("userOrMemberKey", out IEnumerable<string> sqls);

            // populate the new columns with the userId as a Guid. Same method as IntExtensions.ToGuid.
            Execute.Sql(
                    $"UPDATE {ExternalLoginDto.TableName} SET userOrMemberKey = CAST(CONVERT(char(8), CONVERT(BINARY(4), userId), 2) + '-0000-0000-0000-000000000000' AS UNIQUEIDENTIFIER)")
                .Do();

            // now apply constraints (NOT NULL) to new table
            foreach (var sql in sqls)
            {
                Execute.Sql(sql).Do();
            }

            // now remove these old columns
            Delete.Column("userId").FromTable(ExternalLoginDto.TableName).Do();

            // create index with the correct definition
            Create
                .Index(indexNameToRecreate)
                .OnTable(ExternalLoginDto.TableName)
                .OnColumn("loginProvider").Ascending()
                .OnColumn("userOrMemberKey").Ascending()
                .WithOptions()
                .Unique()
                .WithOptions()
                .NonClustered()
                .Do();
        }
    }
}
