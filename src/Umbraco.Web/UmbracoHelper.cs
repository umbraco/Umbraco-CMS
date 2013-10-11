using System;
using System.Collections;
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
using Umbraco.Core.Xml;
using Umbraco.Web.Models;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Templates;
using umbraco;
using System.Collections.Generic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.templateControls;
using Member = umbraco.cms.businesslogic.member.Member;

namespace Umbraco.Web
{

	/// <summary>
	/// A helper class that provides many useful methods and functionality for using Umbraco in templates
	/// </summary>
	public class UmbracoHelper
	{
		private readonly UmbracoContext _umbracoContext;
		private readonly IPublishedContent _currentPage;

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
			var templateRenderer = new TemplateRenderer(_umbracoContext, pageId, altTemplateId);
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
			
			if (_umbracoContext.PublishedContentRequest == null)
			{
				throw new InvalidOperationException("Cannot render a macro when there is no current PublishedContentRequest.");
			}

		    return RenderMacro(alias, parameters, _umbracoContext.PublishedContentRequest.UmbracoPage);
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

            if (_umbracoContext.PageId == null)
            {
                throw new InvalidOperationException("Cannot render a macro when UmbracoContext.PageId is null.");
            }

            var macroProps = new Hashtable();
            foreach (var i in parameters)
            {
                //TODO: We are doing at ToLower here because for some insane reason the UpdateMacroModel method of macro.cs 
                // looks for a lower case match. WTF. the whole macro concept needs to be rewritten.
                macroProps.Add(i.Key.ToLower(), i.Value);
            }
            var macroControl = m.renderMacro(macroProps,
                umbracoPage.Elements,
                _umbracoContext.PageId.Value);

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

                    var contentType = _umbracoContext.HttpContext.Response.ContentType;
                    var traceIsEnabled = containerPage.Trace.IsEnabled;
                    containerPage.Trace.IsEnabled = false;
                    _umbracoContext.HttpContext.Server.Execute(containerPage, output, true);
                    containerPage.Trace.IsEnabled = traceIsEnabled;
                    //reset the content type
                    _umbracoContext.HttpContext.Response.ContentType = contentType;

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
            if (_umbracoContext.PublishedContentRequest == null 
                || _umbracoContext.PublishedContentRequest.PublishedContent.Id != currentPage.Id)
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

		/// <summary>
		/// Check if a document object is protected by the "Protect Pages" functionality in umbraco
		/// </summary>
		/// <param name="documentId">The identifier of the document object to check</param>
		/// <param name="path">The full path of the document object to check</param>
		/// <returns>True if the document object is protected</returns>
		public bool IsProtected(int documentId, string path)
		{
			return Access.IsProtected(documentId, path);
		}

		/// <summary>
		/// Check if the current user has access to a document
		/// </summary>
		/// <param name="nodeId">The identifier of the document object to check</param>
		/// <param name="path">The full path of the document object to check</param>
		/// <returns>True if the current user has access or if the current document isn't protected</returns>
		public bool MemberHasAccess(int nodeId, string path)
		{
			if (IsProtected(nodeId, path))
			{
				return Member.IsLoggedOn() && Access.HasAccess(nodeId, path, Membership.GetUser());
			}
			return true;
		}

		/// <summary>
		/// Whether or not the current member is logged in (based on the membership provider)
		/// </summary>
		/// <returns>True is the current user is logged in</returns>
		public bool MemberIsLoggedOn()
		{
			/*
			   MembershipUser u = Membership.GetUser();
			   return u != null;           
			*/
			return Member.IsLoggedOn();
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

		#region Content

		public IPublishedContent TypedContent(object id)
		{
            return TypedDocumentById(id, _umbracoContext.ContentCache);
		}

		public IPublishedContent TypedContent(int id)
		{
            return TypedDocumentById(id, _umbracoContext.ContentCache);
		}

		public IPublishedContent TypedContent(string id)
		{
            return TypedDocumentById(id, _umbracoContext.ContentCache);
		}

        public IPublishedContent TypedContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return TypedDocumentByXPath(xpath, vars, _umbracoContext.ContentCache);
        }

