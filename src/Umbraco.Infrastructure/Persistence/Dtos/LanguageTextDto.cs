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

    internal const string ReferenceMemberName = "UniqueId"; // for clarity in DictionaryDto reference

    private const string LanguageIdColumnName = "languageId";
    private const string UniqueIdColumnName = "UniqueId";

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int PrimaryKey { get; set; }

    [Column(LanguageIdColumnName)]
    [ForeignKey(typeof(LanguageDto), Column = LanguageDto.PrimaryKeyColumnName)]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_languageId", ForColumns = $"{LanguageIdColumnName},{UniqueIdColumnName}")]
    public int LanguageId { get; set; }

    [Column(UniqueIdColumnName)]
    [ForeignKey(typeof(DictionaryDto), Column = DictionaryDto.UniqueIdColumnName)]
    public Guid UniqueId { get; set; }

    [Column("value")]
    [Length(1000)]
    public string Value { get; set; } = null!;
}
