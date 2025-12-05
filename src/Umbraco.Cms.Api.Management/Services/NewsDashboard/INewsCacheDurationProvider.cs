namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

/// <summary>
/// Cache duration provider for the news dashboard service.
/// </summary>
public interface INewsCacheDurationProvider
{
    /// <summary>
    /// Gets the cache duration for news dashboard items.
    /// </summary>
    TimeSpan CacheDuration { get; }
}
