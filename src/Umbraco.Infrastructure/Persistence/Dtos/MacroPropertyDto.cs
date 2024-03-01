using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.MacroProperty)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class MacroPropertyDto
{
    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    // important to use column name != cmsMacro.uniqueId (fix in v8)
    [Column("uniquePropertyId")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacroProperty_UniquePropertyId")]
    public Guid UniqueId { get; set; }

    [Column("editorAlias")]
    public string EditorAlias { get; set; } = null!;

    [Column("macro")]
    [ForeignKey(typeof(MacroDto))]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacroProperty_Alias", ForColumns = "macro, macroPropertyAlias")]
    public int Macro { get; set; }

    [Column("macroPropertySortOrder")]
    [Constraint(Default = "0")]
    public byte SortOrder { get; set; }

    [Column("macroPropertyAlias")]
    [Length(50)]
    public string Alias { get; set; } = null!;

    [Column("macroPropertyName")]
    public string? Name { get; set; }
}
