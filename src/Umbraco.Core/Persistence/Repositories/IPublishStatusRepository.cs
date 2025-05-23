namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IPublishStatusRepository
{
    Task<IDictionary<Guid, ISet<string>>> GetAllPublishStatusAsync(CancellationToken cancellationToken);
    Task<ISet<string>> GetPublishStatusAsync(Guid documentKey, CancellationToken cancellationToken);
    Task<IDictionary<Guid, ISet<string>>> GetDescendantsOrSelfPublishStatusAsync(Guid rootDocumentKey, CancellationToken cancellationToken);
}
