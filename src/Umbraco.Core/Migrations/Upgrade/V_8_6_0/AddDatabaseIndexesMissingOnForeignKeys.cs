using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class AddDatabaseIndexesMissingOnForeignKeys : MigrationBase
    {
        public AddDatabaseIndexesMissingOnForeignKeys(IMigrationContext context)
            : base(context)
        {

        }

        /// <summary>
        /// Adds an index to the foreign key column <c>parent</c> on <c>DictionaryDto</c>'s table
        /// if it doesn't already exist
        /// </summary>
        public override void Migrate()
        {
            var tableInfo = Context.Database.PocoDataFactory.ForType(typeof(DictionaryDto)).TableInfo;
            tableInfo.TableName = tableInfo.TableName;
            var indexName = "IX_" + tableInfo.TableName + "_Parent";

            if (IndexExists(indexName) == false)
            {
                Create
                     .Index(indexName)
                     .OnTable(tableInfo.TableName)
                     .OnColumn("parent")
                     .Ascending()
                     .WithOptions().NonClustered()
                     .Do();
            }
        }
    }
}
