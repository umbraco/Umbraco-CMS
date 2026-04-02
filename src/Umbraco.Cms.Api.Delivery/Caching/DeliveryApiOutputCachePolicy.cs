using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Caching;

internal sealed class DeliveryApiOutputCachePolicy : IOutputCachePolicy
{
    private readonly TimeSpan _duration;
    private readonly StringValues _varyByHeaderNames;

    public DeliveryApiOutputCachePolicy(TimeSpan duration, StringValues varyByHeaderNames)
    {
        _duration = duration;
        _varyByHeaderNames = varyByHeaderNames;
    }

    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        IRequestPreviewService requestPreviewService = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IRequestPreviewService>();

        IApiAccessService apiAccessService = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IApiAccessService>();

        context.EnableOutputCaching = requestPreviewService.IsPreview() is false && apiAccessService.HasPublicAccess();
        context.ResponseExpirationTimeSpan = _duration;
        context.CacheVaryByRules.HeaderNames = _varyByHeaderNames;

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
