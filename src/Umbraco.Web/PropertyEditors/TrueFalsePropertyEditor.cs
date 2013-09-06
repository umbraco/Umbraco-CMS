using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", "boolean")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
    }
}