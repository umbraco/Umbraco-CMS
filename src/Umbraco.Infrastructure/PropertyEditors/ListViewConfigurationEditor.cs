using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the listview value editor.
    /// </summary>
    public class ListViewConfigurationEditor : ConfigurationEditor<ListViewConfiguration>
    {
        public ListViewConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
