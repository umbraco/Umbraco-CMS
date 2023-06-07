using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class DocumentCultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentCultureVariation;

    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("nodeId")]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = "nodeId,languageId")]
    public int NodeId { get; set; }

    [Column("languageId")]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    // this is convenient to carry the culture around, but has no db counterpart
    [Ignore]
    public string? Culture { get; set; }

    // authority on whether a culture has been edited
    [Column("edited")]
    public bool Edited { get; set; }

    // de-normalized for perfs
    // (means there is a current content version culture variation for the language)
    [Column("available")]
    public bool Available { get; set; }

    // de-normalized for perfs
    // (means there is a published content version culture variation for the language)
    [Column("published")]
    public bool Published { get; set; }

    // de-normalized for perfs
    // (when available, copies name from current content version culture variation for the language)
    // (otherwise, it's the published one, 'cos we need to have one)
    [Column("name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }
}
