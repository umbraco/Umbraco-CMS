using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.TrueFalseAlias, "True/False", "INT", "boolean", IsParameterEditor = true)]
    public class TrueFalsePropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public TrueFalsePropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}