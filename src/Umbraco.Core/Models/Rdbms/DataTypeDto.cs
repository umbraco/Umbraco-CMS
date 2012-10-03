using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("cmsDataType")]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class DataTypeDto
    {
        [Column("pk")]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        public int DataTypeId { get; set; }

        [Column("controlId")]
        public Guid ControlId { get; set; }

        [Column("dbType")]
        public string DbType { get; set; }

        [ResultColumn]
        public NodeDto NodeDto { get; set; }
    }
}