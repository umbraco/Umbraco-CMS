using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.DataType)]
    [PrimaryKey("nodeId", AutoIncrement = false)]
    [ExplicitColumns]
    internal class DataTypeDto
    {
        [Column("nodeId")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(NodeDto))]
        public int NodeId { get; set; }

        [Column("propertyEditorAlias")]
        public string EditorAlias { get; set; } // fixme - length?!

        [Column("dbType")]
        [Length(50)]
        public string DbType { get; set; }//NOTE Is set to [varchar] (50) in Sql Server script

        [Column("config")]
        [SpecialDbType(SpecialDbTypes.NTEXT)]
        [Constraint(Default = "")]
        public string Configuration { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "DataTypeId")]
        public NodeDto NodeDto { get; set; }
    }
}
