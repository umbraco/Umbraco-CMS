using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Blocks;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.Examine;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

internal class RichTextPropertyIndexValueFactory : NestedPropertyIndexValueFactoryBase<RichTextEditorValue, BlockItemData>, IRichTextPropertyIndexValueFactory
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IContentTypeService _contentTypeService;
    private readonly ILogger<RichTextPropertyIndexValueFactory> _logger;

    public RichTextPropertyIndexValueFactory(
        PropertyEditorCollection propertyEditorCollection,
        IJsonSerializer jsonSerializer,
        IOptionsMonitor<IndexingSettings> indexingSettings,
        IContentTypeService contentTypeService,
        ILogger<RichTextPropertyIndexValueFactory> logger)
        : base(propertyEditorCollection, jsonSerializer, indexingSettings)
    {
        _jsonSerializer = jsonSerializer;
        _contentTypeService = contentTypeService;
        _logger = logger;
    }

    public new IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(
        IProperty property,
        string? culture,
        string? segment,
        bool published,
        IEnumerable<string> availableCultures,
        IDictionary<Guid, IContentType> contentTypeDictionary)
    {
        var val = property.GetValue(culture, segment, published);
        if (RichTextPropertyEditorHelper.TryParseRichTextEditorValue(val, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue) is false)
        {
            yield break;
        }

        // the "blocks values resume" (the combined searchable text values from all blocks) is stored as a string value under the property alias by the base implementation
        var blocksIndexValues = base.GetIndexValues(property, culture, segment, published, availableCultures, contentTypeDictionary).ToDictionary(pair => pair.Key, pair => pair.Value);
        var blocksIndexValuesResume = blocksIndexValues.TryGetValue(property.Alias, out IEnumerable<object?>? blocksIndexValuesResumeValue)
                ? blocksIndexValuesResumeValue.FirstOrDefault() as string
                : null;

        // index the stripped HTML values combined with "blocks values resume" value
        yield return new KeyValuePair<string, IEnumerable<object?>>(
            property.Alias,
            new object[] { $"{richTextEditorValue.Markup.StripHtml()} {blocksIndexValuesResume}" });

        // store the raw value
        yield return new KeyValuePair<string, IEnumerable<object?>>(
            $"{UmbracoExamineFieldNames.RawFieldPrefix}{property.Alias}", new object[] { richTextEditorValue.Markup });
    }

    [Obsolete("Use the overload with the 'availableCultures' parameter instead, scheduled for removal in v14")]
    public IEnumerable<KeyValuePair<string, IEnumerable<object?>>> GetIndexValues(IProperty property, string? culture, string? segment, bool published)
        => GetIndexValues(property, culture, segment, published, Enumerable.Empty<string>());

    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData nestedItem, IDictionary<Guid, IContentType> contentTypeDictionary)
        => contentTypeDictionary.TryGetValue(nestedItem.ContentTypeKey, out var result) ? result : null;

    [Obsolete("Use non-obsolete overload. Scheduled for removal in Umbraco 14.")]
    protected override IContentType? GetContentTypeOfNestedItem(BlockItemData nestedItem)
        => _contentTypeService.Get(nestedItem.ContentTypeKey);

    protected override IDictionary<string, object?> GetRawProperty(BlockItemData blockItemData)
        => blockItemData.RawPropertyValues;

    protected override IEnumerable<BlockItemData> GetDataItems(RichTextEditorValue input)
        => input.Blocks?.ContentData ?? new List<BlockItemData>();
}
