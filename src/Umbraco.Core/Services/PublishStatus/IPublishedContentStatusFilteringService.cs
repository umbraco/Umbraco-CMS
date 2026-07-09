namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides filtering operations specifically for published document content.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IPublishedStatusFilteringService"/> to provide
/// a content-specific filtering service that considers publish status, culture, and preview mode.
/// </remarks>
public interface IPublishedContentStatusFilteringService : IPublishedStatusFilteringService
{
}
