using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Xml.XPath;

namespace Umbraco.Cms.Infrastructure.PublishedCache.Navigable;

internal class Source : INavigableSource
{
    private readonly INavigableData _data;
    private readonly bool _preview;
    private readonly RootContent _root;

    public Source(INavigableData data, bool preview)
    {
        _data = data;
        _preview = preview;

        IEnumerable<IPublishedContent> contentAtRoot = data.GetAtRoot(preview);
        _root = new RootContent(contentAtRoot.Select(x => x.Id));
    }

    public int LastAttributeIndex => NavigableContentType.BuiltinProperties.Length - 1;

    public INavigableContent? Get(int id)
    {
        // wrap in a navigable content
        IPublishedContent? content = _data.GetById(_preview, id);
        return content == null ? null : new NavigableContent(content);
    }

    public INavigableContent Root => _root;
}
