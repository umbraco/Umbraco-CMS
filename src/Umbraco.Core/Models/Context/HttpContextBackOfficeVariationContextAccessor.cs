using Umbraco.Cms.Core.Cache;

namespace Umbraco.Cms.Core.Models.Context;

/// <summary>
///     Implements <see cref="IBackOfficeVariationContextAccessor" /> on top of <see cref="IRequestCache" />.
/// </summary>
public class HttpContextBackOfficeVariationContextAccessor : IBackOfficeVariationContextAccessor
{
    private const string ContextKey = "Umbraco.Web.Models.PublishedContent.HttpContextBackOfficeVariationContextAccessor";
    private readonly IRequestCache _requestCache;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HttpContextBackOfficeVariationContextAccessor" /> class.
    /// </summary>
    public HttpContextBackOfficeVariationContextAccessor(IRequestCache requestCache)
        => _requestCache = requestCache;

    /// <inheritdoc />
    public BackOfficeVariationContext? VariationContext
    {
        get => (BackOfficeVariationContext?)_requestCache.Get(ContextKey);
        set => _requestCache.Set(ContextKey, value);
    }
}
