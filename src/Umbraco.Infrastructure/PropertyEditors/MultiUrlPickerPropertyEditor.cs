// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a property editor in Umbraco that enables users to select and manage multiple URLs within a property.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.MultiUrlPicker,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = true)]
public class MultiUrlPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="MultiUrlPickerPropertyEditor"/> class.
    /// </summary>
    /// <param name="ioHelper">Provides file system operations.</param>
    /// <param name="dataValueEditorFactory">Factory for creating data value editors.</param>
    public MultiUrlPickerPropertyEditor(IIOHelper ioHelper, IDataValueEditorFactory dataValueEditorFactory)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <summary>
    /// Gets the property index value factory used by the multi URL picker property editor.
    /// By default, this returns a <see cref="NoopPropertyIndexValueFactory"/>, indicating that no custom indexing is performed for this property editor.
    /// </summary>
    public override IPropertyIndexValueFactory PropertyIndexValueFactory { get; } = new NoopPropertyIndexValueFactory();

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MultiUrlPickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MultiUrlPickerValueEditor>(Attribute!);
}
