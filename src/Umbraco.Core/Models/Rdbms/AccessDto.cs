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
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoAccess_nodeId")]
        public int NodeId { get; set; }

        [Column("loginNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id1")]
        public int LoginNodeId { get; set; }

        [Column("noAccessNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id2")]
        public int NoAccessNodeId { get; set; }

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