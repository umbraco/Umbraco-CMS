using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
        private readonly Guid _key;
        private readonly QueryString _queryString;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private string _url;

        private string Url
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_url)) return _url;

                if (PublishedContent is null)
                    throw new InvalidOperationException($"Cannot redirect, no entity was found for key {Key}");

                var result = _publishedUrlProvider.GetUrl(PublishedContent.Id);

                if (result == "#")
                    throw new InvalidOperationException(
                        $"Could not route to entity with key {Key}, the NiceUrlProvider could not generate a URL");

                _url = result;

                return _url;
            }
        }
        public Guid Key => _key;
        private IPublishedContent PublishedContent
        {
            get
            {
                if (!(_publishedContent is null)) return _publishedContent;

                //need to get the URL for the page
                _publishedContent = _umbracoContextAccessor.GetRequiredUmbracoContext().Content.GetById(_key);

               return _publishedContent;
            }
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="publishedUrlProvider"></param>
        public RedirectToUmbracoPageResult(Guid key, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _key = key;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Creates a new RedirectToUmbracoResult
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="queryStringValues"></param>
        /// <param name="publishedUrlProvider"></param>
        public RedirectToUmbracoPageResult(Guid key, QueryString queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _key = key;
            _queryString = queryString;
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
            _key = publishedContent.Key;
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
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, QueryString queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            _key = publishedContent.Key;
            _queryString = queryString;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            var httpContext = context.HttpContext;
            var ioHelper = httpContext.RequestServices.GetRequiredService<IIOHelper>();
            var destinationUrl = ioHelper.ResolveUrl(Url);

            if (_queryString.HasValue)
            {
                destinationUrl += _queryString.ToUriComponent();
            }

            var tempDataDictionaryFactory = context.HttpContext.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            var tempData = tempDataDictionaryFactory.GetTempData(context.HttpContext);
            tempData?.Keep();

            httpContext.Response.Redirect(destinationUrl);

            return Task.CompletedTask;
        }

    }
}
