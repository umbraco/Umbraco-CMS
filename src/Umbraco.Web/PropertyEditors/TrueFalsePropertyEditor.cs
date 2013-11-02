using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", "boolean", IsParameterEditor = true)]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
    }
}