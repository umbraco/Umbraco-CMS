using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.DecimalAlias, "Decimal", "decimal", "decimal", IsParameterEditor = true)]
    public class DecimalPropertyEditor : PropertyEditor
    {
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
        
        protected override PreValueEditor CreatePreValueEditor()
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
                Fields.Add(new PreValueField(new DecimalValidator())
                {
                    Description = "Enter the minimum amount of number to be entered",
                    Key = "min",
                    View = "decimal",
                    Name = "Minimum"
                });
                Fields.Add(new PreValueField(new DecimalValidator())
                {
                    Description = "Enter the intervals amount between each step of number to be entered",
                    Key = "step",
                    View = "decimal",
                    Name = "Step Size"
                });
                Fields.Add(new PreValueField(new DecimalValidator())
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