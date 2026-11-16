// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for label properties holding a string.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Label,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class LabelPropertyEditor : LabelPropertyEditorBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LabelPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public LabelPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory, ioHelper)
    {
    }
}
