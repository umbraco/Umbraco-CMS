namespace Umbraco.Cms.Search.Core.Cache;

internal record ContentCacheRefresherNotificationPayload<TPayload>(TPayload[] Payloads, string Origin)
{
}
