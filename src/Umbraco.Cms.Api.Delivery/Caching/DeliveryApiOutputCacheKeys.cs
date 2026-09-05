namespace Umbraco.Cms.Api.Delivery.Caching;

/// <summary>
///     Keys used to pass resolved content and media items from controllers to the output cache policy
///     via <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>.
/// </summary>
internal static class DeliveryApiOutputCacheKeys
{
    /// <summary>
    ///     Key for storing resolved content items in <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>.
    /// </summary>
    public const string ResolvedContentItemsKey = "Umbraco.DeliveryApi.OutputCache.ResolvedContentItems";

    /// <summary>
    ///     Key for storing resolved media items in <see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/>.
    /// </summary>
    public const string ResolvedMediaItemsKey = "Umbraco.DeliveryApi.OutputCache.ResolvedMediaItems";
}
