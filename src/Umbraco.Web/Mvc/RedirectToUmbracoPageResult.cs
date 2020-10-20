using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Composing;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Redirects to an Umbraco page by Id or Entity
    /// </summary>
    /// Migrated already to .Net Core
    public class RedirectToUmbracoPageResult : ActionResult
    {
        private IPublishedContent _publishedContent;
        private readonly int _pageId;
        private readonly Guid _key;
        private NameValueCollection _queryStringValues;
        private IPublishedUrlProvider _publishedUrlProvider;
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

                var result = _publishedUrlProvider.GetUrl(PublishedContent.Id);
                if (result != "#")
                {
                    _url = result;
                    return _url;
                }

                throw new InvalidOperationException(string.Format("Could not route to entity with id {0}, the NiceUrlProvider could not generate a URL", _pageId));

            }
        }

        public int PageId
        {
            get { return _pageId; }
        }

        public Guid Key
        {
            get { return _key; }
        }
        public IPublishedContent PublishedContent
        {
            get
            {
                if (_publishedContent != null) return _publishedContent;

                if (_pageId != default(int))
                {
                    _publishedContent = Current.UmbracoContext.Content.GetById(_pageId);
                }

                else if (_key != default(Guid))
                {
                    _publishedContent = Current.UmbracoContext.Content.GetById(_key);
                }

                return _publishedContent;
            }
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        public RedirectToUmbracoPageResult(int pageId)
            : this(pageId,  Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(int pageId, NameValueCollection queryStringValues)
            : this(pageId, queryStringValues,  Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        public RedirectToUmbracoPageResult(int pageId, string queryString)
            : this(pageId, queryString,  Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent)
            : this(publishedContent,  Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, NameValueCollection queryStringValues)
            : this(publishedContent, queryStringValues,  Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="queryString"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, string queryString)
            : this(publishedContent, queryString, Current.PublishedUrlProvider)
        {
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(int pageId, IPublishedUrlProvider publishedUrlProvider)
        {
            _pageId = pageId;
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(int pageId, NameValueCollection queryStringValues, IPublishedUrlProvider publishedUrlProvider)
        {
            _pageId = pageId;
            _queryStringValues = queryStringValues;
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(int pageId, string queryString, IPublishedUrlProvider publishedUrlProvider)
        {
            _pageId = pageId;
            _queryStringValues = ParseQueryString(queryString);
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="key"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(Guid key)
        {
            _key = key;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="key"></param>
        /// <param name="queryStringValues"></param>
        public RedirectToUmbracoPageResult(Guid key, NameValueCollection queryStringValues)
        {
            _key = key;
            _queryStringValues = queryStringValues;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="key"></param>
        /// <param name="queryString"></param>
        public RedirectToUmbracoPageResult(Guid key, string queryString)
        {
            _key = key;
            _queryStringValues = ParseQueryString(queryString);
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, IPublishedUrlProvider publishedUrlProvider)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, NameValueCollection queryStringValues, IPublishedUrlProvider publishedUrlProvider)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = queryStringValues;
            _publishedUrlProvider = publishedUrlProvider;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryString"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, string queryString, IPublishedUrlProvider publishedUrlProvider)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = ParseQueryString(queryString);
            _publishedUrlProvider = publishedUrlProvider;
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
