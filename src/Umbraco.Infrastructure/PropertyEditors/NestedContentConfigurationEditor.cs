using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the nested content value editor.
    /// </summary>
    public class NestedContentConfigurationEditor : ConfigurationEditor<NestedContentConfiguration>
    {
        public NestedContentConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
