﻿using Umbraco.Core.Cache;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Web.Models.PublishedContent
{
    /// <summary>
    /// Implements <see cref="IVariationContextAccessor"/> on top of <see cref="IHttpContextAccessor"/>.
    /// </summary>
    public class HttpContextVariationContextAccessor : IVariationContextAccessor
    {
        private readonly IRequestCache _requestCache;
        private const string ContextKey = "Umbraco.Web.Models.PublishedContent.DefaultVariationContextAccessor";

        /// <summary>
        /// Initializes a new instance of the <see cref="HttpContextVariationContextAccessor"/> class.
        /// </summary>
        public HttpContextVariationContextAccessor(IRequestCache requestCache)
        {
            _requestCache = requestCache;
        }

        /// <inheritdoc />
        public VariationContext VariationContext
        {
            get => (VariationContext) _requestCache.Get(ContextKey);
            set => _requestCache.Set(ContextKey, value);
        }
    }
}
