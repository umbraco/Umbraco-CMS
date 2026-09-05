// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for label properties holding a time.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.LabelTime,
    ValueType = ValueTypes.Time,
    ValueEditorIsReusable = true)]
public class TimeLabelPropertyEditor : LabelPropertyEditorBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TimeLabelPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public TimeLabelPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory, ioHelper)
    {
    }
}
