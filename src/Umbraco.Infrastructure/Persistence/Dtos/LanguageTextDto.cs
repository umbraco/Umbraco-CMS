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
    public const string PrimaryKeyName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;
    public const string LanguageIdName = "languageId";
    public const string UniqueIdName = Constants.DatabaseSchema.Columns.UniqueIdName;

    [Column(PrimaryKeyName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(LanguageIdName)]
    [ForeignKey(typeof(LanguageDto), Column = LanguageDto.PrimaryKeyName)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_languageId", ForColumns = $"{LanguageIdName},{UniqueIdName}")]
    public int LanguageId { get; set; }

    [Column(UniqueIdName)]
    [ForeignKey(typeof(DictionaryDto), Column = DictionaryDto.UniqueIdName)]
    public Guid UniqueId { get; set; }

    [Column("value")]
    [Length(1000)]
    public string Value { get; set; } = null!;
}
