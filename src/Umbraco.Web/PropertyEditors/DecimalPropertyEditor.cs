using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.Decimal, "Decimal", "decimal", ValueTypes.Decimal, IsMacroParameterEditor = true)]
    public class DecimalPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public DecimalPropertyEditor(ILogger logger) : base(logger)
        {
        }

        /// <summary>
        /// Overridden to ensure that the value is validated
        /// </summary>
        /// <returns></returns>
        protected override ValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DecimalValidator());
            return editor;
        }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new DecimalConfigurationEditor();
        }
    }
}
