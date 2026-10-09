// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a property editor for label properties holding a decimal.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.LabelDecimal,
    ValueType = ValueTypes.Decimal,
    ValueEditorIsReusable = true)]
public class DecimalLabelPropertyEditor : LabelPropertyEditorBase
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="DecimalLabelPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public DecimalLabelPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory, ioHelper)
    {
    }
}
