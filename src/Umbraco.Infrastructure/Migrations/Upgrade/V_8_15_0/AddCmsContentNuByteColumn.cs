using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_8_15_0;

public class AddCmsContentNuByteColumn : MigrationBase
{
    private const string TempTableName = Constants.DatabaseSchema.TableNamePrefix + "cms" + "ContentNuTEMP";

    public AddCmsContentNuByteColumn(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        AlterColumn<ContentNuDto>(Constants.DatabaseSchema.Tables.NodeData, "data");

        var columns = SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        AddColumnIfNotExists<ContentNuDto>(columns, "dataRaw");
    }

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
        public string? Data { get; set; }

        [Column("rv")]
        public long Rv { get; set; }
    }
}
