using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.EmailAddress, EditorType.PropertyValue | EditorType.MacroParameter, "Email address", "email", Icon="icon-message")]
    public class EmailAddressPropertyEditor : DataEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public EmailAddressPropertyEditor(ILogger logger) : base(logger)
        {
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            //add an email address validator
            editor.Validators.Add(new EmailValidator());
            return editor;
        }

        protected override IConfigurationEditor CreateConfigurationEditor()
        {
            return new EmailAddressConfigurationEditor();
        }
    }
}
