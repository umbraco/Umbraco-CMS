using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0
{
    public class ExternalLoginTableIndexes : MigrationBase
    {
        public ExternalLoginTableIndexes(IMigrationContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Adds new indexes to the External Login table
        /// </summary>
        public override void Migrate()
        {
            // TODO: Before adding these indexes we need to remove duplicate data

            var indexName1 = "IX_" + ExternalLoginDto.TableName + "_LoginProvider";

            if (!IndexExists(indexName1))
            {
                Create
                     .Index(indexName1)
                     .OnTable(ExternalLoginDto.TableName)
                     .OnColumn("loginProvider")
                     .Ascending()
                     .WithOptions()
                     .Unique()
                     .WithOptions()
                     .NonClustered()
                     .Do();
            }

            var indexName2 = "IX_" + ExternalLoginDto.TableName + "_ProviderKey";

            if (!IndexExists(indexName2))
            {
                Create
                     .Index(indexName2)
                     .OnTable(ExternalLoginDto.TableName)
                     .OnColumn("loginProvider").Ascending()
                     .OnColumn("providerKey").Ascending()
                     .WithOptions()
                     .NonClustered()
                     .Do();
            }
        }
    }
}
