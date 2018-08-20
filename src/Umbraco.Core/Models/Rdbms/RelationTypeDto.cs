using System;
using Umbraco.Core.Persistence;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Models.Rdbms
{
    [TableName("umbracoRelationType")]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationTypeDto
    {
        public const int NodeIdSeed = 4;

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
        public int Id { get; set; }

        [Column("typeUniqueId")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_UniqueId")]
        public Guid UniqueId { get; set; }

        [Column("dual")]
        public bool Dual { get; set; }

        [Column("parentObjectType")]
        public Guid ParentObjectType { get; set; }

        [Column("childObjectType")]
        public Guid ChildObjectType { get; set; }

        [Column("name")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_name")]
        public string Name { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(100)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_alias")]
        public string Alias { get; set; }
    }
}