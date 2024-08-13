// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors.Validators;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a date and time property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.DateTime,
    ValueType = ValueTypes.DateTime,
    ValueEditorIsReusable = true)]
public class DateTimePropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DateTimePropertyEditor" /> class.
    /// </summary>
    public DateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor()
    {
        IDataValueEditor editor = base.CreateValueEditor();
        editor.Validators.Add(new DateTimeValidator());
        return editor;
    }
}
