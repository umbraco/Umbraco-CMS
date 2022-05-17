using System.ComponentModel;
using System.Xml.XPath;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Xml;

namespace Umbraco.Cms.Core.PublishedCache.Internal;

// TODO: Only used in unit tests, needs to be moved to test project
[EditorBrowsable(EditorBrowsableState.Never)]
public sealed class InternalPublishedContentCache : PublishedCacheBase, IPublishedContentCache, IPublishedMediaCache
{
    private readonly Dictionary<int, IPublishedContent> _content = new();

    public InternalPublishedContentCache()
        : base(false)
    {
    }

    public IPublishedContent GetByRoute(bool preview, string route, bool? hideTopLevelNode = null, string? culture = null) => throw new NotImplementedException();

    public IPublishedContent GetByRoute(string route, bool? hideTopLevelNode = null, string? culture = null) =>
        throw new NotImplementedException();

    public string GetRouteById(bool preview, int contentId, string? culture = null) =>
        throw new NotImplementedException();

    public string GetRouteById(int contentId, string? culture = null) => throw new NotImplementedException();

    public override IPublishedContent? GetById(bool preview, int contentId) =>
        _content.ContainsKey(contentId) ? _content[contentId] : null;

    public override IPublishedContent GetById(bool preview, Guid contentId) => throw new NotImplementedException();

    public override IPublishedContent GetById(bool preview, Udi nodeId) => throw new NotSupportedException();

    public override bool HasById(bool preview, int contentId) => _content.ContainsKey(contentId);

    public override IEnumerable<IPublishedContent> GetAtRoot(bool preview, string? culture = null) =>
        _content.Values.Where(x => x.Parent == null);

    public override IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars) =>
        throw new NotImplementedException();

    public override IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars) =>
        throw new NotImplementedException();

    public override IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars) =>
        throw new NotImplementedException();

    public override IEnumerable<IPublishedContent>
        GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars) => throw new NotImplementedException();

    public override XPathNavigator CreateNavigator(bool preview) => throw new NotImplementedException();

    public override XPathNavigator CreateNodeNavigator(int id, bool preview) => throw new NotImplementedException();

    public override bool HasContent(bool preview) => _content.Count > 0;

    public override IPublishedContentType GetContentType(int id) => throw new NotImplementedException();

    public override IPublishedContentType GetContentType(string alias) => throw new NotImplementedException();

    public override IPublishedContentType GetContentType(Guid key) => throw new NotImplementedException();

    public override IEnumerable<IPublishedContent> GetByContentType(IPublishedContentType contentType) =>
        throw new NotImplementedException();

    // public void Add(InternalPublishedContent content) => _content[content.Id] = content.CreateModel(Mock.Of<IPublishedModelFactory>());
    public void Clear() => _content.Clear();
}
