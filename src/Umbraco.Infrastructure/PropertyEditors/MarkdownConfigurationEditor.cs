using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editorfor the markdown value editor.
    /// </summary>
    internal class MarkdownConfigurationEditor : ConfigurationEditor<MarkdownConfiguration>
    {
        public MarkdownConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
