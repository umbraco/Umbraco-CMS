using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    /// <summary>
    /// Fixes up the original <see cref="ExternalLoginTableIndexes"/> for post RC release to ensure that
    /// the correct indexes are applied.
    /// </summary>
    public class ExternalLoginTableIndexesFixup : MigrationBase
    {
        public ExternalLoginTableIndexesFixup(IMigrationContext context) : base(context)
        {
        }

        protected override void Migrate()
        {
            var indexName1 = "IX_" + ExternalLoginDto.TableName + "_LoginProvider";

            if (IndexExists(indexName1))
            {
                // drop it since the previous migration index was wrong
                Delete.Index(indexName1).OnTable(ExternalLoginDto.TableName).Do();

                // create it with the correct definition
                Create
                     .Index(indexName1)
                     .OnTable(ExternalLoginDto.TableName)
                     .OnColumn("loginProvider").Ascending()
                     .OnColumn("userId").Ascending()
                     .WithOptions()
                     .Unique()
                     .WithOptions()
                     .NonClustered()
                     .Do();
            }

            // then fixup the length of the loginProvider column
            AlterColumn<ExternalLoginDto>(ExternalLoginDto.TableName, "loginProvider");
        }
    }
}
