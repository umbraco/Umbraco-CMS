using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.AccessControl;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoAccess")]
    [PrimaryKey("id", autoIncrement = false)]
    [ExplicitColumns]
    internal class AccessDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoAccess", AutoIncrement = false)]
        public Guid Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_uniqueID", Column = "uniqueID")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoAccess_nodeId")]
        public Guid NodeId { get; set; }

        [Column("loginNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_uniqueID1", Column = "uniqueID")]
        public Guid LoginNodeId { get; set; }

        [Column("noAccessNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_uniqueID2", Column = "uniqueID")]
        public Guid AccessDeniedNodeId { get; set; }

        [Column("createDate")]
        [Constraint(Default = "getdate()")]
        public DateTime CreateDate { get; set; }

        [Column("updateDate")]
        [Constraint(Default = "getdate()")]
        public DateTime UpdateDate { get; set; }

        [ResultColumn]
        public List<AccessRuleDto> Rules { get; set; }
    }
}