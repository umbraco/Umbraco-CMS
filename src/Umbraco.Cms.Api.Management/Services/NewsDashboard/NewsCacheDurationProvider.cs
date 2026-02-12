namespace Umbraco.Cms.Api.Management.Services.NewsDashboard;

public class NewsCacheDurationProvider : INewsCacheDurationProvider
{
    /// <inheritdoc />
    public TimeSpan CacheDuration => TimeSpan.FromMinutes(30);
}
