using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [ValueEditor(Constants.PropertyEditors.Aliases.EmailAddress, "Email address", "email", Icon="icon-message")]
    public class EmailAddressPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public EmailAddressPropertyEditor(ILogger logger) : base(logger)
        {
        }

        protected override IPropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            //add an email address validator
            editor.Validators.Add(new EmailValidator());
            return editor;
        }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new EmailAddressConfigurationEditor();
        }
    }
}
