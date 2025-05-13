// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

public abstract class BlockEditorPropertyValueEditor<TValue, TLayout> : BlockValuePropertyValueEditorBase<TValue, TLayout>
    where TValue : BlockValue<TLayout>, new()
    where TLayout : class, IBlockLayoutItem, new()
{
    [Obsolete("Please use the non-obsolete constructor. Will be removed in V16.")]
    protected BlockEditorPropertyValueEditor(
        DataEditorAttribute attribute,
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        ILocalizedTextService textService,
        ILogger<BlockEditorPropertyValueEditor<TValue, TLayout>> logger,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        IIOHelper ioHelper)
        : this(propertyEditors, dataValueReferenceFactories, dataTypeConfigurationCache, shortStringHelper, jsonSerializer,
            StaticServiceProvider.Instance.GetRequiredService<BlockEditorVarianceHandler>(),
            StaticServiceProvider.Instance.GetRequiredService<ILanguageService>(),
            ioHelper,
            attribute)
    {
    }

    protected BlockEditorPropertyValueEditor(
        PropertyEditorCollection propertyEditors,
        DataValueReferenceFactoryCollection dataValueReferenceFactories,
        IDataTypeConfigurationCache dataTypeConfigurationCache,
        IShortStringHelper shortStringHelper,
        IJsonSerializer jsonSerializer,
        BlockEditorVarianceHandler blockEditorVarianceHandler,
        ILanguageService languageService,
        IIOHelper ioHelper,
        DataEditorAttribute attribute)
        : base(propertyEditors, dataTypeConfigurationCache, shortStringHelper, jsonSerializer, dataValueReferenceFactories, blockEditorVarianceHandler, languageService, ioHelper, attribute) =>
        JsonSerializer = jsonSerializer;

    /// <summary>
    /// Gets the <see cref="IJsonSerializer"/>.
    /// </summary>
    protected IJsonSerializer JsonSerializer { get; }

    /// <inheritdoc />
    public override IEnumerable<UmbracoEntityReference> GetReferences(object? value)
    {
        TValue? blockValue = ParseBlockValue(value);
        return blockValue is not null
            ? GetBlockValueReferences(blockValue)
            : Enumerable.Empty<UmbracoEntityReference>();
    }

    /// <inheritdoc />
    public override IEnumerable<ITag> GetTags(object? value, object? dataTypeConfiguration, int? languageId)
    {
        TValue? blockValue = ParseBlockValue(value);
        return blockValue is not null
            ? GetBlockValueTags(blockValue, languageId)
            : Enumerable.Empty<ITag>();
    }

    private TValue? ParseBlockValue(object? value)
    {
        var rawJson = value == null ? string.Empty : value is string str ? str : value.ToString();
        return BlockEditorValues.DeserializeAndClean(rawJson)?.BlockValue;
    }

    /// <summary>
    ///     Ensure that sub-editor values are translated through their ToEditor methods
    /// </summary>
    /// <param name="property"></param>
    /// <param name="culture"></param>
    /// <param name="segment"></param>
    /// <returns></returns>
    public override object ToEditor(IProperty property, string? culture = null, string? segment = null)
    {
        var val = property.GetValue(culture, segment);

        BlockEditorData<TValue, TLayout>? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(val);
        }
        catch
        {
            // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
            return string.Empty;
        }

        if (blockEditorData == null)
        {
            return string.Empty;
        }

        MapBlockValueToEditor(property, blockEditorData.BlockValue, culture, segment);

        // return json convertable object
        return blockEditorData.BlockValue;
    }

    /// <summary>
    ///     Ensure that sub-editor values are translated through their FromEditor methods
    /// </summary>
    /// <param name="editorValue"></param>
    /// <param name="currentValue"></param>
    /// <returns></returns>
    public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
    {
        if (editorValue.Value == null || string.IsNullOrWhiteSpace(editorValue.Value.ToString()))
        {
            return null;
        }

        BlockEditorData<TValue, TLayout>? blockEditorData;
        try
        {
            blockEditorData = BlockEditorValues.DeserializeAndClean(editorValue.Value);
        }
        catch
        {
            // if this occurs it means the data is invalid, shouldn't happen but has happened if we change the data format.
            return string.Empty;
        }

        if (blockEditorData == null || blockEditorData.BlockValue.ContentData.Count == 0)
        {
            return string.Empty;
        }

        MapBlockValueFromEditor(blockEditorData.BlockValue);

        // return json
        return JsonSerializer.Serialize(blockEditorData.BlockValue);
    }
}
