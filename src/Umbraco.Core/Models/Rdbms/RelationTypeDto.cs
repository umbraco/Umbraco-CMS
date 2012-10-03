using System;
using Umbraco.Core.Persistence;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoRelationType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationTypeDto
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("dual")]
        public bool Dual { get; set; }

        [Column("parentObjectType")]
        public Guid ParentObjectType { get; set; }

        [Column("childObjectType")]
        public Guid ChildObjectType { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("alias")]
        public string Alias { get; set; }
    }
}