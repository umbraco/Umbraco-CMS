namespace Umbraco.Cms.Core.Services.Navigation;

public interface IPublishStatusManagementService
{
    Task InitializeAsync(CancellationToken cancellationToken);
    Task AddOrUpdateStatusAsync(Guid documentKey, CancellationToken cancellationToken);
    Task RemoveAsync(Guid documentKey, CancellationToken cancellationToken);
    Task AddOrUpdateStatusWithDescendantsAsync(Guid rootDocumentKey, CancellationToken cancellationToken);
}
