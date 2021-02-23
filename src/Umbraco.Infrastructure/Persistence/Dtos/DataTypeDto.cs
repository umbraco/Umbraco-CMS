using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    [TableName(Cms.Core.Constants.DatabaseSchema.Tables.DataType)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class DataTypeDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("propertyEditorAlias")]
        public string EditorAlias { get; set; } // TODO: should this have a length

        [Column("dbType")]
        [Length(50)]
        public string DbType { get; set; }

        [Column("config")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Configuration { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
        public NodeDto NodeDto { get; set; }
    }
}
