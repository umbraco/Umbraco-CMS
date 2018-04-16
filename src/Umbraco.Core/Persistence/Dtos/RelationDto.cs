using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;
using Umbraco.Core.Persistence.DatabaseModelDefinitions;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.Relation)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationDto
    {
        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("parentId")]
        [ForeignKey(typeof(NodeDto), Name = "FK_umbracoRelation_umbracoNode")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelation_parentChildType", ForColumns = "parentId,childId,relType")]
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
