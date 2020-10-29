using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.IO;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the rich text value editor.
    /// </summary>
    [UmbracoVolatile]
    public class RichTextConfigurationEditor : ConfigurationEditor<RichTextConfiguration>
    {
        public RichTextConfigurationEditor(IIOHelper ioHelper) : base(ioHelper)
        {
        }
    }
}
