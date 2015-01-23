using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoAccess")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class AccessDto
    {
        [Column("id")]
        [PrimaryKeyColumn(Name = "PK_umbracoAccess")]
        public int Id { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id")]
        public int NodeId { get; set; }

        [Column("loginNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id1")]
        public int LoginNodeId { get; set; }

        [Column("noAccessNodeId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoAccess_umbracoNode_id2")]
        public int AccessDeniedNodeId { get; set; }
    }
}