﻿using System;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    internal class ContentVersionCultureVariationDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.ContentVersionCultureVariation;
        private int? _publishedUserId;

        [Column("id")]
        [PrimaryKeyColumn]
        public int Id { get; set; }

        [Column("versionId")]
        [ForeignKey(typeof(ContentVersionDto))]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_VersionId", ForColumns = "versionId,languageId")]
        public int VersionId { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto))]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
        public int LanguageId { get; set; }

        // this is convenient to carry the culture around, but has no db counterpart
        [Ignore]
        public string Culture { get; set; }

        [Column("name")]
        public string Name { get; set; }

        [Column("date")]
        public DateTime Date { get; set; }

        // fixme want?
        [Column("availableUserId")]
        [ForeignKey(typeof(UserDto))]
        [NullSetting(NullSetting = NullSettings.Null)]
        public int? PublishedUserId { get => _publishedUserId == 0 ? null : _publishedUserId; set => _publishedUserId = value; } //return null if zero

        [Column("edited")]
        public bool Edited { get; set; }
    }
}
