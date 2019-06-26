using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Mvc;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;

namespace Umbraco.Web
{
    /// <summary>
    /// A helper class that provides many useful methods and functionality for using Umbraco in templates
    /// </summary>
    public class UmbracoHelper : IUmbracoComponentRenderer
    {
        private readonly UmbracoContext _umbracoContext;
        private readonly IPublishedContent _currentPage;
        private readonly ITypedPublishedContentQuery _typedQuery;
        private readonly IDynamicPublishedContentQuery _dynamicQuery;
        private readonly HtmlStringUtilities _stringUtilities = new HtmlStringUtilities();

        private IUmbracoComponentRenderer _componentRenderer;
        private PublishedContentQuery _query;
        private MembershipHelper _membershipHelper;
        private TagQuery _tag;
        private IDataTypeService _dataTypeService;
        private IEntityService _entityService;
        private UrlProvider _urlProvider;
        private ICultureDictionary _cultureDictionary;

        /// <summary>
        /// Lazy instantiates the tag context
        /// </summary>
        public TagQuery TagQuery
        {
            //TODO: Unfortunately we cannot change this return value to be ITagQuery
            // since it's a breaking change, need to fix it for v8
            // http://issues.umbraco.org/issue/U4-6899

            get
            {
                return _tag ??
                       (_tag = new TagQuery(UmbracoContext.Application.Services.TagService,
                           _typedQuery ?? ContentQuery));
            }
        }

        /// <summary>
        /// Lazy instantiates the query context if not specified in the constructor
        /// </summary>
        public PublishedContentQuery ContentQuery
        {
            get
            {
                //If the content query doesn't exist it will either be created with the ITypedPublishedContentQuery, IDynamicPublishedContentQuery
                // used to construct this instance or with the content caches of the UmbracoContext
                return _query ??
                       (_query = _typedQuery != null
                           ? new PublishedContentQuery(_typedQuery, _dynamicQuery)
                           : new PublishedContentQuery(UmbracoContext.ContentCache, UmbracoContext.MediaCache));
            }
        }

        /// <summary>
        /// Helper method to ensure an umbraco context is set when it is needed
        /// </summary>
        public UmbracoContext UmbracoContext
        {
            get
            {
                if (_umbracoContext == null)
                {
                    throw new NullReferenceException("No " + typeof(UmbracoContext) + " reference has been set for this " + typeof(UmbracoHelper) + " instance");
                }
                return _umbracoContext;
            }
        }

