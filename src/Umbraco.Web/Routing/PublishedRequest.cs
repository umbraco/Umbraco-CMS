using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Macros;

namespace Umbraco.Web.Routing
{
    /// <summary>
    /// Represents a request for one specified Umbraco IPublishedContent to be rendered
    /// by one specified template, using one specified Culture and RenderingEngine.
    /// </summary>
    public class PublishedRequest
    {
        private readonly IPublishedRouter _publishedRouter;

        private bool _readonly; // after prepared
        private bool _readonlyUri; // after preparing
        private Uri _uri; // clean uri, no virtual dir, no trailing slash nor .aspx, nothing
        private bool _is404;
        private DomainAndUri _domain;
        private CultureInfo _culture;
        private IPublishedContent _publishedContent;
        private IPublishedContent _initialPublishedContent; // found by finders before 404, redirects, etc
        private PublishedContentHashtableConverter _umbracoPage; // legacy

        /// <summary>
        /// Initializes a new instance of the <see cref="PublishedRequest"/> class.
        /// </summary>
        /// <param name="publishedRouter">The published router.</param>
        /// <param name="umbracoContext">The Umbraco context.</param>
        /// <param name="uri">The request <c>Uri</c>.</param>
        internal PublishedRequest(IPublishedRouter publishedRouter, UmbracoContext umbracoContext, Uri uri = null)
        {
            UmbracoContext = umbracoContext ?? throw new ArgumentNullException(nameof(umbracoContext));
            _publishedRouter = publishedRouter ?? throw new ArgumentNullException(nameof(publishedRouter));
            Uri = uri ?? umbracoContext.CleanedUmbracoUrl;
        }

        /// <summary>
        /// Gets the UmbracoContext.
        /// </summary>
        public UmbracoContext UmbracoContext { get; }

        /// <summary>
        /// Gets or sets the cleaned up Uri used for routing.
        /// </summary>
        /// <remarks>The cleaned up Uri has no virtual directory, no trailing slash, no .aspx extension, etc.</remarks>
        public Uri Uri
        {
            get => _uri;
            set
            {
                if (_readonlyUri)
                    throw new InvalidOperationException("Cannot modify Uri after Preparing has triggered.");
                _uri = value;
            }
        }

        // utility for ensuring it is ok to set some properties
        private void EnsureWriteable()
        {
            if (_readonly)
                throw new InvalidOperationException("Cannot modify a PublishedRequest once it is read-only.");
        }

