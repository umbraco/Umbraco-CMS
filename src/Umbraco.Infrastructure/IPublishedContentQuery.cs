using System.Xml.XPath;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Xml;

namespace Umbraco.Cms.Core;

/// <summary>
///     Query methods used for accessing strongly typed content in templates.
/// </summary>
public interface IPublishedContentQuery
{
    IPublishedContent? Content(int id);

    IPublishedContent? Content(Guid id);

    IPublishedContent? Content(Udi id);

    IPublishedContent? Content(object id);

    IPublishedContent? ContentSingleAtXPath(string xpath, params XPathVariable[] vars);

    IEnumerable<IPublishedContent> Content(IEnumerable<int> ids);

    IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids);

    IEnumerable<IPublishedContent> Content(IEnumerable<object> ids);

    IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars);

    IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars);

    IEnumerable<IPublishedContent> ContentAtRoot();

    IPublishedContent? Media(int id);

    IPublishedContent? Media(Guid id);

    IPublishedContent? Media(Udi id);

    IPublishedContent? Media(object id);

    IEnumerable<IPublishedContent> Media(IEnumerable<int> ids);

    IEnumerable<IPublishedContent> Media(IEnumerable<object> ids);

    IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids);

    IEnumerable<IPublishedContent> MediaAtRoot();
}
