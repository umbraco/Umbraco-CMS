using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.Macro)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class MacroDto
{
    [Column("id")]
    [PrimaryKeyColumn]
    public int Id { get; set; }

    [Column("uniqueId")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacro_UniqueId")]
    public Guid UniqueId { get; set; }

    [Column("macroUseInEditor")]
    [Constraint(Default = "0")]
    public bool UseInEditor { get; set; }

    [Column("macroRefreshRate")]
    [Constraint(Default = "0")]
    public int RefreshRate { get; set; }

    [Column("macroAlias")]
    [Index(IndexTypes.UniqueNonClustered, Name = "IX_cmsMacroPropertyAlias")]
    public string Alias { get; set; } = string.Empty;

    [Column("macroName")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    [Column("macroCacheByPage")]
    [Constraint(Default = "1")]
    public bool CacheByPage { get; set; }

    [Column("macroCachePersonalized")]
    [Constraint(Default = "0")]
    public bool CachePersonalized { get; set; }

    [Column("macroDontRender")]
    [Constraint(Default = "0")]
    public bool DontRender { get; set; }

    [Column("macroSource")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string MacroSource { get; set; } = null!;

    [Column("macroType")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public int MacroType { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = "Macro")]
    public List<MacroPropertyDto>? MacroPropertyDtos { get; set; }
}
