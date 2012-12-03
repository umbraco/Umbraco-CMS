using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

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
        public int ParentId { get; set; }

        [Column("childId")]
        public int ChildId { get; set; }

        [Column("relType")]
        public int RelationType { get; set; }

        [Column("datetime")]
        [Constraint(Default = "getdate()")]
        public DateTime Datetime { get; set; }

        [Column("comment")]
        [Length(1000)]
        public string Comment { get; set; }
    }
}