using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the textarea value editor.
    /// </summary>
    public class TextAreaConfigurationEditor : ConfigurationEditor<TextAreaConfiguration>
    {
        public TextAreaConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
