namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

/// <summary>
/// Provides functionality to determine the cache duration for news items displayed on the news dashboard.
/// </summary>
public class NewsCacheDurationProvider : INewsCacheDurationProvider
{
    /// <inheritdoc />
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}
