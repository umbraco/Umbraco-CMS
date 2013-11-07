using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.EmailAddressAlias, "Email address", "email")]
    public class EmailAddressPropertyEditor : PropertyEditor
    {
        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            //add an email address validator
            editor.Validators.Add(new EmailValidator());
            return editor;
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new EmailAddressePreValueEditor();
        }

        internal class EmailAddressePreValueEditor : PreValueEditor
        {
            [PreValueField("Required?", "boolean")]
            public bool IsRequired { get; set; }
        }

    }
}