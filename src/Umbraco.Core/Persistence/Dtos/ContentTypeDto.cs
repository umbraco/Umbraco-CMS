﻿using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class ContentTypeDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.ContentType;

        [Column("pk")]
        [PrimaryKeyColumn(IdentitySeed = 535)]
        public int PrimaryKey { get; set; }

        [Column("nodeId")]
        [ForeignKey(typeof(NodeDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsContentType")]
        public int NodeId { get; set; }

        [Column("alias")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Alias { get; set; }

        [Column("icon")]
        [Index(IndexTypes.NonClustered)]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Icon { get; set; }

        [Column("thumbnail")]
        [Constraint(Default = "folder.png")]
        public string Thumbnail { get; set; }

        [Column("description")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [Length(1500)]
        public string Description { get; set; }

        [Column("isContainer")]
        [Constraint(Default = "0")]
        public bool IsContainer { get; set; }

        [Column("isElement")]
        [Constraint(Default = "0")]
        public bool IsElement { get; set; }

        [Column("allowAtRoot")]
        [Constraint(Default = "0")]
        public bool AllowAtRoot { get; set; }

        [Column("variations")]
        [Constraint(Default = "1" /*ContentVariation.InvariantNeutral*/)]
        public byte Variations { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne, ColumnName = "NodeId")]
        public NodeDto NodeDto { get; set; }
    }
}
