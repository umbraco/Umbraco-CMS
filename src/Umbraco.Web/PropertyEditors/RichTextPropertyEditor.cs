using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TinyMCEv3, "Rich Text Editor", "rte")]
    public class RichTextPropertyEditor : PropertyEditor
    {
    }
}