        /// <summary>
        /// Prepares the request.
        /// </summary>
        public void Prepare()
        {
            _publishedRouter.PrepareRequest(this);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Umbraco Backoffice should ignore a collision for this request.
        /// </summary>
        public bool IgnorePublishedContentCollisions { get; set; }

        #region Events

        /// <summary>
        /// Triggers before the published content request is prepared.
        /// </summary>
        /// <remarks>When the event triggers, no preparation has been done. It is still possible to
        /// modify the request's Uri property, for example to restore its original, public-facing value
        /// that might have been modified by an in-between equipment such as a load-balancer.</remarks>
        public static event EventHandler<EventArgs> Preparing;

        /// <summary>
        /// Triggers once the published content request has been prepared, but before it is processed.
        /// </summary>
        /// <remarks>When the event triggers, preparation is done ie domain, culture, document, template,
        /// rendering engine, etc. have been setup. It is then possible to change anything, before
        /// the request is actually processed and rendered by Umbraco.</remarks>
        public static event EventHandler<EventArgs> Prepared;

        /// <summary>
        /// Triggers the Preparing event.
        /// </summary>
        internal void OnPreparing()
        {
            Preparing?.Invoke(this, EventArgs.Empty);
            _readonlyUri = true;
        }

        /// <summary>
        /// Triggers the Prepared event.
        /// </summary>
        internal void OnPrepared()
        {
            Prepared?.Invoke(this, EventArgs.Empty);

            if (HasPublishedContent == false)
                Is404 = true; // safety

            _readonly = true;
        }

        #endregion

        #region PublishedContent

        /// <summary>
        /// Gets or sets the requested content.
        /// </summary>
        /// <remarks>Setting the requested content clears <c>Template</c>.</remarks>
        public IPublishedContent PublishedContent
        {
            get { return _publishedContent; }
            set
            {
                EnsureWriteable();
                _publishedContent = value;
                IsInternalRedirectPublishedContent = false;
                TemplateModel = null;
            }
        }

        /// <summary>
        /// Sets the requested content, following an internal redirect.
        /// </summary>
        /// <param name="content">The requested content.</param>
        /// <remarks>Depending on <c>UmbracoSettings.InternalRedirectPreservesTemplate</c>, will
        /// preserve or reset the template, if any.</remarks>
        public void SetInternalRedirectPublishedContent(IPublishedContent content)
        {
            if (content == null) throw new ArgumentNullException(nameof(content));
            EnsureWriteable();

            // unless a template has been set already by the finder,
            // template should be null at that point.

            // IsInternalRedirect if IsInitial, or already IsInternalRedirect
            var isInternalRedirect = IsInitialPublishedContent || IsInternalRedirectPublishedContent;

            // redirecting to self
            if (content.Id == PublishedContent.Id) // neither can be null
            {
                // no need to set PublishedContent, we're done
                IsInternalRedirectPublishedContent = isInternalRedirect;
                return;
            }

            // else

            // save
            var template = TemplateModel;

            // set published content - this resets the template, and sets IsInternalRedirect to false
            PublishedContent = content;
            IsInternalRedirectPublishedContent = isInternalRedirect;

            // must restore the template if it's an internal redirect & the config option is set
            if (isInternalRedirect && Current.Configs.Settings().WebRouting.InternalRedirectPreservesTemplate)
            {
                // restore
                TemplateModel = template;
            }
        }

        /// <summary>
        /// Gets the initial requested content.
        /// </summary>
        /// <remarks>The initial requested content is the content that was found by the finders,
        /// before anything such as 404, redirect... took place.</remarks>
        public IPublishedContent InitialPublishedContent => _initialPublishedContent;

        /// <summary>
        /// Gets value indicating whether the current published content is the initial one.
        /// </summary>
        public bool IsInitialPublishedContent => _initialPublishedContent != null && _initialPublishedContent == _publishedContent;

        /// <summary>
        /// Indicates that the current PublishedContent is the initial one.
        /// </summary>
        public void SetIsInitialPublishedContent()
        {
            EnsureWriteable();

            // note: it can very well be null if the initial content was not found
            _initialPublishedContent = _publishedContent;
            IsInternalRedirectPublishedContent = false;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the current published content has been obtained
        /// from the initial published content following internal redirections exclusively.
        /// </summary>
        /// <remarks>Used by PublishedContentRequestEngine.FindTemplate() to figure out whether to
        /// apply the internal redirect or not, when content is not the initial content.</remarks>
        public bool IsInternalRedirectPublishedContent { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the content request has a content.
        /// </summary>
        public bool HasPublishedContent => PublishedContent != null;

        #endregion

        #region Template

        /// <summary>
        /// Gets or sets the template model to use to display the requested content.
        /// </summary>
        internal ITemplate TemplateModel { get; set; }

        /// <summary>
        /// Gets the alias of the template to use to display the requested content.
        /// </summary>
        public string TemplateAlias => TemplateModel?.Alias;

        /// <summary>
        /// Tries to set the template to use to display the requested content.
        /// </summary>
        /// <param name="alias">The alias of the template.</param>
        /// <returns>A value indicating whether a valid template with the specified alias was found.</returns>
        /// <remarks>
        /// <para>Successfully setting the template does refresh <c>RenderingEngine</c>.</para>
        /// <para>If setting the template fails, then the previous template (if any) remains in place.</para>
        /// </remarks>
        public bool TrySetTemplate(string alias)
        {
            EnsureWriteable();

            if (string.IsNullOrWhiteSpace(alias))
            {
                TemplateModel = null;
                return true;
            }

            // NOTE - can we still get it with whitespaces in it due to old legacy bugs?
            alias = alias.Replace(" ", "");

            var model = _publishedRouter.GetTemplate(alias);
            if (model == null)
                return false;

            TemplateModel = model;
            return true;
        }

        /// <summary>
        /// Sets the template to use to display the requested content.
        /// </summary>
        /// <param name="template">The template.</param>
        /// <remarks>Setting the template does refresh <c>RenderingEngine</c>.</remarks>
        public void SetTemplate(ITemplate template)
        {
            EnsureWriteable();
            TemplateModel = template;
        }

        /// <summary>
        /// Resets the template.
        /// </summary>
        public void ResetTemplate()
        {
            EnsureWriteable();
            TemplateModel = null;
        }

        /// <summary>
        /// Gets a value indicating whether the content request has a template.
        /// </summary>
        public bool HasTemplate => TemplateModel != null;

        internal void UpdateToNotFound()
        {
            var __readonly = _readonly;
            _readonly = false;
            _publishedRouter.UpdateRequestToNotFound(this);
            _readonly = __readonly;
        }

        #endregion

        #region Domain and Culture

        /// <summary>
        /// Gets or sets the content request's domain.
        /// </summary>
        /// <remarks>Is a DomainAndUri object ie a standard Domain plus the fully qualified uri. For example,
        /// the <c>Domain</c> may contain "example.com" whereas the <c>Uri</c> will be fully qualified eg "http://example.com/".</remarks>
        public DomainAndUri Domain
        {
            get { return _domain; }
            set
            {
                EnsureWriteable();
                _domain = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the content request has a domain.
        /// </summary>
        public bool HasDomain => Domain != null;

        /// <summary>
        /// Gets or sets the content request's culture.
        /// </summary>
        public CultureInfo Culture
        {
            get { return _culture ?? Thread.CurrentThread.CurrentCulture; }
            set
            {
                EnsureWriteable();
                _culture = value;
            }
        }

        // note: do we want to have an ordered list of alternate cultures,
        // to allow for fallbacks when doing dictionary lookup and such?

        #endregion

        #region Status

        /// <summary>
        /// Gets or sets a value indicating whether the requested content could not be found.
        /// </summary>
        /// <remarks>This is set in the <c>PublishedContentRequestBuilder</c> and can also be used in
        /// custom content finders or <c>Prepared</c> event handlers, where we want to allow developers
        /// to indicate a request is 404 but not to cancel it.</remarks>
        public bool Is404
        {
            get { return _is404; }
            set
            {
                EnsureWriteable();
                _is404 = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the content request triggers a redirect (permanent or not).
        /// </summary>
        public bool IsRedirect => string.IsNullOrWhiteSpace(RedirectUrl) == false;

        /// <summary>
        /// Gets or sets a value indicating whether the redirect is permanent.
        /// </summary>
        public bool IsRedirectPermanent { get; private set; }

        /// <summary>
        /// Gets or sets the URL to redirect to, when the content request triggers a redirect.
        /// </summary>
        public string RedirectUrl { get; private set; }

        /// <summary>
        /// Indicates that the content request should trigger a redirect (302).
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        public void SetRedirect(string url)
        {
            EnsureWriteable();
            RedirectUrl = url;
            IsRedirectPermanent = false;
        }

        /// <summary>
        /// Indicates that the content request should trigger a permanent redirect (301).
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        public void SetRedirectPermanent(string url)
        {
            EnsureWriteable();
            RedirectUrl = url;
            IsRedirectPermanent = true;
        }

        /// <summary>
        /// Indicates that the content request should trigger a redirect, with a specified status code.
        /// </summary>
        /// <param name="url">The URL to redirect to.</param>
        /// <param name="status">The status code (300-308).</param>
        /// <remarks>Does not actually perform a redirect, only registers that the response should
        /// redirect. Redirect will or will not take place in due time.</remarks>
        public void SetRedirect(string url, int status)
        {
            EnsureWriteable();

            if (status < 300 || status > 308)
                throw new ArgumentOutOfRangeException(nameof(status), "Valid redirection status codes 300-308.");

            RedirectUrl = url;
            IsRedirectPermanent = (status == 301 || status == 308);
            if (status != 301 && status != 302) // default redirect statuses
                ResponseStatusCode = status;
        }

        /// <summary>
        /// Gets or sets the content request http response status code.
        /// </summary>
        /// <remarks>Does not actually set the http response status code, only registers that the response
        /// should use the specified code. The code will or will not be used, in due time.</remarks>
        public int ResponseStatusCode { get; private set; }

        /// <summary>
        /// Gets or sets the content request http response status description.
        /// </summary>
        /// <remarks>Does not actually set the http response status description, only registers that the response
        /// should use the specified description. The description will or will not be used, in due time.</remarks>
        public string ResponseStatusDescription { get; private set; }

        /// <summary>
        /// Sets the http response status code, along with an optional associated description.
        /// </summary>
        /// <param name="code">The http status code.</param>
        /// <param name="description">The description.</param>
        /// <remarks>Does not actually set the http response status code and description, only registers that
        /// the response should use the specified code and description. The code and description will or will
        /// not be used, in due time.</remarks>
        public void SetResponseStatus(int code, string description = null)
        {
            EnsureWriteable();

            // .Status is deprecated
            // .SubStatusCode is IIS 7+ internal, ignore
            ResponseStatusCode = code;
            ResponseStatusDescription = description;
        }

        #endregion

        #region Response Cache

        /// <summary>
        /// Gets or sets the <c>System.Web.HttpCacheability</c>
        /// </summary>
        // Note: we used to set a default value here but that would then be the default
        // for ALL requests, we shouldn't overwrite it though if people are using [OutputCache] for example
        // see: https://our.umbraco.com/forum/using-umbraco-and-getting-started/79715-output-cache-in-umbraco-752
        public HttpCacheability Cacheability { get; set; }

        /// <summary>
        /// Gets or sets a list of Extensions to append to the Response.Cache object.
        /// </summary>
        public List<string> CacheExtensions { get; set; } = new List<string>();

        /// <summary>
        /// Gets or sets a dictionary of Headers to append to the Response object.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Legacy

        // for legacy/webforms/macro code -
        // TODO: get rid of it eventually
        internal PublishedContentHashtableConverter LegacyContentHashTable
        {
            get
            {
                if (_umbracoPage == null)
                    throw new InvalidOperationException("The UmbracoPage object has not been initialized yet.");

                return _umbracoPage;
            }
            set => _umbracoPage = value;
        }

        #endregion
    }
}
