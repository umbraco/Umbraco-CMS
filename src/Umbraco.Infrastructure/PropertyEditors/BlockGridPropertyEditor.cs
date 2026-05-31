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

    /// <summary>
    /// Initializes a new instance of the <see cref="BlockGridPropertyEditor"/> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">Factory used to create data value editors for property editing.</param>
    /// <param name="ioHelper">Helper for IO operations, such as file and path handling.</param>
    /// <param name="blockValuePropertyIndexValueFactory">Factory for creating index values for block value properties.</param>
    public BlockGridPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IBlockValuePropertyIndexValueFactory blockValuePropertyIndexValueFactory)
        : base(dataValueEditorFactory, blockValuePropertyIndexValueFactory)
        => _ioHelper = ioHelper;

    /// <summary>
    /// Gets a value indicating whether the <see cref="BlockGridPropertyEditor"/> supports configurable elements. This property always returns <c>true</c>.
    /// </summary>
    public override bool SupportsConfigurableElements => true;

    /// <inheritdoc />
    public override bool CanMergePartialPropertyValues(IPropertyType propertyType) => propertyType.VariesByCulture() is false;

    /// <inheritdoc />
    public override object? MergePartialPropertyValueForCulture(object? sourceValue, object? targetValue, string? culture)
    {
        var valueEditor = (BlockGridEditorPropertyValueEditor)GetValueEditor();
        return valueEditor.MergePartialPropertyValueForCulture(sourceValue, targetValue, culture);
    }

    /// <summary>
    /// Merges variant (culture-specific) and invariant (culture-neutral) property values for the Block Grid editor.
    /// </summary>
    /// <param name="sourceValue">The source property value to merge from, typically representing the incoming or updated value.</param>
    /// <param name="targetValue">The target property value to merge into, typically representing the existing or persisted value.</param>
    /// <param name="canUpdateInvariantData">True if invariant (culture-neutral) data is allowed to be updated during the merge; otherwise, false.</param>
    /// <param name="allowedCultures">A set of culture codes that are permitted for merging variant values.</param>
    /// <returns>The resulting merged property value, or <c>null</c> if the merge produces no value.</returns>
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
