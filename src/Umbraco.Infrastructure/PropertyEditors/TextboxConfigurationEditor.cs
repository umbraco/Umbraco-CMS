using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the textbox value editor.
    /// </summary>
    public class TextboxConfigurationEditor : ConfigurationEditor<TextboxConfiguration>
    {
        public TextboxConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
