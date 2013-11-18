using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", "INT", "boolean", IsParameterEditor = true)]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
    }
}