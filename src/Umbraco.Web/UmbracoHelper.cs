using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Xml.Linq;
using System.Xml.XPath;
using HtmlAgilityPack;
using Umbraco.Core;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Core.Security;
using Umbraco.Core.Services;
using Umbraco.Core.Xml;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.Templates;
using umbraco;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.templateControls;
using Umbraco.Core.Cache;
using AttributeCollection = System.Web.UI.AttributeCollection;

namespace Umbraco.Web
{

    /// <summary>
	/// A helper class that provides many useful methods and functionality for using Umbraco in templates
	/// </summary>
	public class UmbracoHelper
	{
		private readonly UmbracoContext _umbracoContext;
		private readonly IPublishedContent _currentPage;
	    private PublishedContentQuery _query;
        private readonly MembershipHelper _membershipHelper;
        private TagQuery _tag;

        /// <summary>
        /// Lazy instantiates the tag context
        /// </summary>
        public TagQuery TagQuery
        {
            get { return _tag ?? (_tag = new TagQuery(UmbracoContext.Application.Services.TagService, ContentQuery)); }
        }

        /// <summary>
        /// Lazy instantiates the query context
        /// </summary>
	    public PublishedContentQuery ContentQuery
	    {
	        get { return _query ?? (_query = new PublishedContentQuery(UmbracoContext.ContentCache, UmbracoContext.MediaCache)); }
	    }

        /// <summary>
        /// Helper method to ensure an umbraco context is set when it is needed
        /// </summary>
        private UmbracoContext UmbracoContext
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
        /// Empty constructor to create an umbraco helper for access to methods that don't have dependencies or used for testing
        /// </summary>
        public UmbracoHelper()
        {            
        }

        public UmbracoHelper(UmbracoContext umbracoContext, IPublishedContent content, PublishedContentQuery query)
            : this(umbracoContext)
        {
            if (content == null) throw new ArgumentNullException("content");
            if (query == null) throw new ArgumentNullException("query");
            _membershipHelper = new MembershipHelper(_umbracoContext);
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
		    _membershipHelper = new MembershipHelper(_umbracoContext);
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
            _membershipHelper = new MembershipHelper(_umbracoContext);
			if (_umbracoContext.IsFrontEndUmbracoRequest)
			{
				_currentPage = _umbracoContext.PublishedContentRequest.PublishedContent;
			}
		}

        public UmbracoHelper(UmbracoContext umbracoContext, PublishedContentQuery query)
            : this(umbracoContext)
        {
            if (query == null) throw new ArgumentNullException("query");
            _query = query;
            _membershipHelper = new MembershipHelper(_umbracoContext);
        }

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
			var templateRenderer = new TemplateRenderer(UmbracoContext, pageId, altTemplateId);
			using (var sw = new StringWriter())
			{
				try
				{
					templateRenderer.Render(sw);					
				}
				catch(Exception ex)
				{
					sw.Write("<!-- Error rendering template with id {0}: '{1}' -->", pageId, ex);
				}
				return new HtmlString(sw.ToString());	
			}			
		}

		#region RenderMacro

		/// <summary>
		/// Renders the macro with the specified alias.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias)
		{
			return RenderMacro(alias, new { });
		}

		/// <summary>
		/// Renders the macro with the specified alias, passing in the specified parameters.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias, object parameters)
		{
			return RenderMacro(alias, parameters.ToDictionary<object>());
		}

