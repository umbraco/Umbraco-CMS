using Microsoft.AspNetCore.OutputCaching;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DeliveryApi;

namespace Umbraco.Cms.Api.Delivery.Caching;

internal sealed class DeliveryApiOutputCachePolicy : IOutputCachePolicy
{
    private readonly TimeSpan _duration;

    public DeliveryApiOutputCachePolicy(TimeSpan duration)
        => _duration = duration;

    ValueTask IOutputCachePolicy.CacheRequestAsync(OutputCacheContext context, CancellationToken cancellationToken)
    {
        IRequestPreviewService requestPreviewService = context
            .HttpContext
            .RequestServices
            .GetRequiredService<IRequestPreviewService>();

        context.EnableOutputCaching = requestPreviewService.IsPreview() is false;
        context.ResponseExpirationTimeSpan = _duration;

        return ValueTask.CompletedTask;
    }

    ValueTask IOutputCachePolicy.ServeFromCacheAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;

    ValueTask IOutputCachePolicy.ServeResponseAsync(OutputCacheContext context, CancellationToken cancellationToken)
        => ValueTask.CompletedTask;
}
