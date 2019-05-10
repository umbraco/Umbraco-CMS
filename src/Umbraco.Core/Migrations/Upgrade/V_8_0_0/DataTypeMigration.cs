using System.Linq;
using Newtonsoft.Json;
using Umbraco.Core.Composing;
using Umbraco.Core.Migrations.Upgrade.V_8_0_0.DataTypes;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.Persistence.Querying;

namespace Umbraco.Core.Migrations.Upgrade.V_8_0_0
{

    public class DataTypeMigration : MigrationBase
    {
        private readonly PreValueMigratorCollection _preValueMigrators;

        public DataTypeMigration(IMigrationContext context, PreValueMigratorCollection preValueMigrators)
            : base(context)
        {
            _preValueMigrators = preValueMigrators;
        }

        public override void Migrate()
        {
            // drop and create columns
            Delete.Column("pk").FromTable("cmsDataType").Do();

            // rename the table
            Rename.Table("cmsDataType").To(Constants.DatabaseSchema.Tables.DataType).Do();

            // create column
            AddColumn<DataTypeDto>(Constants.DatabaseSchema.Tables.DataType, "config");
            Execute.Sql(Sql().Update<DataTypeDto>(u => u.Set(x => x.Configuration, string.Empty))).Do();

            // renames
            Execute.Sql(Sql()
                .Update<DataTypeDto>(u => u.Set(x => x.EditorAlias, "Umbraco.ColorPicker"))
                .Where<DataTypeDto>(x => x.EditorAlias == "Umbraco.ColorPickerAlias")).Do();

            // from preValues to configuration...
            var sql = Sql()
                .Select<DataTypeDto>()
                .AndSelect<PreValueDto>(x => x.Id, x => x.Alias, x => x.SortOrder, x => x.Value)
                .From<DataTypeDto>()
                .InnerJoin<PreValueDto>().On<DataTypeDto, PreValueDto>((left, right) => left.NodeId == right.NodeId)
                .OrderBy<DataTypeDto>(x => x.NodeId)
                .AndBy<PreValueDto>(x => x.SortOrder);

            var dtos = Database.Fetch<PreValueDto>(sql).GroupBy(x => x.NodeId);

            foreach (var group in dtos)
            {
                var dataType = Database.Fetch<DataTypeDto>(Sql()
                    .Select<DataTypeDto>()
                    .From<DataTypeDto>()
                    .Where<DataTypeDto>(x => x.NodeId == group.Key)).First();

                var migrator = _preValueMigrators.GetMigrator(dataType.EditorAlias) ?? new DefaultPreValueMigrator();
                var config = migrator.GetConfiguration(dataType.NodeId, dataType.EditorAlias, group.ToDictionary(x => x.Alias, x => x));
                dataType.Configuration = JsonConvert.SerializeObject(config);

                Database.Update(dataType);
            }
        }
    }
}
