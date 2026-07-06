using HeyRed.MarkdownSharp;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

internal sealed class MarkdownPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    private readonly IHtmlIndexValueParser _htmlIndexValueParser;

    public MarkdownPropertyValueHandler(IHtmlIndexValueParser htmlIndexValueParser)
        => _htmlIndexValueParser = htmlIndexValueParser;

    public bool CanHandle(string propertyEditorAlias)
        => propertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.MarkdownEditor;

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
