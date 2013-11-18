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
        [PrimaryKeyColumn(IdentitySeed = 30)]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered)]
        public int DataTypeId { get; set; }

        [Column("propertyEditorAlias")]
        public string PropertyEditorAlias { get; set; }

        [Column("dbType")]
        [Length(50)]
        public string DbType { get; set; }//NOTE Is set to [varchar] (50) in Sql Server script

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}