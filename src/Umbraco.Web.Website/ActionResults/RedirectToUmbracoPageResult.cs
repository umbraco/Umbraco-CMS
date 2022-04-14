using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Website.ActionResults
{
    /// <summary>
    /// Redirects to an Umbraco page by Id or Entity
    /// </summary>
    public class RedirectToUmbracoPageResult : IActionResult, IKeepTempDataResult
    {
        private IPublishedContent _publishedContent;
        private readonly QueryString _queryString;
        private readonly IPublishedUrlProvider _publishedUrlProvider;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private string _url;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToUmbracoPageResult"/> class.
        /// </summary>
        public RedirectToUmbracoPageResult(Guid key, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            Key = key;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToUmbracoPageResult"/> class.
        /// </summary>
        public RedirectToUmbracoPageResult(Guid key, QueryString queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            Key = key;
            _queryString = queryString;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToUmbracoPageResult"/> class.
        /// </summary>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            Key = publishedContent.Key;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedirectToUmbracoPageResult"/> class.
        /// </summary>
        public RedirectToUmbracoPageResult(IPublishedContent publishedContent, QueryString queryString, IPublishedUrlProvider publishedUrlProvider, IUmbracoContextAccessor umbracoContextAccessor)
        {
            _publishedContent = publishedContent;
            Key = publishedContent.Key;
            _queryString = queryString;
            _publishedUrlProvider = publishedUrlProvider;
            _umbracoContextAccessor = umbracoContextAccessor;
        }

        private string Url
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_url))
                {
                    return _url;
                }

                if (PublishedContent is null)
                {
                    throw new InvalidOperationException($"Cannot redirect, no entity was found for key {Key}");
                }

                var result = _publishedUrlProvider.GetUrl(PublishedContent.Id);

                if (result == "#")
                {
                    throw new InvalidOperationException(
                        $"Could not route to entity with key {Key}, the NiceUrlProvider could not generate a URL");
                }

                _url = result;

                return _url;
            }
        }

        public Guid Key { get; }

        private IPublishedContent PublishedContent
        {
            get
            {
                if (!(_publishedContent is null))
                {
                    return _publishedContent;
                }

                // need to get the URL for the page
                _publishedContent = _umbracoContextAccessor.GetRequiredUmbracoContext().Content.GetById(Key);

                return _publishedContent;
            }
        }

        /// <inheritdoc/>
        public Task ExecuteResultAsync(ActionContext context)
        {
            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            HttpContext httpContext = context.HttpContext;

            string destinationUrl;
            if (IsUrlAbsolute(Url))
            {
                destinationUrl = Url;
            }
            else
            {
                IHostingEnvironment ioHelper = httpContext.RequestServices.GetRequiredService<IHostingEnvironment>();
                destinationUrl = ioHelper.ToAbsolute(Url);
            }

            if (_queryString.HasValue)
            {
                destinationUrl += _queryString.ToUriComponent();
            }

            httpContext.Response.Redirect(destinationUrl);

            return Task.CompletedTask;
        }

        private static bool IsUrlAbsolute(string url) => url.Contains("//");
    }
}
