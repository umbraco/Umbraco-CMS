namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Provides management operations for the in-memory document publish status cache.
/// </summary>
/// <remarks>
/// This interface is kept for backward compatibility. Use <see cref="IDocumentPublishStatusManagementService"/> instead.
/// </remarks>
[Obsolete("Use IDocumentPublishStatusManagementService instead. Scheduled for removal in Umbraco 19.")]
public interface IPublishStatusManagementService : IDocumentPublishStatusManagementService;
