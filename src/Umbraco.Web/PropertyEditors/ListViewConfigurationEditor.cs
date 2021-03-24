using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the listview value editor.
    /// </summary>
    public class ListViewConfigurationEditor : ConfigurationEditor<ListViewConfiguration>
    {
        public ListViewConfigurationEditor()
        {
            Field(nameof(ListViewConfiguration.TreeSource))
                .Config = new Dictionary<string, object>
                {
                    { "showXpath", false },
                    { "allowSelectNode", false }
                };
        }
    }
}
