using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.IntegerAlias, "Numeric", "integer", IsParameterEditor = true)]
    public class IntegerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public IntegerPropertyEditor(ILogger logger) : base(logger)
        {
        }

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new IntegerValidator());
            return editor;
        }

    }
}