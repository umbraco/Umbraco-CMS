﻿using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id", AutoIncrement = false)]
    [ExplicitColumns]
    internal class MediaVersionDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.MediaVersion;

        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = false)]
        [ForeignKey(typeof(ContentVersionDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName, ForColumns = "id, path")]
        public int Id { get; set; }

        [Column("path")]
        [NullSetting(NullSetting = NullSettings.Null)]
        public string Path { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.OneToOne)]
        public ContentVersionDto ContentVersionDto { get; set; }
    }
}
