// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a slider editor holding a range of two values.
/// </summary>
/// <remarks>
/// Holds the same value as <see cref="SliderPropertyEditor" /> and is edited the same way; the two differ in the
/// shape of the value they yield, and in whether the two ends of that value may differ.
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.RangeSlider,
    ValueEditorIsReusable = true)]
public class RangeSliderPropertyEditor : SliderPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="RangeSliderPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for the property editor.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    public RangeSliderPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
        => _ioHelper = ioHelper;

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new RangeSliderConfigurationEditor(_ioHelper);
}
