using System.Collections;
using System.Globalization;
using System.Xml.XPath;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Xml;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure;

/// <summary>
///     A class used to query for published content, media items
/// </summary>
/// <seealso cref="Umbraco.Cms.Core.IPublishedContentQuery" />
public class PublishedContentQuery : IPublishedContentQuery
{
    private readonly IPublishedSnapshot _publishedSnapshot;
    private readonly IVariationContextAccessor _variationContextAccessor;


    /// <summary>
    ///     Initializes a new instance of the <see cref="PublishedContentQuery" /> class.
    /// </summary>
    public PublishedContentQuery(IPublishedSnapshot publishedSnapshot,
        IVariationContextAccessor variationContextAccessor)
    {
        _publishedSnapshot = publishedSnapshot ?? throw new ArgumentNullException(nameof(publishedSnapshot));
        _variationContextAccessor = variationContextAccessor ??
                                    throw new ArgumentNullException(nameof(variationContextAccessor));
    }

    #region Convert Helpers

    private static bool ConvertIdObjectToInt(object id, out int intId)
    {
        switch (id)
        {
            case string s:
                return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out intId);

            case int i:
                intId = i;
                return true;

            default:
                intId = default;
                return false;
        }
    }

    private static bool ConvertIdObjectToGuid(object id, out Guid guidId)
    {
        switch (id)
        {
            case string s:
                return Guid.TryParse(s, out guidId);

            case Guid g:
                guidId = g;
                return true;

            default:
                guidId = default;
                return false;
        }
    }

    private static bool ConvertIdObjectToUdi(object id, out Udi? guidId)
    {
        switch (id)
        {
            case string s:
                return UdiParser.TryParse(s, out guidId);

            case Udi u:
                guidId = u;
                return true;

            default:
                guidId = default;
                return false;
        }
    }

    #endregion

    #region Content

    public IPublishedContent? Content(int id)
        => ItemById(id, _publishedSnapshot.Content);

    public IPublishedContent? Content(Guid id)
        => ItemById(id, _publishedSnapshot.Content);

    public IPublishedContent? Content(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedSnapshot.Content);
    }

    public IPublishedContent? Content(object id)
    {
        if (ConvertIdObjectToInt(id, out var intId))
        {
            return Content(intId);
        }

        if (ConvertIdObjectToGuid(id, out Guid guidId))
        {
            return Content(guidId);
        }

        if (ConvertIdObjectToUdi(id, out Udi? udiId))
        {
            return Content(udiId);
        }

        return null;
    }

    public IPublishedContent? ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        => ItemByXPath(xpath, vars, _publishedSnapshot.Content);

    public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        => ItemsByIds(_publishedSnapshot.Content, ids);

    public IEnumerable<IPublishedContent> Content(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedSnapshot.Content, ids);

    public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        => ids.Select(Content).WhereNotNull();

    public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        => ItemsByXPath(xpath, vars, _publishedSnapshot.Content);

    public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        => ItemsByXPath(xpath, vars, _publishedSnapshot.Content);

    public IEnumerable<IPublishedContent> ContentAtRoot()
        => ItemsAtRoot(_publishedSnapshot.Content);

    #endregion

    #region Media

    public IPublishedContent? Media(int id)
        => ItemById(id, _publishedSnapshot.Media);

    public IPublishedContent? Media(Guid id)
        => ItemById(id, _publishedSnapshot.Media);

    public IPublishedContent? Media(Udi? id)
    {
        if (!(id is GuidUdi udi))
        {
            return null;
        }

        return ItemById(udi.Guid, _publishedSnapshot.Media);
    }

    public IPublishedContent? Media(object id)
    {
        if (ConvertIdObjectToInt(id, out var intId))
        {
            return Media(intId);
        }

        if (ConvertIdObjectToGuid(id, out Guid guidId))
        {
            return Media(guidId);
        }

        if (ConvertIdObjectToUdi(id, out Udi? udiId))
        {
            return Media(udiId);
        }

        return null;
    }

    public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        => ItemsByIds(_publishedSnapshot.Media, ids);

    public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        => ids.Select(Media).WhereNotNull();

    public IEnumerable<IPublishedContent> Media(IEnumerable<Guid> ids)
        => ItemsByIds(_publishedSnapshot.Media, ids);

    public IEnumerable<IPublishedContent> MediaAtRoot()
        => ItemsAtRoot(_publishedSnapshot.Media);

    #endregion

    #region Used by Content/Media

    private static IPublishedContent? ItemById(int id, IPublishedCache? cache)
        => cache?.GetById(id);

    private static IPublishedContent? ItemById(Guid id, IPublishedCache? cache)
        => cache?.GetById(id);

    private static IPublishedContent? ItemByXPath(string xpath, XPathVariable[] vars, IPublishedCache? cache)
        => cache?.GetSingleByXPath(xpath, vars);

    private static IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache? cache, IEnumerable<int> ids)
        => ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();

    private IEnumerable<IPublishedContent> ItemsByIds(IPublishedCache? cache, IEnumerable<Guid> ids)
        => ids.Select(eachId => ItemById(eachId, cache)).WhereNotNull();

    private static IEnumerable<IPublishedContent> ItemsByXPath(string xpath, XPathVariable[] vars,
        IPublishedCache? cache)
        => cache?.GetByXPath(xpath, vars) ?? Array.Empty<IPublishedContent>();

    private static IEnumerable<IPublishedContent> ItemsByXPath(XPathExpression xpath, XPathVariable[] vars,
        IPublishedCache? cache)
        => cache?.GetByXPath(xpath, vars) ?? Array.Empty<IPublishedContent>();

    private static IEnumerable<IPublishedContent> ItemsAtRoot(IPublishedCache? cache)
        => cache?.GetAtRoot() ?? Array.Empty<IPublishedContent>();

    #endregion


}
