namespace Umbraco.Cms.Core.Configuration.Grid;

public interface IGridEditorsConfig
{
    IEnumerable<IGridEditorConfig> Editors { get; }
}
