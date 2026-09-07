namespace Umbraco.Cms.Search.Core.Cache;

/// <summary>
/// Wraps a batch of cache refresher payloads with the originating server, so distributed handlers can tell whether a notification originated locally.
/// </summary>
/// <typeparam name="TPayload">The type of the individual payload entries.</typeparam>
/// <param name="Payloads">The individual payload entries.</param>
/// <param name="Origin">An identifier for the server that raised the notification.</param>
internal record ContentCacheRefresherNotificationPayload<TPayload>(TPayload[] Payloads, string Origin)
{
}
