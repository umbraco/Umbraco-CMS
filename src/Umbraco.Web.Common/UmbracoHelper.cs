using System.Xml.XPath;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Dictionary;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Core.Templates;
using Umbraco.Cms.Core.Xml;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common;

/// <summary>
///     A helper class that provides many useful methods and functionality for using Umbraco in templates
/// </summary>
/// <remarks>
///     This object is a request based lifetime
/// </remarks>
public class UmbracoHelper
{
    private readonly IUmbracoComponentRenderer _componentRenderer;
    private readonly ICultureDictionaryFactory _cultureDictionaryFactory;
    private readonly IPublishedContentQuery _publishedContentQuery;
    private ICultureDictionary? _cultureDictionary;

    private IPublishedContent? _currentPage;

    #region Constructors

    /// <summary>
    ///     Initializes a new instance of <see cref="UmbracoHelper" />.
    /// </summary>
    /// <param name="cultureDictionary"></param>
    /// <param name="componentRenderer"></param>
    /// <param name="publishedContentQuery"></param>
    /// <remarks>Sets the current page to the context's published content request's content item.</remarks>
    public UmbracoHelper(
        ICultureDictionaryFactory cultureDictionary,
        IUmbracoComponentRenderer componentRenderer,
        IPublishedContentQuery publishedContentQuery)
    {
        _cultureDictionaryFactory = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
        _componentRenderer = componentRenderer ?? throw new ArgumentNullException(nameof(componentRenderer));
        _publishedContentQuery =
            publishedContentQuery ?? throw new ArgumentNullException(nameof(publishedContentQuery));
    }

    /// <summary>
    ///     Initializes a new empty instance of <see cref="UmbracoHelper" />.
    /// </summary>
    /// <remarks>For tests - nothing is initialized.</remarks>
#pragma warning disable CS8618
    internal UmbracoHelper()
#pragma warning restore CS8618
    {
    }

    #endregion

    /// <summary>
    ///     Gets (or sets) the current <see cref="IPublishedContent" /> item assigned to the UmbracoHelper.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Note that this is the assigned IPublishedContent item to the
    ///         UmbracoHelper, this is not necessarily the Current IPublishedContent
    ///         item being rendered that is assigned to the UmbracoContext.
    ///         This IPublishedContent object is contextual to the current UmbracoHelper instance.
    ///     </para>
    ///     <para>
    ///         In some cases accessing this property will throw an exception if
    ///         there is not IPublishedContent assigned to the Helper this will
    ///         only ever happen if the Helper is constructed via DI during a non front-end request.
    ///     </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the
    ///     UmbracoHelper is constructed with an UmbracoContext and it is not a
    ///     front-end request.
    /// </exception>
    public IPublishedContent AssignedContentItem
    {
        get
        {
            if (_currentPage != null)
            {
                return _currentPage;
            }

            throw new InvalidOperationException(
                $"Cannot return the {nameof(IPublishedContent)} because the {nameof(UmbracoHelper)} was not constructed with an {nameof(IPublishedContent)}.");
        }
        set => _currentPage = value;
    }

    /// <summary>
    ///     Renders the template for the specified pageId and an optional altTemplateId
    /// </summary>
    /// <param name="contentId">The content id</param>
    /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
    public async Task<IHtmlEncodedString> RenderTemplateAsync(int contentId, int? altTemplateId = null)
        => await _componentRenderer.RenderTemplateAsync(contentId, altTemplateId);

    #region RenderMacro

    /// <summary>
    ///     Renders the macro with the specified alias.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <returns></returns>
    public async Task<IHtmlEncodedString> RenderMacroAsync(string alias)
        => await _componentRenderer.RenderMacroAsync(AssignedContentItem.Id, alias, null);

    /// <summary>
    ///     Renders the macro with the specified alias, passing in the specified parameters.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public async Task<IHtmlEncodedString> RenderMacroAsync(string alias, object parameters)
        => await _componentRenderer.RenderMacroAsync(AssignedContentItem.Id, alias, parameters.ToDictionary<object>());

    /// <summary>
    ///     Renders the macro with the specified alias, passing in the specified parameters.
    /// </summary>
    /// <param name="alias">The alias.</param>
    /// <param name="parameters">The parameters.</param>
    /// <returns></returns>
    public async Task<IHtmlEncodedString> RenderMacroAsync(string alias, IDictionary<string, object> parameters)
        => await _componentRenderer.RenderMacroAsync(AssignedContentItem.Id, alias, parameters);

    #endregion

    #region Dictionary

    /// <summary>
    ///     Returns the dictionary value for the key specified
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string? GetDictionaryValue(string key) => CultureDictionary[key];

    /// <summary>
    ///     Returns the dictionary value for the key specified, and if empty returns the specified default fall back value
    /// </summary>
    /// <param name="key">key of dictionary item</param>
    /// <param name="altText">fall back text if dictionary item is empty - Name altText to match Umbraco.Field</param>
    /// <returns></returns>
    public string GetDictionaryValue(string key, string altText)
    {
        var dictionaryValue = GetDictionaryValue(key);
        if (string.IsNullOrWhiteSpace(dictionaryValue))
        {
            dictionaryValue = altText;
        }

        return dictionaryValue;
    }

