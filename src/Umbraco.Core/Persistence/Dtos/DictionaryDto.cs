using System;
using System.Collections.Generic;
using NPoco;
using Umbraco.Core.Persistence.DatabaseAnnotations;

namespace Umbraco.Core.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    internal class DictionaryDto
    {
        public const string TableName = Constants.DatabaseSchema.Tables.DictionaryEntry;

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
        [Index(IndexTypes.NonClustered, Name = "IX_cmsDictionary_key")]
        public string Key { get; set; }

        [ResultColumn]
        [Reference(ReferenceType.Many, ColumnName = "UniqueId", ReferenceMemberName = "UniqueId")]
        public List<LanguageTextDto> LanguageTextDtos { get; set; }
    }
}
