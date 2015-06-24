using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", "INT", "boolean", IsParameterEditor = true, Group = "Common", Icon="icon-checkbox")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
    }
}