    /// <summary>
    ///     Returns the ICultureDictionary for access to dictionary items
    /// </summary>
    public ICultureDictionary CultureDictionary => _cultureDictionary ??= _cultureDictionaryFactory.CreateDictionary();

    #endregion

    #region Content

    /// <summary>
    ///     Gets a content item from the cache.
    /// </summary>
    /// <param name="id">The unique identifier, or the key, of the content item.</param>
    /// <returns>The content, or null of the content item is not in the cache.</returns>
    public IPublishedContent? Content(object id) => ContentForObject(id);

    private IPublishedContent? ContentForObject(object id) => _publishedContentQuery.Content(id);

    public IPublishedContent? ContentSingleAtXPath(string xpath, params XPathVariable[] vars) =>
        _publishedContentQuery.ContentSingleAtXPath(xpath, vars);

    /// <summary>
    ///     Gets a content item from the cache.
    /// </summary>
    /// <param name="id">The unique identifier of the content item.</param>
    /// <returns>The content, or null of the content item is not in the cache.</returns>
    public IPublishedContent? Content(int id) => _publishedContentQuery.Content(id);

    /// <summary>
    ///     Gets a content item from the cache.
    /// </summary>
    /// <param name="id">The key of the content item.</param>
    /// <returns>The content, or null of the content item is not in the cache.</returns>
    public IPublishedContent? Content(Guid id) => _publishedContentQuery.Content(id);

    /// <summary>
    ///     Gets a content item from the cache.
    /// </summary>
    /// <param name="id">The unique identifier, or the key, of the content item.</param>
    /// <returns>The content, or null of the content item is not in the cache.</returns>
    public IPublishedContent? Content(string id) => _publishedContentQuery.Content(id);

    public IPublishedContent? Content(Udi id) => _publishedContentQuery.Content(id);

    /// <summary>
    ///     Gets content items from the cache.
    /// </summary>
    /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
    /// <returns>The content items that were found in the cache.</returns>
    /// <remarks>Does not support mixing identifiers and keys.</remarks>
    public IEnumerable<IPublishedContent> Content(params object[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(params Udi[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(params GuidUdi[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets content items from the cache.
    /// </summary>
    /// <param name="ids">The unique identifiers of the content items.</param>
    /// <returns>The content items that were found in the cache.</returns>
    public IEnumerable<IPublishedContent> Content(params int[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets content items from the cache.
    /// </summary>
    /// <param name="ids">The keys of the content items.</param>
    /// <returns>The content items that were found in the cache.</returns>
    public IEnumerable<IPublishedContent> Content(params Guid[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets content items from the cache.
    /// </summary>
    /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
    /// <returns>The content items that were found in the cache.</returns>
    /// <remarks>Does not support mixing identifiers and keys.</remarks>
    public IEnumerable<IPublishedContent> Content(params string[] ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(IEnumerable<Udi> ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(IEnumerable<GuidUdi> ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(IEnumerable<string> ids) => _publishedContentQuery.Content(ids);

    /// <summary>
    ///     Gets the contents corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The content identifiers.</param>
    /// <returns>The existing contents corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids) => _publishedContentQuery.Content(ids);

    public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars) =>
        _publishedContentQuery.ContentAtXPath(xpath, vars);

    public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars) =>
        _publishedContentQuery.ContentAtXPath(xpath, vars);

    public IEnumerable<IPublishedContent> ContentAtRoot() => _publishedContentQuery.ContentAtRoot();

    #endregion

    #region Media

    public IPublishedContent? Media(Udi id) => _publishedContentQuery.Media(id);

    public IPublishedContent? Media(Guid id) => _publishedContentQuery.Media(id);

    /// <summary>
    ///     Overloaded method accepting an 'object' type
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    /// <remarks>
    ///     We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass
    ///     this result in to this method.
    ///     This method will throw an exception if the value is not of type int or string.
    /// </remarks>
    public IPublishedContent? Media(object id) => MediaForObject(id);

    private IPublishedContent? MediaForObject(object id) => _publishedContentQuery.Media(id);

    public IPublishedContent? Media(int id) => _publishedContentQuery.Media(id);

    public IPublishedContent? Media(string id) => _publishedContentQuery.Media(id);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(params object[] ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(params int[] ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(params string[] ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(params Udi[] ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(params GuidUdi[] ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(IEnumerable<Udi> ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(IEnumerable<GuidUdi> ids) => _publishedContentQuery.Media(ids);

    /// <summary>
    ///     Gets the medias corresponding to the identifiers.
    /// </summary>
    /// <param name="ids">The media identifiers.</param>
    /// <returns>The existing medias corresponding to the identifiers.</returns>
    /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
    public IEnumerable<IPublishedContent> Media(IEnumerable<string> ids) => _publishedContentQuery.Media(ids);

    public IEnumerable<IPublishedContent> MediaAtRoot() => _publishedContentQuery.MediaAtRoot();

    #endregion
}
