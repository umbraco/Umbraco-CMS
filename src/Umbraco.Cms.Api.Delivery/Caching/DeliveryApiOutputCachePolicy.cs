using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Caching;

internal sealed class DeliveryApiOutputCachePolicy : IOutputCachePolicy
{
    private readonly TimeSpan _duration;
    private readonly StringValues _varyByHeaderNames;

    [Obsolete("Please use the constructor with all parameters. Scheduled for removal in Umbraco 18.")]
    public DeliveryApiOutputCachePolicy(TimeSpan duration)
        : this(duration, StringValues.Empty)
        => _duration = duration;

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

        context.EnableOutputCaching = requestPreviewService.IsPreview() is false;
        context.ResponseExpirationTimeSpan = _duration;
        context.CacheVaryByRules.HeaderNames = _varyByHeaderNames;

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
