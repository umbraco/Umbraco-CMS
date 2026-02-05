using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName)]
[ExplicitColumns]
internal sealed class DocumentCultureVariationDto : ICultureVariationDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.DocumentCultureVariation;

    private const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column(ICultureVariationDto.Columns.NodeId)]
    [ForeignKey(typeof(NodeDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_" + TableName + "_NodeId", ForColumns = $"{INodeDto.Columns.NodeId},{ICultureVariationDto.Columns.LanguageId}")]
    public int NodeId { get; set; }

    [Column(ICultureVariationDto.Columns.LanguageId)]
    [ForeignKey(typeof(LanguageDto))]
    [Index(IndexTypes.NonClustered, Name = "IX_" + TableName + "_LanguageId")]
    public int LanguageId { get; set; }

    // this is convenient to carry the culture around, but has no db counterpart
    [Ignore]
    public string? Culture { get; set; }

    // authority on whether a culture has been edited
    [Column(ICultureVariationDto.Columns.Edited)]
    public bool Edited { get; set; }

    // de-normalized for perfs
    // (means there is a current content version culture variation for the language)
    [Column(ICultureVariationDto.Columns.Available)]
    public bool Available { get; set; }

    // de-normalized for perfs
    // (means there is a published content version culture variation for the language)
    [Column(ICultureVariationDto.Columns.Published)]
    public bool Published { get; set; }

    // de-normalized for perfs
    // (when available, copies name from current content version culture variation for the language)
    // (otherwise, it's the published one, 'cos we need to have one)
    [Column(ICultureVariationDto.Columns.Name)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }
}
