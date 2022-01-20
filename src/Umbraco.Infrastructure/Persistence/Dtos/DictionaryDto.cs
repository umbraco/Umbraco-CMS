using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    public class DictionaryDto  // public as required to be accessible from Deploy for the RepairDictionaryIdsWorkItem.
    {
        public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.DictionaryEntry;

        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("id")]
        [Index(IndexTypes.UniqueNonClustered)]
        public Guid UniqueId { get; set; }

        [Column("parent")]
        [NullSetting(NullSetting = NullSettings.Null)]
        [ForeignKey(typeof(DictionaryDto), Column = "id")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_Parent")]
        public Guid? Parent { get; set; }

        [Column("key")]
        [Length(450)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_key")]
        public string Key { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ColumnName = "UniqueId", ReferenceMemberName = "UniqueId")]
        public List<LanguageTextDto> LanguageTextDtos { get; set; }
    }
}
