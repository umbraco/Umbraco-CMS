using System;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Routing;

namespace Umbraco.Web.Website.ActionResults
{
    /// <summary>
    /// Redirects to an Umbraco page by Id or Entity
    /// </summary>
    public class RedirectToUmbracoPageResult : IActionResult
    {
        private IPublishedContent _publishedContent;
        private readonly int _pageId;
        private readonly NameValueCollection _queryStringValues;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private string _url;

        private string Url
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_url)) return _url;

                if (PublishedContent is null)
                    throw new InvalidOperationException($"Cannot redirect, no entity was found for id {PageId}");

                var result = _publishedUrlProvider.GetUrl(PublishedContent.Id);

                if (result == "#")
                    throw new InvalidOperationException(
                        $"Could not route to entity with id {PageId}, the NiceUrlProvider could not generate a URL");

                _url = result;

                return _url;
            }
        }

        private int PageId => _pageId;

        private IPublishedContent PublishedContent
        {
            get
            {
                if (!(_publishedContent is null)) return _publishedContent;

                //need to get the URL for the page
                _publishedContent = _umbracoContextAccessor.GetRequiredUmbracoContext().Content.GetById(_pageId);

               return _publishedContent;
            }
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="publishedUrlProvider"></param>
        public RedirectToUmbracoPageResult(int pageId, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _pageId = pageId;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="publishedUrlProvider"></param>
        public RedirectToUmbracoPageResult(int pageId, NameValueCollection queryStringValues, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _pageId = pageId;
            _queryStringValues = queryStringValues;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryString"></param>
        /// <param name="publishedUrlProvider"></param>
        public RedirectToUmbracoPageResult(int pageId, string queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _pageId = pageId;
            _queryStringValues = ParseQueryString(queryString);
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="publishedUrlProvider"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="publishedUrlProvider"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, NameValueCollection queryStringValues, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = queryStringValues;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="publishedContent"></param>
        /// <param name="queryString"></param>
        /// <param name="publishedUrlProvider"></param>
        /// <param name="umbracoContextAccessor"></param>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, string queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            _pageId = publishedContent.Id;
            _queryStringValues = ParseQueryString(queryString);
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var httpContext = context.HttpContext;
            var ioHelper = httpContext.RequestServices.GetRequiredService<IIOHelper>();
            var destinationUrl = ioHelper.ResolveUrl(Url);

            if (!(_queryStringValues is null) && _queryStringValues.Count > 0)
            {
                destinationUrl += "?" + string.Join("&",
                                                       _queryStringValues.AllKeys.Select(x => x + "=" + HttpUtility.UrlEncode(_queryStringValues[x])));
            }

            var tempDataDictionaryFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataDictionaryFactory.GetTempData(context.HttpContext);
            tempData?.Keep();

            httpContext.Response.Redirect(destinationUrl);

            return Task.CompletedTask;
        }

        private NameValueCollection ParseQueryString(string queryString)
        {
            return !string.IsNullOrEmpty(queryString) ? HttpUtility.ParseQueryString(queryString) : null;
        }
    }
}
