// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a URL picker property editor holding any number of links.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultiUrlPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultiUrlPickerPropertyEditor : MultiUrlPickerPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerPropertyEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">Provides file system operations.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    public MultiUrlPickerPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiUrlPickerConfigurationEditor(_ioHelper);
}
