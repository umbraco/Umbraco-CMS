using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;

namespace Umbraco.Web.Routing
{
    public class PublishedRequestBuilder : IPublishedRequestBuilder
    {
        private readonly IFileService _fileService;
        private IReadOnlyDictionary<string, string> _headers;
        private bool _cacheability;
        private IReadOnlyList<string> _cacheExtensions;
        private IPublishedContent _internalRedirectContent;
        private string _redirectUrl;
        private HttpStatusCode _responseStatus = HttpStatusCode.NotFound;
        private string _responseDesc;

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRequestBuilder"/> class.
        /// </summary>
        public PublishedRequestBuilder(IFileService fileService)
        {
            _fileService = fileService;
        }

        /// <inheritdoc/>
        public Uri Uri { get; private set; }

        /// <inheritdoc/>
        public DomainAndUri Domain { get; private set; }

        /// <inheritdoc/>
        public CultureInfo Culture { get; private set; }

        /// <inheritdoc/>
        public ITemplate Template { get; private set; }

        /// <inheritdoc/>
        public bool IsInternalRedirectPublishedContent { get; private set; } // TODO: Not sure what this is yet

        /// <inheritdoc/>
        public int ResponseStatusCode => (int)_responseStatus;

        /// <inheritdoc/>
        public IPublishedContent PublishedContent { get; private set; }

        /// <inheritdoc/>
        public IPublishedRequest Build() => new PublishedRequest(
                Uri,
                PublishedContent,
                IsInternalRedirectPublishedContent,
                Template,
                Domain,
                Culture,
                _redirectUrl,
                (int)_responseStatus,
                _responseDesc,
                _cacheExtensions,
                _headers,
                _cacheability);

        /// <inheritdoc/>
        public IPublishedRequestBuilder ResetTemplate()
        {
            Template = null;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetCacheabilityNoCache(bool cacheability)
        {
            _cacheability = cacheability;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetCacheExtensions(IEnumerable<string> cacheExtensions)
        {
            _cacheExtensions = cacheExtensions.ToList();
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetCulture(CultureInfo culture)
        {
            Culture = culture;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetDomain(DomainAndUri domain)
        {
            Domain = domain;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetHeaders(IReadOnlyDictionary<string, string> headers)
        {
            _headers = headers;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetInternalRedirectPublishedContent(IPublishedContent content)
        {
            _internalRedirectContent = content;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetIs404(bool is404)
        {
            _responseStatus = HttpStatusCode.NotFound;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetPublishedContent(IPublishedContent content)
        {
            PublishedContent = content;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetRedirect(string url, int status = (int)HttpStatusCode.Redirect)
        {
            _redirectUrl = url;
            _responseStatus = (HttpStatusCode)status;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetRedirectPermanent(string url)
        {
            _redirectUrl = url;
            _responseStatus = HttpStatusCode.Moved;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetResponseStatus(int code, string description = null)
        {
            _responseStatus = (HttpStatusCode)code;
            _responseDesc = description;
            return this;
        }

        /// <inheritdoc/>
        public IPublishedRequestBuilder SetTemplate(ITemplate template)
        {
            Template = template;
            return this;
        }

        /// <inheritdoc/>
        public bool TrySetTemplate(string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
            {
                Template = null;
                return true;
            }

            // NOTE - can we still get it with whitespaces in it due to old legacy bugs?
            alias = alias.Replace(" ", "");

            ITemplate model = _fileService.GetTemplate(alias);
            if (model == null)
            {
                return false;
            }

            Template = model;
            return true;
        }
    }
}
