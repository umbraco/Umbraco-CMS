using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyName)]
[ExplicitColumns]
public class LanguageTextDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryValue;
    public const string PrimaryKeyName = Constants.DatabaseSchema.PrimaryKeyNamePK;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto), Column = "id")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_languageId", ForColumns = "languageId,uniqueId")]
    public int LanguageId { get; set; }

    [Column("uniqueId")]
    [ForeignKey(typeof(DictionaryDto), Column = "id")]
    public Guid UniqueId { get; set; }

    [Column("value")]
    [Length(1000)]
    public string Value { get; set; } = null!;
}
