using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id", AutoIncrement = true)]
[ExplicitColumns]
internal class DocumentUrlAliasDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentUrlAlias;

    [Column("id")]
    [PrimaryKeyColumn(Clustered = false, AutoIncrement = true)]
    public int Id { get; set; }

    // Unique index on (uniqueId, languageId, alias) - prevents duplicate entries
    [Index(IndexTypes.UniqueNonClustered, ForColumns = "uniqueId, languageId, alias", Name = "IX_" + TableName + "_Unique")]
    [Column("uniqueId")]
    [ForeignKey(typeof(NodeDto), Column = "uniqueId")]
    public Guid UniqueId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    public int LanguageId { get; set; }

    // Lookup index on (alias, languageId) for fast retrieval
    [Index(IndexTypes.NonClustered, ForColumns = "alias, languageId", Name = "IX_" + TableName + "_Lookup")]
    [Column("alias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Alias { get; set; } = string.Empty;
}
