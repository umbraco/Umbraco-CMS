using Microsoft.AspNetCore.Http;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Common.Accessors;

/// <summary>
///     Provides access to the <see cref="IOutputExpansionStrategy"/> for the current HTTP request context.
/// </summary>
public sealed class RequestContextOutputExpansionStrategyAccessor : RequestContextServiceAccessorBase<IOutputExpansionStrategy>, IOutputExpansionStrategyAccessor
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="RequestContextOutputExpansionStrategyAccessor"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    public RequestContextOutputExpansionStrategyAccessor(IHttpContextAccessor httpContextAccessor)
        : base(httpContextAccessor)
    {
    }
}
