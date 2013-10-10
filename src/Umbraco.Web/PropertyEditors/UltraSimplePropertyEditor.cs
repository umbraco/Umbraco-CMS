using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.UltraSimpleEditorAlias, "Ultrasimple editor", "ultrasimple")]
    public class UltraSimplePropertyEditor : PropertyEditor
    {
    }
}