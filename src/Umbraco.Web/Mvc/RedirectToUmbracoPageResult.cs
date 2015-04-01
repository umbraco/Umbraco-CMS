using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
	/// Redirects to an Umbraco page by Id or Entity
	/// </summary>
	public class RedirectToUmbracoPageResult : ActionResult
	{
		private IPublishedContent _publishedContent;
		private readonly int _pageId;
        private NameValueCollection _queryStringValues;
		private readonly UmbracoContext _umbracoContext;
		private string _url;

		public string Url
		{
			get
			{
				if (!_url.IsNullOrWhiteSpace()) return _url;

				if (PublishedContent == null)
				{
					throw new InvalidOperationException(string.Format("Cannot redirect, no entity was found for id {0}", _pageId));
				}

				var result = _umbracoContext.RoutingContext.UrlProvider.GetUrl(PublishedContent.Id);
				if (result != "#")
				{
					_url = result;
					return _url;
				}

				throw new InvalidOperationException(string.Format("Could not route to entity with id {0}, the NiceUrlProvider could not generate a URL", _pageId));

			}
		}

		public IPublishedContent PublishedContent
		{
			get
			{
				if (_publishedContent != null) return _publishedContent;

				//need to get the URL for the page
			    _publishedContent = UmbracoContext.Current.ContentCache.GetById(_pageId);

				return _publishedContent;
			}
		}

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="pageId"></param>
		public RedirectToUmbracoPageResult(int pageId)
			: this(pageId, UmbracoContext.Current)
		{
		}

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(int pageId, NameValueCollection queryStringValues)
            : this(pageId, queryStringValues, UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        public RedirectToUmbracoPageResult(int pageId, string queryString)
            : this(pageId, queryString, UmbracoContext.Current)
        {
        }

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="publishedContent"></param>
		public RedirectToUmbracoPageResult(IPublishedContent publishedContent)
			: this(publishedContent, UmbracoContext.Current)
		{
		}

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, NameValueCollection queryStringValues)
            : this(publishedContent, queryStringValues, UmbracoContext.Current)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, string queryString)
            : this(publishedContent, queryString, UmbracoContext.Current)
        {
        }

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="pageId"></param>
		/// <param name="umbracoContext"></param>
		public RedirectToUmbracoPageResult(int pageId, UmbracoContext umbracoContext)
		{
			_pageId = pageId;
			_umbracoContext = umbracoContext;
		}

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoPageResult(int pageId, NameValueCollection queryStringValues, UmbracoContext umbracoContext)
        {
            _pageId = pageId;
            _queryStringValues = queryStringValues;
            _umbracoContext = umbracoContext;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoPageResult(int pageId, string queryString, UmbracoContext umbracoContext)
        {
            _pageId = pageId;
            _queryStringValues = ParseQueryString(queryString);
            _umbracoContext = umbracoContext;
        }

		/// <summary>
		/// Creates a new RedirectToUmbracoResult
		/// </summary>
		/// <param name="publishedContent"></param>
		/// <param name="umbracoContext"></param>
		public RedirectToUmbracoPageResult(IPublishedContent publishedContent, UmbracoContext umbracoContext)
		{
			_publishedContent = publishedContent;
			_pageId = publishedContent.Id;
			_umbracoContext = umbracoContext;
		}

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, NameValueCollection queryStringValues, UmbracoContext umbracoContext)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = queryStringValues;
            _umbracoContext = umbracoContext;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryString"></param>
        /// <param name="umbracoContext"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, string queryString, UmbracoContext umbracoContext)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = ParseQueryString(queryString);
            _umbracoContext = umbracoContext;
        }

		public override void ExecuteResult(ControllerContext context)
		{
			if (context == null) throw new ArgumentNullException("context");

			if (context.IsChildAction)
			{
				throw new InvalidOperationException("Cannot redirect from a Child Action");
			}

			var destinationUrl = UrlHelper.GenerateContentUrl(Url, context.HttpContext);

            if (_queryStringValues != null && _queryStringValues.Count > 0)
            {
                destinationUrl = destinationUrl += "?" + string.Join("&",
                    _queryStringValues.AllKeys.Select(x => x + "=" + HttpUtility.UrlEncode(_queryStringValues[x])));
            }

			context.Controller.TempData.Keep();

			context.HttpContext.Response.Redirect(destinationUrl, endResponse: false);
		}

        private NameValueCollection ParseQueryString(string queryString)
        {
            if (!string.IsNullOrEmpty(queryString))
            {
                return HttpUtility.ParseQueryString(queryString);
            }

            return null;
        }
	}
}