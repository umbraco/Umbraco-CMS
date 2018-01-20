using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.Decimal, "Decimal", PropertyEditorValueTypes.Decimal, "decimal", IsParameterEditor = true)]
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
        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            editor.Validators.Add(new DecimalValidator());
            return editor;
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new DecimalPreValueEditor();
        }

        /// <summary>
        /// A custom pre-value editor class to deal with the legacy way that the pre-value data is stored.
        /// </summary>
        internal class DecimalPreValueEditor : PreValueEditor
        {
            public DecimalPreValueEditor()
            {
                //create the fields
                Fields.Add(new DataTypeConfigurationField(new DecimalValidator())
                {
                    Description = "Enter the minimum amount of number to be entered",
                    Key = "min",
                    View = "decimal",
                    Name = "Minimum"
                });
                Fields.Add(new DataTypeConfigurationField(new DecimalValidator())
                {
                    Description = "Enter the intervals amount between each step of number to be entered",
                    Key = "step",
                    View = "decimal",
                    Name = "Step Size"
                });
                Fields.Add(new DataTypeConfigurationField(new DecimalValidator())
                {
                    Description = "Enter the maximum amount of number to be entered",
                    Key = "max",
                    View = "decimal",
                    Name = "Maximum"
                });
            }
        }


    }
}
