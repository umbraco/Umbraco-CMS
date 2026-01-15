// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a block list property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.BlockList,
    ValueType = ValueTypes.Json,
    ValueEditorIsReusable = false)]
public class BlockListPropertyEditor : BlockListPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockListPropertyEditor"/> class.
    /// </summary>
    public BlockListPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory,
        IJsonSerializer jsonSerializer)
        : base(dataValueEditorFactory, blockValuePropertyIndexValueFactory, jsonSerializer)
        => _ioHelper = ioHelper;

    /// <inheritdoc/>
    public override bool SupportsConfigurableElements => true;

    /// <inheritdoc />
    public override bool CanMergePartialPropertyValues(IPropertyType propertyType) => propertyType.VariesByCulture() is false;

    /// <inheritdoc />
    public override object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture)
    {
        var valueEditor = (BlockListEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergePartialPropertyValueForCulture(sourceValue, targetValue, culture);
    }

    /// <inheritdoc/>
    public override object? MergeVariantInvariantPropertyValue(
        object? sourceValue,
        object? targetValue,
        bool canUpdateInvariantData,
        HashSet<string> allowedCultures)
    {
        var valueEditor = (BlockListEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergeVariantInvariantPropertyValue(sourceValue, targetValue, canUpdateInvariantData, allowedCultures);
    }

    /// <inheritdoc/>
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new BlockListConfigurationEditor(_ioHelper);
}
