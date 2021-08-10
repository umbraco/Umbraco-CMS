using System;
using NPoco;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos
{
    [TableName(TableName)]
    [PrimaryKey("pk")]
    [ExplicitColumns]
    public class LanguageTextDto
    {
        public const string TableName = Cms.Core.Constants.DatabaseSchema.Tables.DictionaryValue;

        [Column("pk")]
        [PrimaryKeyColumn]
        public int PrimaryKey { get; set; }

        [Column("languageId")]
        [ForeignKey(typeof(LanguageDto), Column = "id")]
        public int LanguageId { get; set; }

        [Column("UniqueId")]
        [ForeignKey(typeof(DictionaryDto), Column = "id")]
        public Guid UniqueId { get; set; }

        [Column("value")]
        [Length(1000)]
        [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_value", ForColumns = "languageId,UniqueId,value")]
        public string Value { get; set; }
    }
}
