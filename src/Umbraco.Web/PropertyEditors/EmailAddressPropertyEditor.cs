using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.PropertyEditors.Validators;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.EmailAddress, "Email address", "email", Icon="icon-message")]
    public class EmailAddressPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public EmailAddressPropertyEditor(ILogger logger) : base(logger)
        {
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            var editor = base.CreateValueEditor();
            //add an email address validator
            editor.Validators.Add(new EmailValidator());
            return editor;
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new EmailAddressePreValueEditor();
        }

        internal class EmailAddressePreValueEditor : PreValueEditor
        {
            //TODO: This doesn't seem necessary since it can be specified at the property type level - this will however be useful if/when
            // we support overridden property value pre-value options.
            [DataTypeConfigurationField("Required?", "boolean")]
            public bool IsRequired { get; set; }
        }

    }
}