        /// <summary>
        /// Lazy instantiates the membership helper if not specified in the constructor
        /// </summary>
        public MembershipHelper MembershipHelper
        {
            get { return _membershipHelper ?? (_membershipHelper = new MembershipHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Lazy instantiates the UrlProvider if not specified in the constructor
        /// </summary>
        public UrlProvider UrlProvider
        {
            get { return _urlProvider ?? (_urlProvider = UmbracoContext.UrlProvider); }
        }

        /// <summary>
        /// Lazy instantiates the IDataTypeService if not specified in the constructor
        /// </summary>
        public IDataTypeService DataTypeService
        {
            get { return _dataTypeService ?? (_dataTypeService = UmbracoContext.Application.Services.DataTypeService); }
        }

        /// <summary>
        /// Lazy instantiates the IEntityService
        /// </summary>
        private IEntityService EntityService
        {
            get { return _entityService ?? (_entityService = UmbracoContext.Application.Services.EntityService); }
        }

        /// <summary>
        /// Lazy instantiates the IUmbracoComponentRenderer if not specified in the constructor
        /// </summary>
        public IUmbracoComponentRenderer UmbracoComponentRenderer
        {
            get { return _componentRenderer ?? (_componentRenderer = new UmbracoComponentRenderer(UmbracoContext)); }
        }

        #region Constructors
        /// <summary>
        /// Empty constructor to create an umbraco helper for access to methods that don't have dependencies
        /// </summary>
        public UmbracoHelper()
        {
        }

        /// <summary>
        /// Constructor accepting all dependencies
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="content"></param>
        /// <param name="typedQuery"></param>
        /// <param name="dynamicQuery"></param>
        /// <param name="tagQuery"></param>
        /// <param name="dataTypeService"></param>
        /// <param name="urlProvider"></param>
        /// <param name="cultureDictionary"></param>
        /// <param name="componentRenderer"></param>
        /// <param name="membershipHelper"></param>
        /// <remarks>
        /// This constructor can be used to create a testable UmbracoHelper
        /// </remarks>
        public UmbracoHelper(UmbracoContext umbracoContext, IPublishedContent content,
            ITypedPublishedContentQuery typedQuery,
            IDynamicPublishedContentQuery dynamicQuery,
            ITagQuery tagQuery,
            IDataTypeService dataTypeService,
            UrlProvider urlProvider,
            ICultureDictionary cultureDictionary,
            IUmbracoComponentRenderer componentRenderer,
            MembershipHelper membershipHelper)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (content == null) throw new ArgumentNullException("content");
            if (typedQuery == null) throw new ArgumentNullException("typedQuery");
            if (dynamicQuery == null) throw new ArgumentNullException("dynamicQuery");
            if (tagQuery == null) throw new ArgumentNullException("tagQuery");
            if (dataTypeService == null) throw new ArgumentNullException("dataTypeService");
            if (urlProvider == null) throw new ArgumentNullException("urlProvider");
            if (cultureDictionary == null) throw new ArgumentNullException("cultureDictionary");
            if (componentRenderer == null) throw new ArgumentNullException("componentRenderer");
            if (membershipHelper == null) throw new ArgumentNullException("membershipHelper");

            _umbracoContext = umbracoContext;
            _tag = new TagQuery(tagQuery);
            _dataTypeService = dataTypeService;
            _urlProvider = urlProvider;
            _cultureDictionary = cultureDictionary;
            _componentRenderer = componentRenderer;
            _membershipHelper = membershipHelper;
            _currentPage = content;
            _typedQuery = typedQuery;
            _dynamicQuery = dynamicQuery;
        }

        [Obsolete("Use the constructor specifying all dependencies")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoHelper(UmbracoContext umbracoContext, IPublishedContent content, PublishedContentQuery query)
            : this(umbracoContext)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (query == null) throw new ArgumentNullException("query");
            _currentPage = content;
            _query = query;
        }

        /// <summary>
        /// Custom constructor setting the current page to the parameter passed in
        /// </summary>
        /// <param name="umbracoContext"></param>
        /// <param name="content"></param>
        public UmbracoHelper(UmbracoContext umbracoContext, IPublishedContent content)
            : this(umbracoContext)
        {
            if (content == null) throw new ArgumentNullException("content");
            _currentPage = content;
        }

        /// <summary>
        /// Standard constructor setting the current page to the page that has been routed to
        /// </summary>
        /// <param name="umbracoContext"></param>
        public UmbracoHelper(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (umbracoContext.RoutingContext == null) throw new NullReferenceException("The RoutingContext on the UmbracoContext cannot be null");

            _umbracoContext = umbracoContext;
            if (_umbracoContext.IsFrontEndUmbracoRequest)
            {
                _currentPage = _umbracoContext.PublishedContentRequest.PublishedContent;
            }
        }

        [Obsolete("Use the constructor specifying all dependencies")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public UmbracoHelper(UmbracoContext umbracoContext, PublishedContentQuery query)
            : this(umbracoContext)
        {
            if (query == null) throw new ArgumentNullException("query");
            _query = query;
        }
        #endregion

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

        #region Field

        /// <summary>
        /// Renders an field to the template
        /// </summary>
        /// <param name="fieldAlias"></param>
        /// <param name="altFieldAlias"></param>
        /// <param name="altText"></param>
        /// <param name="insertBefore"></param>
        /// <param name="insertAfter"></param>
        /// <param name="recursive"></param>
        /// <param name="convertLineBreaks"></param>
        /// <param name="removeParagraphTags"></param>
        /// <param name="casing"></param>
        /// <param name="encoding"></param>
        /// <param name="formatAsDate"></param>
        /// <param name="formatAsDateWithTime"></param>
        /// <param name="formatAsDateWithTimeSeparator"></param>
        //// <param name="formatString"></param>
        /// <returns></returns>
        public IHtmlString Field(string fieldAlias,
            string altFieldAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
            RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged,
            bool formatAsDate = false,
            bool formatAsDateWithTime = false,
            string formatAsDateWithTimeSeparator = "")
        {
            return UmbracoComponentRenderer.Field(AssignedContentItem, fieldAlias, altFieldAlias,
                altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
                casing, encoding, formatAsDate, formatAsDateWithTime, formatAsDateWithTimeSeparator);
        }

        /// <summary>
        /// Renders an field to the template
        /// </summary>
        /// <param name="currentPage"></param>
        /// <param name="fieldAlias"></param>
        /// <param name="altFieldAlias"></param>
        /// <param name="altText"></param>
        /// <param name="insertBefore"></param>
        /// <param name="insertAfter"></param>
        /// <param name="recursive"></param>
        /// <param name="convertLineBreaks"></param>
        /// <param name="removeParagraphTags"></param>
        /// <param name="casing"></param>
        /// <param name="encoding"></param>
        /// <param name="formatAsDate"></param>
        /// <param name="formatAsDateWithTime"></param>
        /// <param name="formatAsDateWithTimeSeparator"></param>
        //// <param name="formatString"></param>
        /// <returns></returns>
        public IHtmlString Field(IPublishedContent currentPage, string fieldAlias,
            string altFieldAlias = "", string altText = "", string insertBefore = "", string insertAfter = "",
            bool recursive = false, bool convertLineBreaks = false, bool removeParagraphTags = false,
            RenderFieldCaseType casing = RenderFieldCaseType.Unchanged,
            RenderFieldEncodingType encoding = RenderFieldEncodingType.Unchanged,
            bool formatAsDate = false,
            bool formatAsDateWithTime = false,
            string formatAsDateWithTimeSeparator = "")
        {
            return UmbracoComponentRenderer.Field(currentPage, fieldAlias, altFieldAlias,
                altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
                casing, encoding, formatAsDate, formatAsDateWithTime, formatAsDateWithTimeSeparator);
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
        public ICultureDictionary CultureDictionary
        {
            get
            {
                if (_cultureDictionary == null)
                {
                    var factory = CultureDictionaryFactoryResolver.Current.Factory;
                    _cultureDictionary = factory.CreateDictionary();
                }
                return _cultureDictionary;
            }
        }

        #endregion

        #region Membership

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the IsProtected method that only specifies path")]
        public bool IsProtected(int documentId, string path)
        {
            return IsProtected(path.EnsureEndsWith("," + documentId));
        }

        /// <summary>
        /// Check if a document object is protected by the "Protect Pages" functionality in umbraco
        /// </summary>
        /// <param name="path">The full path of the document object to check</param>
        /// <returns>True if the document object is protected</returns>
        public bool IsProtected(string path)
        {
            return MembershipHelper.IsProtected(path);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use the MemberHasAccess method that only specifies path")]
        public bool MemberHasAccess(int nodeId, string path)
        {
            return MemberHasAccess(path.EnsureEndsWith("," + nodeId));
        }

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

        #region NiceUrls

        /// <summary>
        /// Returns a string with a friendly url from a node.
        /// IE.: Instead of having /482 (id) as an url, you can have
        /// /screenshots/developer/macros (spoken url)
        /// </summary>
        /// <param name="nodeId">Identifier for the node that should be returned</param>
        /// <returns>String with a friendly url from a node</returns>
        public string NiceUrl(int nodeId)
        {
            return Url(nodeId);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId)
        {
            return UrlProvider.GetUrl(contentId);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The url for the content.</returns>
        public string Url(int contentId, UrlProviderMode mode)
        {
            return UrlProvider.GetUrl(contentId, mode);
        }

        /// <summary>
        /// This method will always add the domain to the path if the hostnames are set up correctly.
        /// </summary>
        /// <param name="nodeId">Identifier for the node that should be returned</param>
        /// <returns>String with a friendly url with full domain from a node</returns>
        public string NiceUrlWithDomain(int nodeId)
        {
            return UrlAbsolute(nodeId);
        }

        /// <summary>
        /// Gets the absolute url of a content identified by its identifier.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <returns>The absolute url for the content.</returns>
        public string UrlAbsolute(int contentId)
        {
            return UrlProvider.GetUrl(contentId, true);
        }

        #endregion

        #region Members

        public IPublishedContent TypedMember(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null) return null;
            return TypedMember(guidUdi.Guid);
        }

        public IPublishedContent TypedMember(Guid id)
        {
            return MembershipHelper.GetByProviderKey(id);
        }

        public IPublishedContent TypedMember(object id)
        {
            int intId;
            if (ConvertIdObjectToInt(id, out intId))
                return MembershipHelper.GetById(intId);
            Guid guidId;
            if (ConvertIdObjectToGuid(id, out guidId))
                return TypedMember(guidId);
            Udi udiId;
            if (ConvertIdObjectToUdi(id, out udiId))
                return TypedMember(udiId);
            return null;
        }

        public IPublishedContent TypedMember(int id)
        {
            return MembershipHelper.GetById(id);
        }

        public IPublishedContent TypedMember(string id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt ? MembershipHelper.GetById(asInt.Result) : MembershipHelper.GetByProviderKey(id);
        }

        public dynamic Member(object id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt
                ? MembershipHelper.GetById(asInt.Result).AsDynamic()
                : MembershipHelper.GetByProviderKey(id).AsDynamic();
        }

        public dynamic Member(int id)
        {
            return MembershipHelper.GetById(id).AsDynamic();
        }

        public dynamic Member(string id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt
                ? MembershipHelper.GetById(asInt.Result).AsDynamic()
                : MembershipHelper.GetByProviderKey(id).AsDynamic();
        }

        #endregion

        #region Content

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent TypedContent(object id)
        {
            return TypedContentForObject(id);
        }

        private IPublishedContent TypedContentForObject(object id)
        {
            int intId;
            if (ConvertIdObjectToInt(id, out intId))
                return ContentQuery.TypedContent(intId);
            Guid guidId;
            if (ConvertIdObjectToGuid(id, out guidId))
                return ContentQuery.TypedContent(guidId);
            Udi udiId;
            if (ConvertIdObjectToUdi(id, out udiId))
                return ContentQuery.TypedContent(udiId);
            return null;
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent TypedContent(int id)
        {
            return ContentQuery.TypedContent(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The key of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent TypedContent(Guid id)
        {
            return ContentQuery.TypedContent(id);
        }

        /// <summary>
        /// Gets a content item from the cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IPublishedContent TypedContent(Udi id)
        {
            return ContentQuery.TypedContent(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or null of the content item is not in the cache.</returns>
        public IPublishedContent TypedContent(string id)
        {
            return TypedContentForObject(id);
        }

        public IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.TypedContentSingleAtXPath(xpath, vars);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> TypedContent(params object[] ids)
        {
            return TypedContentForObjects(ids);
        }   



        /// <summary>
        /// Gets the content corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache</returns>
        public IEnumerable<IPublishedContent> TypedContent(params Udi[] ids)
        {
            return TypedContentForObjects(ids);
        }


        private IEnumerable<IPublishedContent> TypedContentForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            IEnumerable<int> intIds;
            if (ConvertIdsObjectToInts(idsA, out intIds))
                return ContentQuery.TypedContent(intIds);
            IEnumerable<Guid> guidIds;
            if (ConvertIdsObjectToGuids(idsA, out guidIds))
                return ContentQuery.TypedContent(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> TypedContent(params int[] ids)
        {
            return ContentQuery.TypedContent(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The keys of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public IEnumerable<IPublishedContent> TypedContent(params Guid[] ids)
        {
            return ContentQuery.TypedContent(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public IEnumerable<IPublishedContent> TypedContent(params string[] ids)
        {
            return TypedContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<object> ids)
        {
            return TypedContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<string> ids)
        {
            return TypedContentForObjects(ids);
        }

        /// <summary>
        /// Gets the contents corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The content identifiers.</param>
        /// <returns>The existing contents corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing content, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids)
        {
            return ContentQuery.TypedContent(ids);
        }


        /// <summary>
        /// Gets the content corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache</returns>
        public IEnumerable<IPublishedContent> TypedContent(IEnumerable<Udi> ids)
        {
            return TypedContentForObjects(ids);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.TypedContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ContentQuery.TypedContentAtXPath(xpath, vars);
        }

        public IEnumerable<IPublishedContent> TypedContentAtRoot()
        {
            return ContentQuery.TypedContentAtRoot();
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or DynamicNull of the content item is not in the cache.</returns>
        public dynamic Content(object id)
        {
            return ContentForObject(id);
        }

        private dynamic ContentForObject(object id)
        {
            int intId;
            if (ConvertIdObjectToInt(id, out intId))
                return ContentQuery.Content(intId);
            Guid guidId;
            if (ConvertIdObjectToGuid(id, out guidId))
                return ContentQuery.Content(guidId);
            return DynamicNull.Null;
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier of the content item.</param>
        /// <returns>The content, or DynamicNull of the content item is not in the cache.</returns>
        public dynamic Content(int id)
        {
            return ContentQuery.Content(id);
        }

        /// <summary>
        /// Gets a content item from the cache.
        /// </summary>
        /// <param name="id">The unique identifier, or the key, of the content item.</param>
        /// <returns>The content, or DynamicNull of the content item is not in the cache.</returns>
        public dynamic Content(string id)
        {
            return ContentForObject(id);
        }

        public dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        public dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public dynamic Content(params object[] ids)
        {
            return ContentForObjects(ids);
        }

        private dynamic ContentForObjects(IEnumerable<object> ids)
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
        public dynamic Content(params int[] ids)
        {
            return ContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public dynamic Content(params string[] ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public dynamic Content(IEnumerable<object> ids)
        {
            return ContentForObjects(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        public dynamic Content(IEnumerable<int> ids)
        {
            return ContentQuery.Content(ids);
        }

        /// <summary>
        /// Gets content items from the cache.
        /// </summary>
        /// <param name="ids">The unique identifiers, or the keys, of the content items.</param>
        /// <returns>The content items that were found in the cache.</returns>
        /// <remarks>Does not support mixing identifiers and keys.</remarks>
        public dynamic Content(IEnumerable<string> ids)
        {
            return ContentForObjects(ids);
        }

        public dynamic ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentAtXPath(xpath, vars);
        }

        public dynamic ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentAtXPath(xpath, vars);
        }

        public dynamic ContentAtRoot()
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

                case Udi u:
                    guidId = new GuidUdi(u.UriValue).Guid;
                    return true;

                default:
                    guidId = default;
                    return false;
            }
        }

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

        private static bool ConvertIdsObjectToInts(IEnumerable<object> ids, out IEnumerable<int> intIds)
        {
            var list = new List<int>();
            intIds = null;
            foreach (var id in ids)
            {
                int intId;
                if (ConvertIdObjectToInt(id, out intId))
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

        #endregion

        #region Media

        public IPublishedContent TypedMedia(Udi id)
        {
            var guidUdi = id as GuidUdi;
            if (guidUdi == null) return null;
            return TypedMedia(guidUdi.Guid);
        }

        public IPublishedContent TypedMedia(Guid id)
        {
            //TODO: This is horrible but until the media cache properly supports GUIDs we have no choice here and 
            // currently there won't be any way to add this method correctly to `ITypedPublishedContentQuery` without breaking an interface and adding GUID support for media

            var entityService = UmbracoContext.Application.Services.EntityService;
            var mediaAttempt = entityService.GetIdForKey(id, UmbracoObjectTypes.Media);
            return mediaAttempt.Success ? ContentQuery.TypedMedia(mediaAttempt.Result) : null;
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
        public IPublishedContent TypedMedia(object id)
        {
            return TypedMediaForObject(id);
        }

        private IPublishedContent TypedMediaForObject(object id)
        {
            int intId;
            if (ConvertIdObjectToInt(id, out intId))
                return ContentQuery.TypedMedia(intId);
            Guid guidId;
            if (ConvertIdObjectToGuid(id, out guidId))
                return TypedMedia(guidId);
            Udi udiId;
            if (ConvertIdObjectToUdi(id, out udiId))
                return TypedMedia(udiId);
            return null;
        }

        public IPublishedContent TypedMedia(int id)
        {
            return ContentQuery.TypedMedia(id);
        }

        /// <summary>
        /// Returns typed Media content based on an Identifier
        /// </summary>
        /// <param name="id">The id - this can be the numeric Id such as '1234' or a UDI string such as 'umb://media/a1276990a50e4784b25458fc8d0c487c'</param>
        /// <returns>PublishedContent if a corresponding media Id exists; otherwise null</returns>
        public IPublishedContent TypedMedia(string id)
        {
            return TypedMediaForObject(id);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(params object[] ids)
        {
            return TypedMediaForObjects(ids);
        }

        private IEnumerable<IPublishedContent> TypedMediaForObjects(IEnumerable<object> ids)
        {
            var idsA = ids.ToArray();
            IEnumerable<int> intIds;
            if (ConvertIdsObjectToInts(idsA, out intIds))
                return ContentQuery.TypedMedia(intIds);
            //IEnumerable<Guid> guidIds;
            //if (ConvertIdsObjectToGuids(idsA, out guidIds))
            //    return ContentQuery.TypedMedia(guidIds);
            return Enumerable.Empty<IPublishedContent>();
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(params int[] ids)
        {
            return ContentQuery.TypedMedia(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(params string[] ids)
        {
            return TypedMediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<object> ids)
        {
            return TypedMediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids)
        {
            return ContentQuery.TypedMedia(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<string> ids)
        {
            return TypedMediaForObjects(ids);
        }

        public IEnumerable<IPublishedContent> TypedMediaAtRoot()
        {
            return ContentQuery.TypedMediaAtRoot();
        }

        public dynamic Media(object id)
        {
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.Media(intId) : DynamicNull.Null;
        }

        public dynamic Media(int id)
        {
            return ContentQuery.Media(id);
        }

        public dynamic Media(string id)
        {
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.Media(intId) : DynamicNull.Null;
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public dynamic Media(params object[] ids)
        {
            return MediaForObjects(ids);
        }

        private dynamic MediaForObjects(IEnumerable<object> ids)
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
        public dynamic Media(params int[] ids)
        {
            return ContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public dynamic Media(params string[] ids)
        {
            return MediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public dynamic Media(IEnumerable<object> ids)
        {
            return MediaForObjects(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public dynamic Media(IEnumerable<int> ids)
        {
            return ContentQuery.Media(ids);
        }

        /// <summary>
        /// Gets the medias corresponding to the identifiers.
        /// </summary>
        /// <param name="ids">The media identifiers.</param>
        /// <returns>The existing medias corresponding to the identifiers.</returns>
        /// <remarks>If an identifier does not match an existing media, it will be missing in the returned value.</remarks>
        public dynamic Media(IEnumerable<string> ids)
        {
            return MediaForObjects(ids);
        }

        public dynamic MediaAtRoot()
        {
            return ContentQuery.MediaAtRoot();
        }

        #endregion

        #region Search

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public dynamic Search(string term, bool useWildCards = true, string searchProvider = null)
        {
            return ContentQuery.Search(term, useWildCards, searchProvider);
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return ContentQuery.Search(criteria, searchProvider);
        }

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(string term, bool useWildCards = true, string searchProvider = null)
        {
            return ContentQuery.TypedSearch(term, useWildCards, searchProvider);
        }

        /// <summary>
        /// Searches content
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="totalRecords"></param>
        /// <param name="term"></param>
        /// <param name="useWildCards"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(int skip, int take, out int totalRecords, string term, bool useWildCards = true, string searchProvider = null)
        {
            return ContentQuery.TypedSearch(skip, take, out totalRecords, term, useWildCards, searchProvider);
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="skip"></param>
        /// <param name="take"></param>
        /// <param name="totalRecords"></param>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(int skip, int take, out int totalRecords, Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return ContentQuery.TypedSearch(skip, take, out totalRecords, criteria, searchProvider);
        }

        /// <summary>
        /// Searhes content
        /// </summary>
        /// <param name="criteria"></param>
        /// <param name="searchProvider"></param>
        /// <returns></returns>
        public IEnumerable<IPublishedContent> TypedSearch(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
        {
            return ContentQuery.TypedSearch(criteria, searchProvider);
        }

        #endregion

        #region Xml

        public dynamic ToDynamicXml(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml)) return null;
            var xElement = XElement.Parse(xml);
            return new DynamicXml(xElement);
        }

        public dynamic ToDynamicXml(XElement xElement)
        {
            return new DynamicXml(xElement);
        }

        public dynamic ToDynamicXml(XPathNodeIterator xpni)
        {
            return new DynamicXml(xpni);
        }

        #endregion

        #region Strings

        /// <summary>
        /// Replaces text line breaks with html line breaks
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The text with text line breaks replaced with html linebreaks (<br/>)</returns>
        public string ReplaceLineBreaksForHtml(string text)
        {
            return _stringUtilities.ReplaceLineBreaksForHtml(text);
        }

        /// <summary>
        /// Returns an MD5 hash of the string specified
        /// </summary>
        /// <param name="text">The text to create a hash from</param>
        /// <returns>Md5 hash of the string</returns>
        [Obsolete("Please use the CreateHash method instead. This may be removed in future versions")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string CreateMd5Hash(string text)
        {
            return text.ToMd5();
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
        /// Strips all html tags from a given string, all contents of the tags will remain.
        /// </summary>
        public HtmlString StripHtml(IHtmlString html, params string[] tags)
        {
            return StripHtml(html.ToHtmlString(), tags);
        }

        /// <summary>
        /// Strips all html tags from a given string, all contents of the tags will remain.
        /// </summary>
        public HtmlString StripHtml(DynamicNull html, params string[] tags)
        {
            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Strips all html tags from a given string, all contents of the tags will remain.
        /// </summary>
        public HtmlString StripHtml(string html, params string[] tags)
        {
            return _stringUtilities.StripHtmlTags(html, tags);
        }

        /// <summary>
        /// Will take the first non-null value in the collection and return the value of it.
        /// </summary>
        public string Coalesce(params object[] args)
        {
            return _stringUtilities.Coalesce<DynamicNull>(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string
        /// </summary>
        public string Concatenate(params object[] args)
        {
            return _stringUtilities.Concatenate<DynamicNull>(args);
        }

        /// <summary>
        /// Joins any number of int/string/objects into one string and seperates them with the string seperator parameter.
        /// </summary>
        public string Join(string seperator, params object[] args)
        {
            return _stringUtilities.Join<DynamicNull>(seperator, args);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length)
        {
            return Truncate(html.ToHtmlString(), length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(DynamicNull html, int length)
        {
            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis)
        {
            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return new HtmlString(string.Empty);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length)
        {
            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length, bool addElipsis)
        {
            return Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given length, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
        {
            return _stringUtilities.Truncate(html, length, addElipsis, treatTagsAsContent);
        }
        #region Truncate by Words

        /// <summary>
        /// Truncates a string to a given amount of words, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(string html, int words)
        {
            int length = _stringUtilities.WordsToLength(html, words);

            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(string html, int words, bool addElipsis)
        {
            int length = _stringUtilities.WordsToLength(html, words);

            return Truncate(html, length, addElipsis, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(IHtmlString html, int words)
        {
            int length = _stringUtilities.WordsToLength(html.ToHtmlString(), words);

            return Truncate(html, length, true, false);
        }

        /// <summary>
        /// Truncates a string to a given amount of words, can add a elipsis at the end (...). Method checks for open html tags, and makes sure to close them
        /// </summary>
        public IHtmlString TruncateByWords(IHtmlString html, int words, bool addElipsis)
        {
            int length = _stringUtilities.WordsToLength(html.ToHtmlString(), words);

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

        #region Prevalues

        /// <summary>
        /// Gets a specific PreValue by its Id
        /// </summary>
        /// <param name="id">Id of the PreValue to retrieve the value from</param>
        /// <returns>PreValue as a string</returns>
        public string GetPreValueAsString(int id)
        {
            return DataTypeService.GetPreValueAsString(id);
        }

        #endregion

        #region canvasdesigner

        [Obsolete("Use EnableCanvasDesigner on the HtmlHelper extensions instead")]
        public IHtmlString EnableCanvasDesigner()
        {
            return EnableCanvasDesigner(string.Empty, string.Empty);
        }

        [Obsolete("Use EnableCanvasDesigner on the HtmlHelper extensions instead")]
        public IHtmlString EnableCanvasDesigner(string canvasdesignerConfigPath)
        {
            return EnableCanvasDesigner(canvasdesignerConfigPath, string.Empty);
        }

        [Obsolete("Use EnableCanvasDesigner on the HtmlHelper extensions instead")]
        public IHtmlString EnableCanvasDesigner(string canvasdesignerConfigPath, string canvasdesignerPalettesPath)
        {
            var html = CreateHtmlHelper("");
            var urlHelper = new UrlHelper(UmbracoContext.HttpContext.Request.RequestContext);
            return html.EnableCanvasDesigner(urlHelper, UmbracoContext, canvasdesignerConfigPath, canvasdesignerPalettesPath);
        }

        [Obsolete("This shouldn't need to be used but because the obsolete extension methods above don't have access to the current HtmlHelper, we need to create a fake one, unfortunately however this will not pertain the current views viewdata, tempdata or model state so should not be used")]
        private HtmlHelper CreateHtmlHelper(object model)
        {
            var cc = new ControllerContext
            {
                RequestContext = UmbracoContext.HttpContext.Request.RequestContext
            };
            var viewContext = new ViewContext(cc, new FakeView(), new ViewDataDictionary(model), new TempDataDictionary(), new StringWriter());
            var htmlHelper = new HtmlHelper(viewContext, new ViewPage());
            return htmlHelper;
        }

        [Obsolete("This shouldn't need to be used but because the obsolete extension methods above don't have access to the current HtmlHelper, we need to create a fake one, unfortunately however this will not pertain the current views viewdata, tempdata or model state so should not be used")]
        private class FakeView : IView
        {
            public void Render(ViewContext viewContext, TextWriter writer)
            {
            }
        }

        #endregion

        internal static bool DecryptAndValidateEncryptedRouteString(string ufprt, out IDictionary<string, string> parts)
        {
            string decryptedString;
            try
            {
                decryptedString = ufprt.DecryptWithMachineKey();
            }
            catch (FormatException)
            {
                LogHelper.Warn<UmbracoHelper>("A value was detected in the ufprt parameter but Umbraco could not decrypt the string");
                parts = null;
                return false;
            }

            var parsedQueryString = HttpUtility.ParseQueryString(decryptedString);
            parts = new Dictionary<string, string>();

            foreach (var key in parsedQueryString.AllKeys)
            {
                parts[key] = parsedQueryString[key];
            }

            //validate all required keys exist

            //the controller
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Controller))
                return false;
            //the action
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Action))
                return false;
            //the area
            if (parts.All(x => x.Key != RenderRouteHandler.ReservedAdditionalKeys.Area))
                return false;

            return true;
        }

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
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNullOrEmpty(controllerAction, "controllerAction");
            Mandate.ParameterNotNull(area, "area");

            //need to create a params string as Base64 to put into our hidden field to use during the routes
            var surfaceRouteParams = $"{RenderRouteHandler.ReservedAdditionalKeys.Controller}={HttpUtility.UrlEncode(controllerName)}&{RenderRouteHandler.ReservedAdditionalKeys.Action}={HttpUtility.UrlEncode(controllerAction)}&{RenderRouteHandler.ReservedAdditionalKeys.Area}={area}";

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

        public int GetIdForUdi(Udi udi)
        {
            var udiToIdAttempt = EntityService.GetIdForUdi(udi);
            return udiToIdAttempt.Success ? udiToIdAttempt.Result : -1;
        }
    }
}
