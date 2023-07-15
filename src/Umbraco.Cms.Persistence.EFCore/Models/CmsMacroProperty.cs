namespace Umbraco.Cms.Persistence.EFCore.Models;

public class CmsMacroProperty
{
    public int Id { get; set; }

    public Guid UniquePropertyId { get; set; }

    public string EditorAlias { get; set; } = null!;

    public int Macro { get; set; }

    public int MacroPropertySortOrder { get; set; }

    public string MacroPropertyAlias { get; set; } = null!;

    public string MacroPropertyName { get; set; } = null!;

    public virtual CmsMacro MacroNavigation { get; set; } = null!;
}
