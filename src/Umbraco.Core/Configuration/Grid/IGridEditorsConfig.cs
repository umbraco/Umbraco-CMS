using System.Collections.Generic;

namespace Umbraco.Core.Configuration.Grid
{
    public interface IGridEditorsConfig
    {
        IEnumerable<IGridEditorConfig> Editors { get; }
    }
}