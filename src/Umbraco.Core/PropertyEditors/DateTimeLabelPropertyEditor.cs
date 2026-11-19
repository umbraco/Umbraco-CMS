// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for label properties holding a date and time.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.LabelDateTime,
    ValueType = ValueTypes.DateTime,
    ValueEditorIsReusable = true)]
public class DateTimeLabelPropertyEditor : LabelPropertyEditorBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DateTimeLabelPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public DateTimeLabelPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory, ioHelper)
    {
    }
}
