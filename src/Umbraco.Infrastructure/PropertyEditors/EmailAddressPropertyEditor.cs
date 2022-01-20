// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.EmailAddress,
        EditorType.PropertyValue | EditorType.MacroParameter,
        "Email address",
        "email",
        Icon = "icon-message")]
    public class EmailAddressPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public EmailAddressPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper)
            : base(dataValueEditorFactory)
        {
            _ioHelper = ioHelper;
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
            return new EmailAddressConfigurationEditor(_ioHelper);
        }
    }
}
