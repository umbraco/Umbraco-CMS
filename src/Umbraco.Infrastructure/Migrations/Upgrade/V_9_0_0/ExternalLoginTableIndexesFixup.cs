namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

/// <summary>
///     Fixes up the original <see cref="ExternalLoginTableIndexes" /> for post RC release to ensure that
///     the correct indexes are applied.
/// </summary>
public class ExternalLoginTableIndexesFixup : MigrationBase
{
    public ExternalLoginTableIndexesFixup(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var indexName1 = "IX_" + ExternalLoginTokenTable.LegacyExternalLoginDto.TableName + "_LoginProvider";
        var indexName2 = "IX_" + ExternalLoginTokenTable.LegacyExternalLoginDto.TableName + "_ProviderKey";

        if (IndexExists(indexName1))
        {
            // drop it since the previous migration index was wrong, and we
            // need to modify a column that belons to it
            Delete.Index(indexName1).OnTable(ExternalLoginTokenTable.LegacyExternalLoginDto.TableName).Do();
        }

        if (IndexExists(indexName2))
        {
            // drop since it's using a column we're about to modify
            Delete.Index(indexName2).OnTable(ExternalLoginTokenTable.LegacyExternalLoginDto.TableName).Do();
        }

        // then fixup the length of the loginProvider column
        AlterColumn<ExternalLoginTokenTable.LegacyExternalLoginDto>(
            ExternalLoginTokenTable.LegacyExternalLoginDto.TableName, "loginProvider");

        // create it with the correct definition
        Create
            .Index(indexName1)
            .OnTable(ExternalLoginTokenTable.LegacyExternalLoginDto.TableName)
            .OnColumn("loginProvider").Ascending()
            .OnColumn("userId").Ascending()
            .WithOptions()
            .Unique()
            .WithOptions()
            .NonClustered()
            .Do();

        // re-create the original
        Create
            .Index(indexName2)
            .OnTable(ExternalLoginTokenTable.LegacyExternalLoginDto.TableName)
            .OnColumn("loginProvider").Ascending()
            .OnColumn("providerKey").Ascending()
            .WithOptions()
            .NonClustered()
            .Do();
    }
}
