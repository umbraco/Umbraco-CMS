using System;
using System.ComponentModel;
using System.Web;
using System.Web.Security;
using System.Xml.Linq;
using System.Xml.XPath;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using System.Collections.Generic;
using Umbraco.Core.Cache;

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
        private ITagQuery _tag;
        private IDataTypeService _dataTypeService;
        private UrlProvider _urlProvider;
        private ICultureDictionary _cultureDictionary;

        /// <summary>
        /// Lazy instantiates the tag context
        /// </summary>
        public ITagQuery TagQuery
        {
            get { return _tag ?? (_tag = new TagQuery(UmbracoContext.Application.Services.TagService, _typedQuery)); }
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
            _tag = tagQuery;
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
        /// Returns the current IPublishedContent item assigned to the UmbracoHelper
        /// </summary>
        /// <remarks>
        /// Note that this is the assigned IPublishedContent item to the UmbracoHelper, this is not necessarily the Current IPublishedContent item
        /// being rendered. This IPublishedContent object is contextual to the current UmbracoHelper instance.
        /// 
        /// In some cases accessing this property will throw an exception if there is not IPublishedContent assigned to the Helper
        /// this will only ever happen if the Helper is constructed with an UmbracoContext and it is not a front-end request
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown if the UmbracoHelper is constructed with an UmbracoContext and it is not a front-end request</exception>
	    public IPublishedContent AssignedContentItem
	    {
	        get
	        {
	            if (_currentPage == null)
                    throw new InvalidOperationException("Cannot return the " + typeof(IPublishedContent).Name + " because the " + typeof(UmbracoHelper).Name + " was constructed with an " + typeof(UmbracoContext).Name + " and the current request is not a front-end request.");
                
                return _currentPage;
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
            return _componentRenderer.Field(AssignedContentItem, fieldAlias, altFieldAlias,
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
            bool formatAsDate =  false,
            bool formatAsDateWithTime = false,
            string formatAsDateWithTimeSeparator = "")
		{
            return _componentRenderer.Field(currentPage, fieldAlias, altFieldAlias,
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
            return UmbracoContext.Application.Services.PublicAccessService.IsProtected(path);
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
            if (IsProtected(path))
            {
                return MembershipHelper.IsLoggedIn()
                       && UmbracoContext.Application.Services.PublicAccessService.HasAccess(path, GetCurrentMember(), Roles.Provider);
            }
            return true;
        }

        /// <summary>
        /// Gets (or adds) the current member from the current request cache
        /// </summary>
        private MembershipUser GetCurrentMember()
        {
            return UmbracoContext.Application.ApplicationCache.RequestCache
                .GetCacheItem<MembershipUser>("UmbracoHelper.GetCurrentMember", () =>
                {
                    var provider = Core.Security.MembershipProviderExtensions.GetMembersMembershipProvider();
                    return provider.GetCurrentUser();
                });
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

        public IPublishedContent TypedMember(object id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt ? MembershipHelper.GetById(asInt.Result) : MembershipHelper.GetByProviderKey(id);
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

        public IPublishedContent TypedContent(object id)
		{
		    int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.TypedContent(intId) : null;
		}

		public IPublishedContent TypedContent(int id)
		{
            return ContentQuery.TypedContent(id);
		}

		public IPublishedContent TypedContent(string id)
		{
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.TypedContent(intId) : null;
		}

        public IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.TypedContentSingleAtXPath(xpath, vars);
        }

		public IEnumerable<IPublishedContent> TypedContent(params object[] ids)
		{
            return ContentQuery.TypedContent(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedContent(params int[] ids)
		{
            return ContentQuery.TypedContent(ids);
		}

		public IEnumerable<IPublishedContent> TypedContent(params string[] ids)
		{
            return ContentQuery.TypedContent(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<object> ids)
		{
            return ContentQuery.TypedContent(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<string> ids)
		{
            return ContentQuery.TypedContent(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids)
		{
            return ContentQuery.TypedContent(ids);
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

		public dynamic Content(object id)
		{
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.Content(intId) : DynamicNull.Null;
		}

		public dynamic Content(int id)
		{
            return ContentQuery.Content(id);
		}

		public dynamic Content(string id)
		{
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.Content(intId) : DynamicNull.Null;
		}

        public dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        public dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return ContentQuery.ContentSingleAtXPath(xpath, vars);
        }

        public dynamic Content(params object[] ids)
		{
            return ContentQuery.Content(ConvertIdsObjectToInts(ids));
		}

		public dynamic Content(params int[] ids)
		{
            return ContentQuery.Content(ids);
		}

		public dynamic Content(params string[] ids)
		{
            return ContentQuery.Content(ConvertIdsObjectToInts(ids));
		}

		public dynamic Content(IEnumerable<object> ids)
		{
            return ContentQuery.Content(ConvertIdsObjectToInts(ids));
		}

		public dynamic Content(IEnumerable<int> ids)
		{
            return ContentQuery.Content(ids);
		}

		public dynamic Content(IEnumerable<string> ids)
		{
            return ContentQuery.Content(ConvertIdsObjectToInts(ids));
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

        private bool ConvertIdObjectToInt(object id, out int intId)
        {
            var s = id as string;
            if (s != null)
            {
                return int.TryParse(s, out intId);
            }
                
            if (id is int)
            {
                intId = (int) id;
                return true;
            }

            throw new InvalidOperationException("The value of parameter 'id' must be either a string or an integer");
        }

        private IEnumerable<int> ConvertIdsObjectToInts(IEnumerable<object> ids)
        {
            var list = new List<int>();
            foreach (var id in ids)
            {
                int intId;
                if (ConvertIdObjectToInt(id, out intId))
                {
                    list.Add(intId);
                }
            }
            return list;
        }

		#endregion

		#region Media

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
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.TypedMedia(intId) : null;
		}

		public IPublishedContent TypedMedia(int id)
		{
            return ContentQuery.TypedMedia(id);
		}

		public IPublishedContent TypedMedia(string id)
		{
            int intId;
            return ConvertIdObjectToInt(id, out intId) ? ContentQuery.TypedMedia(intId) : null;
		}

		public IEnumerable<IPublishedContent> TypedMedia(params object[] ids)
		{
            return ContentQuery.TypedMedia(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedMedia(params int[] ids)
		{
            return ContentQuery.TypedMedia(ids);
		}

		public IEnumerable<IPublishedContent> TypedMedia(params string[] ids)
		{
            return ContentQuery.TypedMedia(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<object> ids)
		{
            return ContentQuery.TypedMedia(ConvertIdsObjectToInts(ids));
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids)
		{
            return ContentQuery.TypedMedia(ids);
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<string> ids)
		{
            return ContentQuery.TypedMedia(ConvertIdsObjectToInts(ids));
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

		public dynamic Media(params object[] ids)
		{
            return ContentQuery.Media(ConvertIdsObjectToInts(ids));
		}

		public dynamic Media(params int[] ids)
		{
            return ContentQuery.Media(ids);
		}

		public dynamic Media(params string[] ids)
		{
            return ContentQuery.Media(ConvertIdsObjectToInts(ids));
		}

		public dynamic Media(IEnumerable<object> ids)
		{
            return ContentQuery.Media(ConvertIdsObjectToInts(ids));
		}

		public dynamic Media(IEnumerable<int> ids)
		{
            return ContentQuery.Media(ids);
		}

		public dynamic Media(IEnumerable<string> ids)
		{
            return ContentQuery.Media(ConvertIdsObjectToInts(ids));
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
		/// <returns>Md5 has of the string</returns>
		public string CreateMd5Hash(string text)
		{
			return text.ToMd5();
		}

		public HtmlString StripHtml(IHtmlString html, params string[] tags)
		{
			return StripHtml(html.ToHtmlString(), tags);
		}
		public HtmlString StripHtml(DynamicNull html, params string[] tags)
		{
			return new HtmlString(string.Empty);
		}
		public HtmlString StripHtml(string html, params string[] tags)
		{
            return _stringUtilities.StripHtmlTags(html, tags);
		}
        
		public string Coalesce(params object[] args)
		{
            return _stringUtilities.Coalesce<DynamicNull>(args);
		}

		public string Concatenate(params object[] args)
		{
            return _stringUtilities.Concatenate<DynamicNull>(args);
		}

		public string Join(string seperator, params object[] args)
		{
            return _stringUtilities.Join<DynamicNull>(seperator, args);
		}

		public IHtmlString Truncate(IHtmlString html, int length)
		{
			return Truncate(html.ToHtmlString(), length, true, false);
		}
		public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis)
		{
			return Truncate(html.ToHtmlString(), length, addElipsis, false);
		}
		public IHtmlString Truncate(IHtmlString html, int length, bool addElipsis, bool treatTagsAsContent)
		{
			return Truncate(html.ToHtmlString(), length, addElipsis, treatTagsAsContent);
		}
		public IHtmlString Truncate(DynamicNull html, int length)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(DynamicNull html, int length, bool addElipsis, bool treatTagsAsContent)
		{
			return new HtmlString(string.Empty);
		}
		public IHtmlString Truncate(string html, int length)
		{
			return Truncate(html, length, true, false);
		}
		public IHtmlString Truncate(string html, int length, bool addElipsis)
		{
			return Truncate(html, length, addElipsis, false);
		}
		public IHtmlString Truncate(string html, int length, bool addElipsis, bool treatTagsAsContent)
		{
            return _stringUtilities.Truncate(html, length, addElipsis, treatTagsAsContent);
		}


		#endregion

		#region If

		public HtmlString If(bool test, string valueIfTrue, string valueIfFalse)
		{
			return test ? new HtmlString(valueIfTrue) : new HtmlString(valueIfFalse);
		}
		public HtmlString If(bool test, string valueIfTrue)
		{
			return test ? new HtmlString(valueIfTrue) : new HtmlString(string.Empty);
		}

		#endregion

        #region Prevalues

        public string GetPreValueAsString(int id)
        {
            return DataTypeService.GetPreValueAsString(id);
        }

        #endregion

        #region canvasdesigner
        
        public HtmlString EnableCanvasDesigner()
        {
            return EnableCanvasDesigner(string.Empty, string.Empty);
        }

        public HtmlString EnableCanvasDesigner(string canvasdesignerConfigPath)
        {
            return EnableCanvasDesigner(canvasdesignerConfigPath, string.Empty);
        }

        public HtmlString EnableCanvasDesigner(string canvasdesignerConfigPath, string canvasdesignerPalettesPath)
        {

            string previewLink = @"<script src=""/Umbraco/lib/jquery/jquery.min.js"" type=""text/javascript""></script>" +
                                 @"<script src=""{0}"" type=""text/javascript""></script>" +
                                 @"<script src=""{1}"" type=""text/javascript""></script>" +
                                 @"<script type=""text/javascript"">var pageId = '{2}'</script>" +
                                 @"<script src=""/umbraco/js/canvasdesigner.front.js"" type=""text/javascript""></script>";

            string noPreviewLinks = @"<link href=""{0}"" type=""text/css"" rel=""stylesheet"" data-title=""canvasdesignerCss"" />";

            // Get page value
            int pageId = UmbracoContext.PublishedContentRequest.UmbracoPage.PageID;
            string[] path = UmbracoContext.PublishedContentRequest.UmbracoPage.SplitPath;
            string result = string.Empty;
            string cssPath = CanvasDesignerUtility.GetStylesheetPath(path, false);

            if (UmbracoContext.Current.InPreviewMode)
            {
                canvasdesignerConfigPath = !string.IsNullOrEmpty(canvasdesignerConfigPath) ? canvasdesignerConfigPath : "/umbraco/js/canvasdesigner.config.js";
                canvasdesignerPalettesPath = !string.IsNullOrEmpty(canvasdesignerPalettesPath) ? canvasdesignerPalettesPath : "/umbraco/js/canvasdesigner.palettes.js";
                
                if (!string.IsNullOrEmpty(cssPath))
                    result = string.Format(noPreviewLinks, cssPath) + Environment.NewLine;

                result = result + string.Format(previewLink, canvasdesignerConfigPath, canvasdesignerPalettesPath, pageId);
            }
            else
            {
                // Get css path for current page
                if (!string.IsNullOrEmpty(cssPath))
                    result = string.Format(noPreviewLinks, cssPath);
            }

            return new HtmlString(result);

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
            Mandate.ParameterNotNullOrEmpty(controllerName, "controllerName");
            Mandate.ParameterNotNullOrEmpty(controllerAction, "controllerAction");
            Mandate.ParameterNotNull(area, "area");

            //need to create a params string as Base64 to put into our hidden field to use during the routes
            var surfaceRouteParams = string.Format("c={0}&a={1}&ar={2}",
                                                      HttpUtility.UrlEncode(controllerName),
                                                      HttpUtility.UrlEncode(controllerAction),
                                                      area);

            var additionalRouteValsAsQuery = additionalRouteVals != null ? additionalRouteVals.ToDictionary<object>().ToQueryString() : null;

            if (additionalRouteValsAsQuery.IsNullOrWhiteSpace() == false)
                surfaceRouteParams += "&" + additionalRouteValsAsQuery;

            return surfaceRouteParams.EncryptWithMachineKey();
        }

	}
}
