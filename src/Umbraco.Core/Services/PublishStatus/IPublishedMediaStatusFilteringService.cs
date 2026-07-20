namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides filtering operations specifically for published media content.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IPublishedStatusFilteringService"/> to provide
/// a media-specific filtering service that can be registered separately in the dependency injection container.
/// </remarks>
public interface IPublishedMediaStatusFilteringService : IPublishedStatusFilteringService
{
}
