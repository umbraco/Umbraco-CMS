using HeyRed.MarkdownSharp;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Indexes markdown property values by converting the markdown to HTML and parsing it into indexable text.
/// </summary>
internal sealed class MarkdownPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IHtmlIndexValueParser _htmlIndexValueParser;

    /// <summary>
    /// Initializes a new instance of the <see cref="MarkdownPropertyValueHandler"/> class.
    /// </summary>
    /// <param name="htmlIndexValueParser">The parser used to extract indexable text from the markdown, once converted to HTML.</param>
    public MarkdownPropertyValueHandler(IHtmlIndexValueParser htmlIndexValueParser)
        => _htmlIndexValueParser = htmlIndexValueParser;

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

        var mark = new Markdown();
        var html = mark.Transform(markdown);

        IndexValue? indexValue = _htmlIndexValueParser.Parse(html);
        return indexValue is not null
            ? [new IndexField(property.Alias, indexValue, culture, segment)]
            : [];
    }
}
