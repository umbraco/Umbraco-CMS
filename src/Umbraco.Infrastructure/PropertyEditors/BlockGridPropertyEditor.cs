// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a block list property editor.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.BlockGrid,
    ValueType = ValueTypes.Json)]
public class BlockGridPropertyEditor : BlockGridPropertyEditorBase
{
    private readonly IIOHelper _ioHelper;

    public BlockGridPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory, blockValuePropertyIndexValueFactory)
        => _ioHelper = ioHelper;

    public override bool SupportsConfigurableElements => true;

    /// <inheritdoc />
    public override bool CanMergePartialPropertyValues(IPropertyType propertyType) => propertyType.VariesByCulture() is false;

    /// <inheritdoc />
    public override object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture)
    {
        var valueEditor = (BlockGridEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergePartialPropertyValueForCulture(sourceValue, targetValue, culture);
    }

    public override object? MergeVariantInvariantPropertyValue(
        object? sourceValue,
        object? targetValue,
        bool canUpdateInvariantData,
        HashSet<string> allowedCultures)
    {
        var valueEditor = (BlockGridEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergeVariantInvariantPropertyValue(sourceValue, targetValue, canUpdateInvariantData,allowedCultures);
    }

    #region Pre Value Editor

    protected override IConfigurationEditor CreateConfigurationEditor() => new BlockGridConfigurationEditor(_ioHelper);

    #endregion
}
