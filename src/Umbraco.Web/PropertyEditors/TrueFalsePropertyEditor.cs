using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalse, "True/False", "boolean")]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
    }
}