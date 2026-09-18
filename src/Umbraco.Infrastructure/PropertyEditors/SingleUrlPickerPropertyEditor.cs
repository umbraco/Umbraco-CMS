// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a URL picker property editor holding a single link.
/// </summary>
/// <remarks>
/// Stores the same value as <see cref="MultiUrlPickerPropertyEditor" /> and is edited the same way; the two differ
/// in the shape of the value they yield, and in how many links may be picked.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.SingleUrlPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class SingleUrlPickerPropertyEditor : MultiUrlPickerPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="SingleUrlPickerPropertyEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">Provides file system operations.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    public SingleUrlPickerPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SingleUrlPickerConfigurationEditor(_ioHelper);
}
