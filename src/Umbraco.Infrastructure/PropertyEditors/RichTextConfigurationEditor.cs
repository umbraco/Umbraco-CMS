using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the rich text value editor.
    /// </summary>
    public class RichTextConfigurationEditor : ConfigurationEditor<RichTextConfiguration>
    {
        public RichTextConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
