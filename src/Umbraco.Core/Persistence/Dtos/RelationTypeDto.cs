﻿using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(Constants.DatabaseSchema.Tables.RelationType)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class RelationTypeDto
    {
        public const int NodeIdSeed = 10;

        [Column("id")]
        [PrimaryKeyColumn(IdentitySeed = NodeIdSeed)]
        public int Id { get; set; }

        [Column("typeUniqueId")]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_UniqueId")]
        public Guid UniqueId { get; set; }

        [Column("dual")]
        public bool Dual { get; set; }

        [Column("parentObjectType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? ParentObjectType { get; set; }

        [Column("childObjectType")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public Guid? ChildObjectType { get; set; }

        [Column("name")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_name")]
        public string Name { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        [Length(100)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_umbracoRelationType_alias")]
        public string Alias { get; set; }
    }
}
