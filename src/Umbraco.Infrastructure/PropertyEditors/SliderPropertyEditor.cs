// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a slider editor holding a single value.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.Slider,
    ValueEditorIsReusable = true)]
public class SliderPropertyEditor : SliderPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SliderPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for the slider property editor.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    public SliderPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new SliderConfigurationEditor(_ioHelper);
}
