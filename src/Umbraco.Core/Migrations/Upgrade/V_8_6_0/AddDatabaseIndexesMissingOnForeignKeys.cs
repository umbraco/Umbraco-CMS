using NPoco;
using System;
using System.Linq;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;

namespace Umbraco.Core.Migrations.Upgrade.V_8_6_0
{
    public class AddDatabaseIndexesMissingOnForeignKeys : MigrationBase
    {
        public AddDatabaseIndexesMissingOnForeignKeys(IMigrationContext context)
            : base(context)
        {

        }

        public override void Migrate()
        {
            var newIndexes = new[]
            {
                (typeof(DictionaryDto), nameof(DictionaryDto.Parent))
            };

            foreach (var (type, propertyName) in newIndexes)
            {
                CreateIndexIfNotExists(type, propertyName);
            }
        }

        private void CreateIndexIfNotExists(Type dto, string propertyName)
        {
            var property = dto.GetProperty(propertyName);
            var indexName = property.GetCustomAttributes(false).OfType<IndexAttribute>().Single().Name;

            if (IndexExists(indexName))
            {
                return;
            }

            var tableName = dto.GetCustomAttributes<TableNameAttribute>(false).Single().Value;
            var columnName = property.GetCustomAttributes(false).OfType<ColumnAttribute>().Single().Name;


            Create
                .Index(indexName)
                .OnTable(tableName)
                .OnColumn(columnName)
                .Ascending()
                .WithOptions().NonClustered() // All newly defined indexes are non-clustered
                .Do();
        }
    }
}
