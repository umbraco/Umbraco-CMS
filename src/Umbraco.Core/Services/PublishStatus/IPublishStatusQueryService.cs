namespace Umbraco.Cms.Core.Services.Navigation;

/// <summary>
/// Verifies the published status of documents.
/// </summary>
/// <remarks>
/// This interface is kept for backward compatibility. Use <see cref="IDocumentPublishStatusQueryService"/> instead.
/// </remarks>
[Obsolete("Use IDocumentPublishStatusQueryService instead. Scheduled for removal in Umbraco 19.")]
public interface IPublishStatusQueryService : IDocumentPublishStatusQueryService;
