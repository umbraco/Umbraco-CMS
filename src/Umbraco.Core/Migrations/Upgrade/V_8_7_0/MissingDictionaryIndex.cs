using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class MissingDictionaryIndex : MigrationBase
    {
        public MissingDictionaryIndex(IMigrationContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Adds an index to the foreign key column <c>parent</c> on <c>DictionaryDto</c>'s table
        /// if it doesn't already exist
        /// </summary>
        public override void Migrate()
        {          
            var indexName = "IX_" + DictionaryDto.TableName + "_Parent";

            if (!IndexExists(indexName))
            {
                Create
                     .Index(indexName)
                     .OnTable(DictionaryDto.TableName)
                     .OnColumn("parent")
                     .Ascending()
                     .WithOptions().NonClustered()
                     .Do();
            }
        }
    }
}
