using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IDocumentUrlRepository
{
    void Save(IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments);
    IEnumerable<PublishedDocumentUrlSegment> GetAll();
    void DeleteByDocumentKey(IEnumerable<Guid> select);
}
