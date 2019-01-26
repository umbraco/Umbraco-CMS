using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Exceptions;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Core.Composing;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web
{
    using Examine = global::Examine;

    /// <summary>
    /// A helper class that provides many useful methods and functionality for using Umbraco in templates
    /// </summary>
    public class UmbracoHelper : IUmbracoComponentRenderer
    {
        private static readonly HtmlStringUtilities StringUtilities = new HtmlStringUtilities();

        private readonly UmbracoContext _umbracoContext;
        private readonly IPublishedContent _currentPage;
        private readonly ServiceContext _services;
        
        private IUmbracoComponentRenderer _componentRenderer;
        private IPublishedContentQuery _query;
        private MembershipHelper _membershipHelper;
        private ITagQuery _tag;
        private ICultureDictionary _cultureDictionary;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoHelper"/> class.
        /// </summary>
        /// <remarks>For tests.</remarks>
        internal UmbracoHelper(UmbracoContext umbracoContext, IPublishedContent content,
            ITagQuery tagQuery,
            ICultureDictionary cultureDictionary,
            IUmbracoComponentRenderer componentRenderer,
            MembershipHelper membershipHelper,
            ServiceContext services)
        {
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _tag = tagQuery ?? throw new ArgumentNullException(nameof(tagQuery));
            _cultureDictionary = cultureDictionary ?? throw new ArgumentNullException(nameof(cultureDictionary));
            _componentRenderer = componentRenderer ?? throw new ArgumentNullException(nameof(componentRenderer));
            _membershipHelper = membershipHelper ?? throw new ArgumentNullException(nameof(membershipHelper));
            _currentPage = content ?? throw new ArgumentNullException(nameof(content));
            _services = services ?? throw new ArgumentNullException(nameof(services));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoHelper"/> class.
        /// </summary>
        /// <remarks>For tests - nothing is initialized.</remarks>
        internal UmbracoHelper()
        { }


        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoHelper"/> class with an Umbraco context
        /// and a specific content item.
        /// </summary>
        /// <param name="umbracoContext">An Umbraco context.</param>
        /// <param name="content">A content item.</param>
        /// <param name="services">A services context.</param>
        /// <remarks>Sets the current page to the supplied content item.</remarks>
        public UmbracoHelper(UmbracoContext umbracoContext, ServiceContext services, IPublishedContent content)
            : this(umbracoContext, services)
        {
            _currentPage = content ?? throw new ArgumentNullException(nameof(content));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UmbracoHelper"/> class with an Umbraco context.
        /// </summary>
        /// <param name="umbracoContext">An Umbraco context.</param>
        /// <param name="services">A services context.</param>
        /// <remarks>Sets the current page to the context's published content request's content item.</remarks>
        public UmbracoHelper(UmbracoContext umbracoContext, ServiceContext services)
        {
            _services = services ?? throw new ArgumentNullException(nameof(services));
            _umbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            if (_umbracoContext.IsFrontEndUmbracoRequest)
                _currentPage = _umbracoContext.PublishedRequest.PublishedContent;
        }

        #endregion

        /// <summary>
        /// Gets the tag context.
        /// </summary>
        public ITagQuery TagQuery => _tag ??
            (_tag = new TagQuery(_services.TagService, ContentQuery));

        /// <summary>
        /// Gets the query context.
        /// </summary>
        public IPublishedContentQuery ContentQuery => _query ??
            (_query = new PublishedContentQuery(UmbracoContext.ContentCache, UmbracoContext.MediaCache, UmbracoContext.VariationContextAccessor));

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get
            {
                if (_umbracoContext == null)
                    throw new NullReferenceException("UmbracoContext has not been set.");
                return _umbracoContext;
            }
        }

        /// <summary>
        /// Gets the membership helper.
        /// </summary>
        public MembershipHelper MembershipHelper => _membershipHelper
            ?? (_membershipHelper = Current.Factory.GetInstance<MembershipHelper>());

        /// <summary>
        /// Gets the url provider.
        /// </summary>
        public UrlProvider UrlProvider => UmbracoContext.UrlProvider;

        /// <summary>
        /// Gets the component renderer.
        /// </summary>
        public IUmbracoComponentRenderer UmbracoComponentRenderer => _componentRenderer
            ?? (_componentRenderer = new UmbracoComponentRenderer(UmbracoContext));

        /// <summary>
        /// Returns the current <seealso cref="IPublishedContent"/> item
        /// assigned to the UmbracoHelper.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Note that this is the assigned IPublishedContent item to the
        /// UmbracoHelper, this is not necessarily the Current IPublishedContent
        /// item being rendered. This IPublishedContent object is contextual to
        /// the current UmbracoHelper instance.
        /// </para>
        ///<para>
        /// In some cases accessing this property will throw an exception if
        /// there is not IPublishedContent assigned to the Helper this will
        /// only ever happen if the Helper is constructed with an UmbracoContext
        /// and it is not a front-end request.
        /// </para>
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the
        /// UmbracoHelper is constructed with an UmbracoContext and it is not a
        /// front-end request.</exception>
        public IPublishedContent AssignedContentItem
        {
            get
            {
                if (_currentPage != null)
                {
                    return _currentPage;
                }

                throw new InvalidOperationException(
                    $"Cannot return the {nameof(IPublishedContent)} because the {nameof(UmbracoHelper)} was constructed with an {nameof(UmbracoContext)} and the current request is not a front-end request."
                    );

            }
        }

        /// <summary>
        /// Renders the template for the specified pageId and an optional altTemplateId
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="altTemplateId">If not specified, will use the template assigned to the node</param>
        /// <returns></returns>
        public IHtmlString RenderTemplate(int pageId, int? altTemplateId = null)
        {
            return UmbracoComponentRenderer.RenderTemplate(pageId, altTemplateId);
        }

        #region RenderMacro

        /// <summary>
        /// Renders the macro with the specified alias.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias)
        {
            return UmbracoComponentRenderer.RenderMacro(alias, new { });
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, object parameters)
        {
            return UmbracoComponentRenderer.RenderMacro(alias, parameters.ToDictionary<object>());
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters)
        {
            return UmbracoComponentRenderer.RenderMacro(alias, parameters);
        }

        #endregion

        #region Dictionary

        /// <summary>
        /// Returns the dictionary value for the key specified
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetDictionaryValue(string key)
        {
            return CultureDictionary[key];
        }

        /// <summary>
        /// Returns the dictionary value for the key specified, and if empty returns the specified default fall back value
        /// </summary>
        /// <param name="key">key of dictionary item</param>
        /// <param name="altText">fall back text if dictionary item is empty - Name altText to match Umbraco.Field</param>
        /// <returns></returns>
        public string GetDictionaryValue(string key, string altText)
        {
            var dictionaryValue = GetDictionaryValue(key);
            if (String.IsNullOrWhiteSpace(dictionaryValue))
            {
                dictionaryValue = altText;
            }
            return dictionaryValue;
        }

        /// <summary>
        /// Returns the ICultureDictionary for access to dictionary items
        /// </summary>
        public ICultureDictionary CultureDictionary => _cultureDictionary
            ?? (_cultureDictionary = Current.CultureDictionaryFactory.CreateDictionary());

        #endregion

        #region Membership

        /// <summary>
        /// Check if the current user has access to a document
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the current user has access or if the current document isn't protected</returns>
        public bool MemberHasAccess(string path)
        {
            return MembershipHelper.MemberHasAccess(path);
        }

        /// <summary>
        /// Whether or not the current member is logged in (based on the membership provider)
        /// </summary>
        /// <returns>True is the current user is logged in</returns>
        public bool MemberIsLoggedOn()
        {
            return MembershipHelper.IsLoggedIn();
        }

        #endregion

        #region Urls

        /// <summary>
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, culture);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId, UrlProviderMode mode, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, mode, culture);
        }

        /// <summary>
        /// Gets the absolute url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>The absolute url for the content.</returns>
        public string UrlAbsolute(int contentId, string culture = null)
        {
            return UrlProvider.GetUrl(contentId, true, culture);
        }

        #endregion

        #region Member/Content/Media from Udi

        public IPublishedContent PublishedContent(Udi udi)
        {
            var guidUdi = udi as GuidUdi;
            if (guidUdi == null) return null;

            var umbracoType = Constants.UdiEntityType.ToUmbracoObjectType(udi.EntityType);

            switch (umbracoType)
            {
                case UmbracoObjectTypes.Document:
                    return Content(guidUdi.Guid);
                case UmbracoObjectTypes.Media:
                    return Media(guidUdi.Guid);
                case UmbracoObjectTypes.Member:
                    return Member(guidUdi.Guid);
            }

            return null;
        }

        #endregion

        #region Members

        public IPublishedContent Member(Udi id)
        {
            var guidUdi = id as GuidUdi;
            return guidUdi == null ? null : Member(guidUdi.Guid);
        }

        public IPublishedContent Member(Guid id)
        {
            return MembershipHelper.GetByProviderKey(id);
        }

        public IPublishedContent Member(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return Member(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return Member(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return Member(udiId);
            return null;
        }

        public IPublishedContent Member(int id)
        {
            return MembershipHelper.GetById(id);
        }

        public IPublishedContent Member(string id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt ? MembershipHelper.GetById(asInt.Result) : MembershipHelper.GetByProviderKey(id);
        }

        #endregion

        #region Content

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(object id)
        {
            return ContentForObject(id);
        }

        private IPublishedContent ContentForObject(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return ContentQuery.Content(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return ContentQuery.Content(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return ContentQuery.Content(udiId);
            return null;
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(int id)
        {
            return ContentQuery.Content(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The key of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(Guid id)
        {
            return ContentQuery.Content(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent Content(string id)
        {
            return ContentForObject(id);
        }

        public IPublishedContent Content(Udi id)
        {
            return ContentQuery.Content(id);
        }

        public IPublishedContent ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> Content(params object[] ids)
        {
            return ContentForObjects(ids);
        }

        private IEnumerable<IPublishedContent> ContentForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            IEnumerable<int> intIds;
            if (ConvertIdsObjectToInts(idsA, out intIds))
                return ContentQuery.Content(intIds);
            IEnumerable<Guid> guidIds;
            if (ConvertIdsObjectToGuids(idsA, out guidIds))
                return ContentQuery.Content(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> Content(params int[] ids)
        {
            return ContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The keys of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> Content(params Guid[] ids)
        {
            return ContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> Content(params string[] ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<object> ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<string> ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Content(IEnumerable<int> ids)
        {
            return ContentQuery.Content(ids);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> ContentAtRoot()
        {
            return ContentQuery.ContentAtRoot();
        }

        internal static bool ConvertIdObjectToInt(object id, out int intId)
        {
            switch (id)
            {
                case string s:
                    return int.TryParse(s, out intId);

                case int i:
                    intId = i;
                    return true;

                default:
                    intId = default;
                    return false;
            }
        }

        internal static bool ConvertIdObjectToGuid(object id, out Guid guidId)
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

        private static bool ConvertIdsObjectToInts(IEnumerable<object> ids, out IEnumerable<int> intIds)
        {
            var list = new List<int>();
            intIds = null;
            foreach (var id in ids)
            {
                if (ConvertIdObjectToInt(id, out var intId))
                    list.Add(intId);
                else
                    return false; // if one of them is not an int, fail
            }
            intIds = list;
            return true;
        }

        private static bool ConvertIdsObjectToGuids(IEnumerable<object> ids, out IEnumerable<Guid> guidIds)
        {
            var list = new List<Guid>();
            guidIds = null;
            foreach (var id in ids)
            {
                Guid guidId;
                if (ConvertIdObjectToGuid(id, out guidId))
                    list.Add(guidId);
                else
                    return false; // if one of them is not a guid, fail
            }
            guidIds = list;
            return true;
        }

        /// <remarks>Had to change to internal for testing.</remarks>
        internal static bool ConvertIdObjectToUdi(object id, out Udi guidId)
        {
            switch (id)
            {
                case string s:
                    return Udi.TryParse(s, out guidId);

                case Udi u:
                    guidId = u;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }


        #endregion

        #region Media

        public IPublishedContent Media(Udi id)
        {
            var guidUdi = id as GuidUdi;
            return guidUdi == null ? null : Media(guidUdi.Guid);
        }

        public IPublishedContent Media(Guid id)
        {
            //TODO: This is horrible but until the media cache properly supports GUIDs we have no choice here and
            // currently there won't be any way to add this method correctly to `ITypedPublishedContentQuery` without breaking an interface and adding GUID support for media

            var entityService = Current.Services.EntityService; // todo inject
            var mediaAttempt = entityService.GetId(id, UmbracoObjectTypes.Media);
            return mediaAttempt.Success ? ContentQuery.Media(mediaAttempt.Result) : null;
        }

        /// <summary>
        /// Overloaded method accepting an 'object' type
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass
        /// this result in to this method.
        /// This method will throw an exception if the value is not of type int or string.
        /// </remarks>
        public IPublishedContent Media(object id)
        {
            return MediaForObject(id);
        }

        private IPublishedContent MediaForObject(object id)
        {
            if (ConvertIdObjectToInt(id, out var intId))
                return ContentQuery.Media(intId);
            if (ConvertIdObjectToGuid(id, out var guidId))
                return ContentQuery.Media(guidId);
            if (ConvertIdObjectToUdi(id, out var udiId))
                return ContentQuery.Media(udiId);
            return null;
        }

        public IPublishedContent Media(int id)
        {
            return ContentQuery.Media(id);
        }

        public IPublishedContent Media(string id)
        {
            return MediaForObject(id);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params object[] ids)
        {
            return MediaForObjects(ids);
        }

        private IEnumerable<IPublishedContent> MediaForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            IEnumerable<int> intIds;
            if (ConvertIdsObjectToInts(idsA, out intIds))
                return ContentQuery.Media(intIds);
            //IEnumerable<Guid> guidIds;
            //if (ConvertIdsObjectToGuids(idsA, out guidIds))
            //    return ContentQuery.Media(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params int[] ids)
        {
            return ContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(params string[] ids)
        {
            return MediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<object> ids)
        {
            return MediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<int> ids)
        {
            return ContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> Media(IEnumerable<string> ids)
        {
            return MediaForObjects(ids);
        }

        public IEnumerable<IPublishedContent> MediaAtRoot()
        {
            return ContentQuery.MediaAtRoot();
        }

        #endregion

        #region Strings

        /// <summary>
        /// Replaces text line breaks with HTML line breaks
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with text line breaks replaced with HTML line breaks (<br/>)</returns>
        public IHtmlString ReplaceLineBreaksForHtml(string text)
        {
            return StringUtilities.ReplaceLineBreaksForHtml(text);
        }

        /// <summary>
        /// Generates a hash based on the text string passed in.  This method will detect the
        /// security requirements (is FIPS enabled) and return an appropriate hash.
        /// </summary>
        /// <param name="text">The text to create a hash from</param>
        /// <returns>Hash of the text string</returns>
        public string CreateHash(string text)
        {
            return text.GenerateHash();
        }

        /// <summary>
        /// Strips all HTML tags from a given string, all contents of the tags will remain.
        /// </summary>
        public HtmlString StripHtml(IHtmlString html, params string[] tags)
        {
            return StripHtml(html.ToHtmlString(), tags);
        }

        /// <summary>
        /// Strips all HTML tags from a given string, all contents of the tags will remain.
        /// </summary>
        public HtmlString StripHtml(string html, params string[] tags)
        {
            return StringUtilities.StripHtmlTags(html, tags);
        }

        /// <summary>
        /// Will take the first non-null value in the collection and return the value of it.
        /// </summary>
        public string Coalesce(params object[] args)
        {
            return StringUtilities.Coalesce(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string
        /// </summary>
        public string Concatenate(params object[] args)
        {
            return StringUtilities.Concatenate(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string and separates them with the string separator parameter.
        /// </summary>
        public string Join(string separator, params object[] args)
        {
            return StringUtilities.Join(separator, args);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length)
        {
            return Truncate(html.ToHtmlString(), length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length)
        {
            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length, bool addElipsis)
        {
            return Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return StringUtilities.Truncate(html, length, addElipsis, treatTagsAsContent);
        }

        #region Truncate by Words

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(string html, int words)
        {
            int length = StringUtilities.WordsToLength(html, words);

            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(string html, int words, bool addElipsis)
        {
            int length = StringUtilities.WordsToLength(html, words);

            return Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(IHtmlString html, int words)
        {
            int length = StringUtilities.WordsToLength(html.ToHtmlString(), words);

            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a ellipsis at the end (...). Method checks for open HTML tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(IHtmlString html, int words, bool addElipsis)
        {
            int length = StringUtilities.WordsToLength(html.ToHtmlString(), words);

            return Truncate(html, length, addElipsis, false);
        }

        #endregion

        #endregion

        #region If

        /// <summary>
        /// If the test is true, the string valueIfTrue will be returned, otherwise the valueIfFalse will be returned.
        /// </summary>
        public HtmlString If(bool test, string valueIfTrue, string valueIfFalse)
        {
            return test ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
        }

        /// <summary>
        /// If the test is true, the string valueIfTrue will be returned, otherwise the valueIfFalse will be returned.
        /// </summary>
        public HtmlString If(bool test, string valueIfTrue)
        {
            return test ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
        }

        #endregion

        /// <summary>
        /// This is used in methods like BeginUmbracoForm and SurfaceAction to generate an encrypted string which gets submitted in a request for which
        /// Umbraco can decrypt during the routing process in order to delegate the request to a specific MVC Controller.
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="controllerAction"></param>
        /// <param name="area"></param>
        /// <param name="additionalRouteVals"></param>
        /// <returns></returns>
        internal static string CreateEncryptedRouteString(string controllerName, string controllerAction, string area, object additionalRouteVals = null)
        {
            if (string.IsNullOrEmpty(controllerName)) throw new ArgumentNullOrEmptyException(nameof(controllerName));
            if (string.IsNullOrEmpty(controllerAction)) throw new ArgumentNullOrEmptyException(nameof(controllerAction));
            if (area == null) throw new ArgumentNullException(nameof(area));

            //need to create a params string as Base64 to put into our hidden field to use during the routes
            var surfaceRouteParams = $"c={HttpUtility.UrlEncode(controllerName)}&a={HttpUtility.UrlEncode(controllerAction)}&ar={area}";

            //checking if the additional route values is already a dictionary and convert to querystring
            string additionalRouteValsAsQuery;
            if (additionalRouteVals != null)
            {
                var additionalRouteValsAsDictionary = additionalRouteVals as Dictionary<string, object>;
                if (additionalRouteValsAsDictionary != null)
                    additionalRouteValsAsQuery = additionalRouteValsAsDictionary.ToQueryString();
                else
                    additionalRouteValsAsQuery = additionalRouteVals.ToDictionary<object>().ToQueryString();
            }
            else
                additionalRouteValsAsQuery = null;

            if (additionalRouteValsAsQuery.IsNullOrWhiteSpace() == false)
                surfaceRouteParams += "&" + additionalRouteValsAsQuery;

            return surfaceRouteParams.EncryptWithMachineKey();
        }
    }
}
