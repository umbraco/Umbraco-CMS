using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
public class LanguageTextDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DictionaryValue;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNamePk;

    private const string LanguageIdName = "languageId";
    private const string UniqueIdName = "UniqueId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(LanguageIdName)]
    [ForeignKey(typeof(LanguageDto), Column = LanguageDto.PrimaryKeyColumnName)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_languageId", ForColumns = $"{LanguageIdName},{UniqueIdName}")]
    public int LanguageId { get; set; }

    [Column(UniqueIdName)]
    [ForeignKey(typeof(DictionaryDto), Column = DictionaryDto.UniqueIdColumnName)]
    public Guid UniqueId { get; set; }

    [Column("value")]
    [Length(1000)]
    public string Value { get; set; } = null!;
}
