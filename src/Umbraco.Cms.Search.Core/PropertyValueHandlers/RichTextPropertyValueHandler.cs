using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Search.Core.Models.Indexing;
using Umbraco.Cms.Search.Core.PropertyValueHandlers.Collection;
using IndexValue = Umbraco.Cms.Search.Core.Models.Indexing.IndexValue;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes rich text property values, extracting HTML text (weighted by heading level) and recursively indexing
/// any blocks embedded in the rich text.
/// </summary>
internal sealed class RichTextPropertyValueHandler : BlockEditorPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IJsonSerializer _jsonSerializer;
    private readonly IHtmlIndexValueParser _htmlIndexValueParser;
    private readonly ILogger<RichTextPropertyValueHandler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="RichTextPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="jsonSerializer">The JSON serializer used to deserialize the rich text property's stored value.</param>
    /// <param name="contentTypeService">The service used to resolve the embedded blocks' element types.</param>
    /// <param name="propertyEditorCollection">The property editor collection used to resolve each embedded block property's editor.</param>
    /// <param name="propertyValueHandlerCollection">The property value handler collection used to index each embedded block property's value.</param>
    /// <param name="htmlIndexValueParser">The parser used to extract indexable, relevance-weighted text from the rich text markup.</param>
    /// <param name="logger">The logger used to record diagnostic information when indexing the rich text value or its embedded blocks.</param>
    public RichTextPropertyValueHandler(
        IJsonSerializer jsonSerializer,
        IContentTypeService contentTypeService,
        PropertyEditorCollection propertyEditorCollection,
        PropertyValueHandlerCollection propertyValueHandlerCollection,
        IHtmlIndexValueParser htmlIndexValueParser,
        ILogger<RichTextPropertyValueHandler> logger)
        : base(jsonSerializer, contentTypeService, propertyEditorCollection, propertyValueHandlerCollection, logger)
    {
        _htmlIndexValueParser = htmlIndexValueParser;
        _logger = logger;
        _jsonSerializer = jsonSerializer;
    }

    /// <inheritdoc />
    public override bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.RichText;

    /// <inheritdoc />
    public override IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        var source = property.GetValue(culture, segment, published);
        if (RichTextPropertyEditorHelper.TryParseRichTextEditorValue(source, _jsonSerializer, _logger, out RichTextEditorValue? richTextEditorValue) is false)
        {
            return [];
        }

        Dictionary<(string? Culture, string? Segment), CumulativeIndexValue> blockIndexValues = richTextEditorValue.Blocks is not null
            ? GetCumulativeIndexValues(richTextEditorValue.Blocks.ContentData, richTextEditorValue.Blocks.Expose, property, culture, segment, published, contentContext)
            : new ();

        IndexValue? htmlFieldValue = _htmlIndexValueParser.Parse(richTextEditorValue.Markup);
        if (htmlFieldValue is not null)
        {
            if (blockIndexValues.TryGetValue((culture, segment), out CumulativeIndexValue? fieldValue) is false)
            {
                fieldValue = new();
                blockIndexValues[(culture, segment)] = fieldValue;
            }

            AmendCumulativeIndexValue(fieldValue, htmlFieldValue);
        }

        return blockIndexValues.Count > 0
            ? ToIndexFields(blockIndexValues, property.Alias)
            : [];
    }
}
