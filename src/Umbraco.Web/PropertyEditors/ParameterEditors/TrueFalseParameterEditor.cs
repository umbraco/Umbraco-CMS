using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors.ParameterEditors
{
    [ParameterEditor("bool", "True/False", "boolean")]
    public class TrueFalseParameterEditor : ParameterEditor
    {
    }
}