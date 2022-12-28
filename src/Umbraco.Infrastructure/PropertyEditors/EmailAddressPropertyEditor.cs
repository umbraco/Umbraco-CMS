// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.EmailAddress,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Email address",
    "email",
    Icon = "icon-message",
    ValueEditorIsReusable = true)]
public class EmailAddressPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public EmailAddressPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();

        // add an email address validator
        editor.Validators.Add(new EmailValidator());
        return editor;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new EmailAddressConfigurationEditor(_ioHelper);
}
