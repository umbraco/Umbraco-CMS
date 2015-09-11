using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoRelation")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("parentId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode")]
        public int ParentId { get; set; }

        [Column("childId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode1")]
        public int ChildId { get; set; }

        [Column("relType")]
        [ForeignKey(typeof(RelationTypeDto))]
        public int RelationType { get; set; }

        [Column("datetime")]
        [Constraint(Default = SystemMethods.CurrentDateTime)]
        public DateTime Datetime { get; set; }

        [Column("comment")]
        [Length(1000)]
        public string Comment { get; set; }
    }
}