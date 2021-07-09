using NPoco;
using System.Data;
using System.Linq;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.Dtos;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Core.Migrations.Upgrade.V_8_15_0
{
    public class AddCmsContentNuByteColumn : MigrationBase
    {
        public AddCmsContentNuByteColumn(IMigrationContext context)
            : base(context)
        {

        }

        public override void Migrate()
        {
            // allow null for the `data` field
            if (DatabaseType.IsSqlCe())
            {
                // SQLCE does not support altering NTEXT, so we have to jump through some hoops to do it
                // All column ordering must remain the same as what is defined in the DTO so we need to create a temp table,
                // drop orig and then re-create/copy.
                Create.Table<ContentNuDtoTemp>(withoutKeysAndIndexes: true).Do();
                Execute.Sql($"INSERT INTO [{TempTableName}] SELECT nodeId, published, data, rv FROM [{Constants.DatabaseSchema.Tables.NodeData}]").Do();
                Delete.Table(Constants.DatabaseSchema.Tables.NodeData).Do();
                Create.Table<ContentNuDto>().Do();
                Execute.Sql($"INSERT INTO [{Constants.DatabaseSchema.Tables.NodeData}] SELECT nodeId, published, data, rv, NULL FROM [{TempTableName}]").Do();
            }
            else
            {
                AlterColumn<ContentNuDto>(Constants.DatabaseSchema.Tables.NodeData, "data");
            }

            var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
            AddColumnIfNotExists<ContentNuDto>(columns, "dataRaw");
        }

        private const string TempTableName = Constants.DatabaseSchema.TableNamePrefix + "cms" + "ContentNuTEMP";

        [TableName(TempTableName)]
        [ExplicitColumns]
        private class ContentNuDtoTemp
        {
            [Column("nodeId")]
            public int NodeId { get; set; }

            [Column("published")]
            public bool Published { get; set; }

            [Column("data")]
            [SpecialDbType(SpecialDbTypes.NTEXT)]
            [NullSetting(NullSetting = NullSettings.Null)]
            public string Data { get; set; }

            [Column("rv")]
            public long Rv { get; set; }
        }
    }
}
