// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for configuration-less date/time properties.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.PlainDateTime,
    ValueEditorIsReusable = true,
    ValueType = ValueTypes.DateTime)]
public class PlainDateTimePropertyEditor : DataEditor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="PlainIntegerPropertyEditor" /> class.
    /// </summary>
    public PlainDateTimePropertyEditor(IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => SupportsReadOnly = true;
}
