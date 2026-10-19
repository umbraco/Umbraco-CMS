using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Search.Indexing;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Infrastructure.Search.PropertyValueHandlers;

/// <summary>
/// Indexes markdown property values by converting the markdown to HTML and parsing it into indexable text.
/// </summary>
internal sealed class MarkdownPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IHtmlIndexValueParser _htmlIndexValueParser;
    private readonly IMarkdownToHtmlConverter _markdownToHtmlConverter;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="htmlIndexValueParser">The parser used to extract indexable text from the markdown, once converted to HTML.</param>
    /// <param name="markdownToHtmlConverter">The converter used to turn the markdown into HTML.</param>
    public MarkdownPropertyValueHandler(IHtmlIndexValueParser htmlIndexValueParser, IMarkdownToHtmlConverter markdownToHtmlConverter)
    {
        _htmlIndexValueParser = htmlIndexValueParser;
        _markdownToHtmlConverter = markdownToHtmlConverter;
    }

    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.MarkdownEditor;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
    {
        if (property.GetValue(culture, segment, published) is not string markdown)
        {
            return [];
        }

        var html = _markdownToHtmlConverter.ToHtml(markdown);

        IndexValue? indexValue = _htmlIndexValueParser.Parse(html);
        return indexValue is not null
            ? [new IndexField(property.Alias, indexValue, culture, segment)]
            : [];
    }
}