		public IEnumerable<IPublishedContent> TypedContent(params object[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.ContentCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedContent(params int[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.ContentCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedContent(params string[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.ContentCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<object> ids)
		{
			return TypedContent(ids.ToArray());
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<string> ids)
		{
			return TypedContent(ids.ToArray());
		}

		public IEnumerable<IPublishedContent> TypedContent(IEnumerable<int> ids)
		{
			return TypedContent(ids.ToArray());
		}

        public IEnumerable<IPublishedContent> TypedContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return TypedDocumentsByXPath(xpath, vars, _umbracoContext.ContentCache);
        }

        public IEnumerable<IPublishedContent> TypedContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return TypedDocumentsByXPath(xpath, vars, _umbracoContext.ContentCache);
        }

        public IEnumerable<IPublishedContent> TypedContentAtRoot()
        {
            return TypedDocumentsAtRoot(_umbracoContext.ContentCache);
        }

		public dynamic Content(object id)
		{
            return DocumentById(id, _umbracoContext.ContentCache, DynamicNull.Null);
		}

		public dynamic Content(int id)
		{
            return DocumentById(id, _umbracoContext.ContentCache, DynamicNull.Null);
		}

		public dynamic Content(string id)
		{
            return DocumentById(id, _umbracoContext.ContentCache, DynamicNull.Null);
		}

        public dynamic ContentSingleAtXPath(string xpath, params XPathVariable[] vars)
        {
            return DocumentByXPath(xpath, vars, _umbracoContext.ContentCache, DynamicNull.Null);
        }

        public dynamic ContentSingleAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return DocumentByXPath(xpath, vars, _umbracoContext.ContentCache, DynamicNull.Null);
        }

        public dynamic Content(params object[] ids)
		{
            return DocumentByIds(_umbracoContext.ContentCache, ids);
		}

		public dynamic Content(params int[] ids)
		{
            return DocumentByIds(_umbracoContext.ContentCache, ids);
		}

		public dynamic Content(params string[] ids)
		{
            return DocumentByIds(_umbracoContext.ContentCache, ids);
		}

		public dynamic Content(IEnumerable<object> ids)
		{
			return Content(ids.ToArray());
		}

		public dynamic Content(IEnumerable<int> ids)
		{
			return Content(ids.ToArray());
		}

		public dynamic Content(IEnumerable<string> ids)
		{
			return Content(ids.ToArray());
		}

        public dynamic ContentAtXPath(string xpath, params XPathVariable[] vars)
        {
            return DocumentsByXPath(xpath, vars, _umbracoContext.ContentCache);
        }

        public dynamic ContentAtXPath(XPathExpression xpath, params XPathVariable[] vars)
        {
            return DocumentsByXPath(xpath, vars, _umbracoContext.ContentCache);
        }

        public dynamic ContentAtRoot()
        {
            return DocumentsAtRoot(_umbracoContext.ContentCache);
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
			return TypedDocumentById(id, _umbracoContext.MediaCache);
		}

		public IPublishedContent TypedMedia(int id)
		{
            return TypedDocumentById(id, _umbracoContext.MediaCache);
		}

		public IPublishedContent TypedMedia(string id)
		{
            return TypedDocumentById(id, _umbracoContext.MediaCache);
		}

		public IEnumerable<IPublishedContent> TypedMedia(params object[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.MediaCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedMedia(params int[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.MediaCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedMedia(params string[] ids)
		{
            return TypedDocumentsbyIds(_umbracoContext.MediaCache, ids);
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<object> ids)
		{
			return TypedMedia(ids.ToArray());
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<int> ids)
		{
			return TypedMedia(ids.ToArray());
		}

		public IEnumerable<IPublishedContent> TypedMedia(IEnumerable<string> ids)
		{
			return TypedMedia(ids.ToArray());
		}

        public IEnumerable<IPublishedContent> TypedMediaAtRoot()
        {
            return TypedDocumentsAtRoot(_umbracoContext.MediaCache);
        }

		public dynamic Media(object id)
		{
            return DocumentById(id, _umbracoContext.MediaCache, DynamicNull.Null);
		}

		public dynamic Media(int id)
		{
            return DocumentById(id, _umbracoContext.MediaCache, DynamicNull.Null);
		}

		public dynamic Media(string id)
		{
            return DocumentById(id, _umbracoContext.MediaCache, DynamicNull.Null);
		}

		public dynamic Media(params object[] ids)
		{
            return DocumentByIds(_umbracoContext.MediaCache, ids);
		}

		public dynamic Media(params int[] ids)
		{
            return DocumentByIds(_umbracoContext.MediaCache, ids);
		}

		public dynamic Media(params string[] ids)
		{
            return DocumentByIds(_umbracoContext.MediaCache, ids);
		}

		public dynamic Media(IEnumerable<object> ids)
		{
			return Media(ids.ToArray());
		}

		public dynamic Media(IEnumerable<int> ids)
		{
			return Media(ids.ToArray());
		}

		public dynamic Media(IEnumerable<string> ids)
		{
			return Media(ids.ToArray());
		}

        public dynamic MediaAtRoot()
        {
            return DocumentsAtRoot(_umbracoContext.MediaCache);
        }

		#endregion

		#region Used by Content/Media

		/// <summary>
		/// Overloaded method accepting an 'object' type
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cache"> </param>
		/// <returns></returns>
		/// <remarks>
		/// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass 
		/// this result in to this method.
		/// This method will throw an exception if the value is not of type int or string.
		/// </remarks>
        private IPublishedContent TypedDocumentById(object id, ContextualPublishedCache cache)
		{
			if (id is string)
				return TypedDocumentById((string)id, cache);
			if (id is int)
				return TypedDocumentById((int)id, cache);
			throw new InvalidOperationException("The value of parameter 'id' must be either a string or an integer");
		}

		private IPublishedContent TypedDocumentById(int id, ContextualPublishedCache cache)
		{
            var doc = cache.GetById(id);
			return doc;
		}

        private IPublishedContent TypedDocumentById(string id, ContextualPublishedCache cache)
		{
			int docId;
			return int.TryParse(id, out docId)
				       ? DocumentById(docId, cache, null)
				       : null;
		}

        private IPublishedContent TypedDocumentByXPath(string xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        private IPublishedContent TypedDocumentByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc;
        }

        /// <summary>
		/// Overloaded method accepting an 'object' type
		/// </summary>
		/// <param name="ids"></param>
		/// <param name="cache"> </param>
		/// <returns></returns>
		/// <remarks>
		/// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass 
		/// this result in to this method.
		/// This method will throw an exception if the value is not of type int or string.
		/// </remarks>
        private IEnumerable<IPublishedContent> TypedDocumentsbyIds(ContextualPublishedCache cache, params object[] ids)
		{
			return ids.Select(eachId => TypedDocumentById(eachId, cache));
		}

        private IEnumerable<IPublishedContent> TypedDocumentsbyIds(ContextualPublishedCache cache, params int[] ids)
		{
			return ids.Select(eachId => TypedDocumentById(eachId, cache));
		}

        private IEnumerable<IPublishedContent> TypedDocumentsbyIds(ContextualPublishedCache cache, params string[] ids)
		{
			return ids.Select(eachId => TypedDocumentById(eachId, cache));
		}

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(string xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedContentCache cache)
        {
            var doc = cache.GetByXPath(xpath, vars);
            return doc;
        }

        private IEnumerable<IPublishedContent> TypedDocumentsAtRoot(ContextualPublishedCache cache)
        {
            return cache.GetAtRoot();
        }

		/// <summary>
		/// Overloaded method accepting an 'object' type
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cache"> </param>
		/// <param name="ifNotFound"> </param>
		/// <returns></returns>
		/// <remarks>
		/// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass 
		/// this result in to this method.
		/// This method will throw an exception if the value is not of type int or string.
		/// </remarks>
        private dynamic DocumentById(object id, ContextualPublishedCache cache, object ifNotFound)
		{
			if (id is string)
				return DocumentById((string)id, cache, ifNotFound);
			if (id is int)
				return DocumentById((int)id, cache, ifNotFound);
			throw new InvalidOperationException("The value of parameter 'id' must be either a string or an integer");
		}

        private dynamic DocumentById(int id, ContextualPublishedCache cache, object ifNotFound)
		{
            var doc = cache.GetById(id);
			return doc == null
					? ifNotFound
					: new DynamicPublishedContent(doc).AsDynamic();
		}

        private dynamic DocumentById(string id, ContextualPublishedCache cache, object ifNotFound)
		{
			int docId;
			return int.TryParse(id, out docId)
				? DocumentById(docId, cache, ifNotFound)
				: ifNotFound;
		}

        private dynamic DocumentByXPath(string xpath, XPathVariable[] vars, ContextualPublishedCache cache, object ifNotFound)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc == null
                ? ifNotFound
                : new DynamicPublishedContent(doc).AsDynamic();
        }

        private dynamic DocumentByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedCache cache, object ifNotFound)
        {
            var doc = cache.GetSingleByXPath(xpath, vars);
            return doc == null
                ? ifNotFound
                : new DynamicPublishedContent(doc).AsDynamic();
        }

        /// <summary>
		/// Overloaded method accepting an 'object' type
		/// </summary>
		/// <param name="ids"></param>
		/// <param name="cache"> </param>
		/// <returns></returns>
		/// <remarks>
		/// We accept an object type because GetPropertyValue now returns an 'object', we still want to allow people to pass 
		/// this result in to this method.
		/// This method will throw an exception if the value is not of type int or string.
		/// </remarks>
        private dynamic DocumentByIds(ContextualPublishedCache cache, params object[] ids)
		{
            var dNull = DynamicNull.Null;
			var nodes = ids.Select(eachId => DocumentById(eachId, cache, dNull))
				.Where(x => !TypeHelper.IsTypeAssignableFrom<DynamicNull>(x))
				.Cast<DynamicPublishedContent>();
			return new DynamicPublishedContentList(nodes);
		}

        private dynamic DocumentByIds(ContextualPublishedCache cache, params int[] ids)
		{
            var dNull = DynamicNull.Null;
			var nodes = ids.Select(eachId => DocumentById(eachId, cache, dNull))
				.Where(x => !TypeHelper.IsTypeAssignableFrom<DynamicNull>(x))
				.Cast<DynamicPublishedContent>();
			return new DynamicPublishedContentList(nodes);
		}

        private dynamic DocumentByIds(ContextualPublishedCache cache, params string[] ids)
		{
            var dNull = DynamicNull.Null;
			var nodes = ids.Select(eachId => DocumentById(eachId, cache, dNull))
				.Where(x => !TypeHelper.IsTypeAssignableFrom<DynamicNull>(x))
				.Cast<DynamicPublishedContent>();
			return new DynamicPublishedContentList(nodes);
		}

        private dynamic DocumentsByXPath(string xpath, XPathVariable[] vars, ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetByXPath(xpath, vars)
                    .Select(publishedContent => new DynamicPublishedContent(publishedContent))
            );
        }

        private dynamic DocumentsByXPath(XPathExpression xpath, XPathVariable[] vars, ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetByXPath(xpath, vars)
                    .Select(publishedContent => new DynamicPublishedContent(publishedContent))
            );
        }

        private dynamic DocumentsAtRoot(ContextualPublishedCache cache)
        {
            return new DynamicPublishedContentList(
                cache.GetAtRoot()
                    .Select(publishedContent => new DynamicPublishedContent(publishedContent))
            );
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
			return new DynamicPublishedContentList(
				TypedSearch(term, useWildCards, searchProvider));
		}

		/// <summary>
		/// Searhes content
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="searchProvider"></param>
		/// <returns></returns>
		public dynamic Search(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			return new DynamicPublishedContentList(
				TypedSearch(criteria, searchProvider));
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
			var searcher = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (!string.IsNullOrEmpty(searchProvider))
				searcher = Examine.ExamineManager.Instance.SearchProviderCollection[searchProvider];

			var results = searcher.Search(term, useWildCards);
			return results.ConvertSearchResultToPublishedContent(_umbracoContext.ContentCache);
		}

		/// <summary>
		/// Searhes content
		/// </summary>
		/// <param name="criteria"></param>
		/// <param name="searchProvider"></param>
		/// <returns></returns>
		public IEnumerable<IPublishedContent> TypedSearch(Examine.SearchCriteria.ISearchCriteria criteria, Examine.Providers.BaseSearchProvider searchProvider = null)
		{
			var s = Examine.ExamineManager.Instance.DefaultSearchProvider;
			if (searchProvider != null)
				s = searchProvider;

			var results = s.Search(criteria);
			return results.ConvertSearchResultToPublishedContent(_umbracoContext.ContentCache);
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
			using (var ms = new MemoryStream())
			{
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
								bool IsInsideElement = false;
								bool lengthReached = false;
								int ic = 0;
								int currentLength = 0, currentTextLength = 0;
								string currentTag = string.Empty;
								string tagContents = string.Empty;
								bool insideTagSpaceEncountered = false;
								bool isTagClose = false;
								while ((ic = tr.Read()) != -1)
								{
									bool write = true;

									if (ic == (int)'<')
									{
										if (!lengthReached)
										{
											IsInsideElement = true;
										}
										insideTagSpaceEncountered = false;
										currentTag = string.Empty;
										tagContents = string.Empty;
										isTagClose = false;
										if (tr.Peek() == (int)'/')
										{
											isTagClose = true;
										}
									}
									else if (ic == (int)'>')
									{
										//if (IsInsideElement)
										//{
										IsInsideElement = false;
										//if (write)
										//{
										//  outputtw.Write('>');
										//}
										currentTextLength++;
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
												if (tr.Peek() != (int)' ')
												{
													if (!string.IsNullOrEmpty(tagContents))
													{
														if (tagContents.EndsWith("/"))
														{
															//short close
															tagStack.Pop();
														}
														outputtw.Write(tagContents);
													}
													outputtw.Write(">");
												}
											}
										}
										//}
										continue;
									}
									else
									{
										if (IsInsideElement)
										{
											if (ic == (int)' ')
											{
												if (!insideTagSpaceEncountered)
												{
													insideTagSpaceEncountered = true;
													//if (!isTagClose)
													//{
													// tagStack.Push(currentTag);
													//}
												}
											}
											if (!insideTagSpaceEncountered)
											{
												currentTag += (char)ic;
											}
										}
									}
									if (IsInsideElement || insideTagSpaceEncountered)
									{
										write = false;
										if (insideTagSpaceEncountered)
										{
											tagContents += (char)ic;
										}
									}
									if (!IsInsideElement || treatTagsAsContent)
									{
										currentTextLength++;
									}
									currentLength++;
									if (currentTextLength <= length || (lengthReached && IsInsideElement))
									{
										if (write)
										{
											outputtw.Write((char)ic);
										}
									}
									if (!lengthReached && currentTextLength >= length)
									{
										//reached truncate point
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
