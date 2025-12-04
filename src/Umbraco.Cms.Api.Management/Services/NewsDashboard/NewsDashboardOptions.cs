namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

/// <summary>
/// Options for the news dashboard service.
/// </summary>
public class NewsDashboardOptions
{
    /// <summary>
    /// Gets the cache duration for news dashboard items.
    /// </summary>
    public virtual TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}
