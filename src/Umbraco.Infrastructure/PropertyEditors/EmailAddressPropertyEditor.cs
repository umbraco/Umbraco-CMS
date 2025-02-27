// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

[DataEditor(
    Constants.PropertyEditors.Aliases.EmailAddress,
    ValueEditorIsReusable = true)]
public class EmailAddressPropertyEditor : DataEditor
{
    /// <summary>
    ///     The constructor will setup the property editor based on the attribute if one is found
    /// </summary>
    public EmailAddressPropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();

        // add an email address validator
        editor.Validators.Add(new EmailValidator());
        return editor;
    }
}