		/// <summary>
		/// Renders the macro with the specified alias, passing in the specified parameters.
		/// </summary>
		/// <param name="alias">The alias.</param>
		/// <param name="parameters">The parameters.</param>
		/// <returns></returns>
		public IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters)
		{
			
			if (UmbracoContext.PublishedContentRequest == null)
			{
				throw new InvalidOperationException("Cannot render a macro when there is no current PublishedContentRequest.");
			}

		    return RenderMacro(alias, parameters, UmbracoContext.PublishedContentRequest.UmbracoPage);
		}

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="alias">The alias.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="umbracoPage">The legacy umbraco page object that is required for some macros</param>
        /// <returns></returns>
        internal IHtmlString RenderMacro(string alias, IDictionary<string, object> parameters, page umbracoPage)
        {
            if (alias == null) throw new ArgumentNullException("alias");
            if (umbracoPage == null) throw new ArgumentNullException("umbracoPage");

            var m = macro.GetMacro(alias);
            if (m == null)
            {
                throw new KeyNotFoundException("Could not find macro with alias " + alias);
            }

            return RenderMacro(m, parameters, umbracoPage);
        }

        /// <summary>
        /// Renders the macro with the specified alias, passing in the specified parameters.
        /// </summary>
        /// <param name="m">The macro.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="umbracoPage">The legacy umbraco page object that is required for some macros</param>
        /// <returns></returns>
        internal IHtmlString RenderMacro(macro m, IDictionary<string, object> parameters, page umbracoPage)
        {   
            if (umbracoPage == null) throw new ArgumentNullException("umbracoPage");
            if (m == null) throw new ArgumentNullException("m");

            if (UmbracoContext.PageId == null)
            {
                throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
            }

            var macroProps = new Hashtable();
            foreach (var i in parameters)
            {
                //TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs 
                // looks for a lower case match. WTF. the whole macro concept needs to be rewritten.
                
                
                //NOTE: the value could have html encoded values, so we need to deal with that
                macroProps.Add(i.Key.ToLowerInvariant(), (i.Value is string) ? HttpUtility.HtmlDecode(i.Value.ToString()) : i.Value);
            }
            var macroControl = m.renderMacro(macroProps,
                umbracoPage.Elements,
                UmbracoContext.PageId.Value);

            string html;
            if (macroControl is LiteralControl)
            {
                // no need to execute, we already have text
                html = (macroControl as LiteralControl).Text;
            }
            else
            {
                var containerPage = new FormlessPage();
                containerPage.Controls.Add(macroControl);

                using (var output = new StringWriter())
                {
                    // .Execute() does a PushTraceContext/PopTraceContext and writes trace output straight into 'output'
                    // and I do not see how we could wire the trace context to the current context... so it creates dirty
                    // trace output right in the middle of the page.
                    //
                    // The only thing we can do is fully disable trace output while .Execute() runs and restore afterwards
                    // which means trace output is lost if the macro is a control (.ascx or user control) that is invoked
                    // from within Razor -- which makes sense anyway because the control can _not_ run correctly from
                    // within Razor since it will never be inserted into the page pipeline (which may even not exist at all
                    // if we're running MVC).
                    //
                    // I'm sure there's more things that will get lost with this context changing but I guess we'll figure 
                    // those out as we go along. One thing we lose is the content type response output.
                    // http://issues.umbraco.org/issue/U4-1599 if it is setup during the macro execution. So 
                    // here we'll save the content type response and reset it after execute is called.

                    var contentType = UmbracoContext.HttpContext.Response.ContentType;
                    var traceIsEnabled = containerPage.Trace.IsEnabled;
                    containerPage.Trace.IsEnabled = false;
                    UmbracoContext.HttpContext.Server.Execute(containerPage, output, true);
                    containerPage.Trace.IsEnabled = traceIsEnabled;
                    //reset the content type
                    UmbracoContext.HttpContext.Response.ContentType = contentType;

                    //Now, we need to ensure that local links are parsed
                    html = TemplateUtilities.ParseInternalLinks(output.ToString());
                }
            }

            return new HtmlString(html);
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

            //TODO: commented out until as it is not implemented by umbraco:item yet
            //,string formatString = "")
		{			
            return Field(AssignedContentItem, fieldAlias, altFieldAlias,
                altText, insertBefore, insertAfter, recursive, convertLineBreaks, removeParagraphTags,
                casing, encoding, formatAsDate, formatAsDateWithTime, formatAsDateWithTimeSeparator); // formatString);
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
            
            //TODO: commented out until as it is not implemented by umbraco:item yet
            //,string formatString = "")
		{
			Mandate.ParameterNotNull(currentPage, "currentPage");
			Mandate.ParameterNotNullOrEmpty(fieldAlias, "fieldAlias");

			//TODO: This is real nasty and we should re-write the 'item' and 'ItemRenderer' class but si fine for now

			var attributes = new Dictionary<string, string>
				{
					{"field", fieldAlias},
					{"recursive", recursive.ToString().ToLowerInvariant()},
					{"useifempty", altFieldAlias},
					{"textifempty", altText},
					{"stripparagraph", removeParagraphTags.ToString().ToLowerInvariant()},
					{
						"case", casing == RenderFieldCaseType.Lower ? "lower"
						        	: casing == RenderFieldCaseType.Upper ? "upper"
						        	  	: casing == RenderFieldCaseType.Title ? "title"
						        	  	  	: string.Empty
						},
					{"inserttextbefore", insertBefore},
					{"inserttextafter", insertAfter},
					{"convertlinebreaks", convertLineBreaks.ToString().ToLowerInvariant()},
                    {"formatasdate", formatAsDate.ToString().ToLowerInvariant()},
                    {"formatasdatewithtime", formatAsDateWithTime.ToString().ToLowerInvariant()},
                    {"formatasdatewithtimeseparator", formatAsDateWithTimeSeparator}
				};
			switch (encoding)
			{
				case RenderFieldEncodingType.Url:
					attributes.Add("urlencode", "true");
					break;
				case RenderFieldEncodingType.Html:
					attributes.Add("htmlencode", "true");
					break;
				case RenderFieldEncodingType.Unchanged:
				default:
					break;
			}

			//need to convert our dictionary over to this weird dictionary type
			var attributesForItem = new AttributeCollectionAdapter(
				new AttributeCollection(
					new StateBag()));
			foreach (var i in attributes)
			{
				attributesForItem.Add(i.Key, i.Value);
			}



            var item = new Item(currentPage)
		                   {		        
		                       Field = fieldAlias,
		                       TextIfEmpty = altText,
		                       LegacyAttributes = attributesForItem
		                   };

            //here we are going to check if we are in the context of an Umbraco routed page, if we are we 
            //will leave the NodeId empty since the underlying ItemRenderer will work ever so slightly faster
            //since it already knows about the current page. Otherwise, we'll assign the id based on our
            //currently assigned node. The PublishedContentRequest will be null if:
            // * we are rendering a partial view or child action
            // * we are rendering a view from a custom route
            if ((UmbracoContext.PublishedContentRequest == null 
                || UmbracoContext.PublishedContentRequest.PublishedContent.Id != currentPage.Id)
                && currentPage.Id > 0) // in case we're rendering a detached content (id == 0)
            {
                item.NodeId = currentPage.Id.ToString();
            }
                
		    
			var containerPage = new FormlessPage();
			containerPage.Controls.Add(item);

			using (var output = new StringWriter())
			using (var htmlWriter = new HtmlTextWriter(output))
			{
				ItemRenderer.Instance.Init(item);
				ItemRenderer.Instance.Load(item);
				ItemRenderer.Instance.Render(item, htmlWriter);
				
				//because we are rendering the output through the legacy Item (webforms) stuff, the {localLinks} will already be replaced.
				return new HtmlString(output.ToString());
			}
		}

		#endregion

		#region Dictionary

		private ICultureDictionary _cultureDictionary;

		/// <summary>
		/// Returns the dictionary value for the key specified
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetDictionaryValue(string key)
		{
			if (_cultureDictionary == null)
			{
				var factory = CultureDictionaryFactoryResolver.Current.Factory;
				_cultureDictionary = factory.CreateDictionary();
			}
			return _cultureDictionary[key];
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
                return _membershipHelper.IsLoggedIn()
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
		    return _membershipHelper.IsLoggedIn();
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
            return UmbracoContext.Current.UrlProvider.GetUrl(contentId);
        }

        /// <summary>
        /// Gets the url of a content identified by its identifier, in a specified mode.
        /// </summary>
        /// <param name="contentId">The content identifier.</param>
        /// <param name="mode">The mode.</param>
        /// <returns>The url for the content.</returns>
	    public string Url(int contentId, UrlProviderMode mode)
	    {
	        return UmbracoContext.Current.UrlProvider.GetUrl(contentId, mode);
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
            return UmbracoContext.Current.UrlProvider.GetUrl(contentId, true);
        }

		#endregion

        #region Members

        public IPublishedContent TypedMember(object id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt ? _membershipHelper.GetById(asInt.Result) : _membershipHelper.GetByProviderKey(id);
        }

        public IPublishedContent TypedMember(int id)
        {
            return _membershipHelper.GetById(id);
        }

        public IPublishedContent TypedMember(string id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt ? _membershipHelper.GetById(asInt.Result) : _membershipHelper.GetByProviderKey(id);
        }

        public dynamic Member(object id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt
                ? _membershipHelper.GetById(asInt.Result).AsDynamic()
                : _membershipHelper.GetByProviderKey(id).AsDynamic();
        }

        public dynamic Member(int id)
        {
            return _membershipHelper.GetById(id).AsDynamic();
        }

        public dynamic Member(string id)
        {
            var asInt = id.TryConvertTo<int>();
            return asInt
                ? _membershipHelper.GetById(asInt.Result).AsDynamic()
                : _membershipHelper.GetByProviderKey(id).AsDynamic();
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
			if (bool.Parse(Umbraco.Core.Configuration.GlobalSettings.EditXhtmlMode))
				return text.Replace("\n", "<br/>\n");
			else
				return text.Replace("\n", "<br />\n");
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
			return StripHtmlTags(html, tags);
		}

		private HtmlString StripHtmlTags(string html, params string[] tags)
		{
			var doc = new HtmlDocument();
			doc.LoadHtml("<p>" + html + "</p>");
            var targets = new List<HtmlNode>();

            var nodes = doc.DocumentNode.FirstChild.SelectNodes(".//*");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    //is element
                    if (node.NodeType != HtmlNodeType.Element) continue;
                    var filterAllTags = (tags == null || !tags.Any());
                    if (filterAllTags || tags.Any(tag => string.Equals(tag, node.Name, StringComparison.CurrentCultureIgnoreCase)))
                    {
                        targets.Add(node);
                    }
                }
                foreach (var target in targets)
                {
                    HtmlNode content = doc.CreateTextNode(target.InnerText);
                    target.ParentNode.ReplaceChild(content, target);
                }
            }
            else
            {
                return new HtmlString(html);
            }
            return new HtmlString(doc.DocumentNode.FirstChild.InnerHtml);
		}

		public string Coalesce(params object[] args)
		{
			return Coalesce<DynamicNull>(args);
		}

		internal string Coalesce<TIgnore>(params object[] args)
		{
			foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
			{
				return sArg;
			}
			return string.Empty;
		}

		public string Concatenate(params object[] args)
		{
			return Concatenate<DynamicNull>(args);
		}

		internal string Concatenate<TIgnore>(params object[] args)
		{
			var result = new StringBuilder();
			foreach (var sArg in args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)))
			{
				result.Append(sArg);
			}
			return result.ToString();
		}

		public string Join(string seperator, params object[] args)
		{
			return Join<DynamicNull>(seperator, args);
		}

		internal string Join<TIgnore>(string seperator, params object[] args)
		{
			var results = args.Where(arg => arg != null && arg.GetType() != typeof(TIgnore)).Select(arg => string.Format("{0}", arg)).Where(sArg => !string.IsNullOrWhiteSpace(sArg)).ToList();
			return string.Join(seperator, results);
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
            using (var outputms = new MemoryStream())
            {
                using (var outputtw = new StreamWriter(outputms))
                {
                    using (var ms = new MemoryStream())
                    {
                        using (var tw = new StreamWriter(ms))
                        {
                            tw.Write(html);
                            tw.Flush();
                            ms.Position = 0;
                            var tagStack = new Stack<string>();

                            using (TextReader tr = new StreamReader(ms))
                            {
                                bool isInsideElement = false,
                                    lengthReached = false,
                                    insideTagSpaceEncountered = false,
                                    isTagClose = false;

                                int ic = 0,
                                    currentLength = 0,
                                    currentTextLength = 0;

                                string currentTag = string.Empty,
                                    tagContents = string.Empty;

                                while ((ic = tr.Read()) != -1)
                                {
                                    bool write = true;

                                    switch ((char)ic)
                                    {
                                        case '<':
                                            if (!lengthReached)
                                            {
                                                isInsideElement = true;
                                            }

                                            insideTagSpaceEncountered = false;
                                            currentTag = string.Empty;
                                            tagContents = string.Empty;
                                            isTagClose = false;
                                            if (tr.Peek() == (int)'/')
                                            {
                                                isTagClose = true;
                                            }
                                            break;

                                        case '>':
                                            isInsideElement = false;

                                            if (isTagClose && tagStack.Count > 0)
                                            {
                                                string thisTag = tagStack.Pop();
                                                outputtw.Write("</" + thisTag + ">");
                                            }
                                            if (!isTagClose && currentTag.Length > 0)
                                            {
                                                if (!lengthReached)
                                                {
                                                    tagStack.Push(currentTag);
                                                    outputtw.Write("<" + currentTag);
                                                    if (!string.IsNullOrEmpty(tagContents))
                                                    {
                                                        if (tagContents.EndsWith("/"))
                                                        {
                                                            // No end tag e.g. <br />.
                                                            tagStack.Pop();
                                                        }

                                                        outputtw.Write(tagContents);
                                                        write = true;
                                                        insideTagSpaceEncountered = false;
                                                    }
                                                    outputtw.Write(">");
                                                }
                                            }
                                            // Continue to next iteration of the text reader.
                                            continue;

                                        default:
                                            if (isInsideElement)
                                            {
                                                if (ic == (int)' ')
                                                {
                                                    if (!insideTagSpaceEncountered)
                                                    {
                                                        insideTagSpaceEncountered = true;
                                                    }
                                                }

                                                if (!insideTagSpaceEncountered)
                                                {
                                                    currentTag += (char)ic;
                                                }
                                            }
                                            break;
                                    }

                                    if (isInsideElement || insideTagSpaceEncountered)
                                    {
                                        write = false;
                                        if (insideTagSpaceEncountered)
                                        {
                                            tagContents += (char)ic;
                                        }
                                    }

                                    if (!isInsideElement || treatTagsAsContent)
                                    {
                                        currentTextLength++;
                                    }

                                    if (currentTextLength <= length || (lengthReached && isInsideElement))
                                    {
                                        if (write)
                                        {
                                            var charToWrite = (char)ic;
                                            outputtw.Write(charToWrite);
                                            currentLength++;
                                        }
                                    }

                                    if (!lengthReached && currentTextLength >= length)
                                    {
                                        // Reached truncate limit.
                                        if (addElipsis)
                                        {
                                            outputtw.Write("&hellip;");
                                        }
                                        lengthReached = true;
                                    }

                                }

                            }
                        }
                    }
                    outputtw.Flush();
					outputms.Position = 0;
					using (TextReader outputtr = new StreamReader(outputms))
					{
						return new HtmlString(outputtr.ReadToEnd().Replace("  ", " ").Trim());
					}
				}
			}
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
            var ds = _umbracoContext.Application.Services.DataTypeService;
            return ds.GetPreValueAsString(id);
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
