using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDataType")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class DataTypeDto
    {
        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered)]
        public int DataTypeId { get; set; }

        [Column("controlId")]
        public Guid ControlId { get; set; }

        [Column("dbType")]
        [DatabaseType(DatabaseTypes.NVARCHAR, Length = 50)]//NOTE Is set to [varchar] (50) in Sql Server script
        public string DbType { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}