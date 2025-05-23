using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Scoping;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Persistence.Repositories.Implement;

public class DocumentUrlRepository : IDocumentUrlRepository
{
    private readonly IScopeAccessor _scopeAccessor;

    public DocumentUrlRepository(IScopeAccessor scopeAccessor) => _scopeAccessor = scopeAccessor;

    private IUmbracoDatabase Database
    {
        get
        {
            if (_scopeAccessor.AmbientScope is null)
            {
                throw new NotSupportedException("Need to be executed in a scope");
            }

            return _scopeAccessor.AmbientScope.Database;
        }
    }

    public void Save(IEnumerable<PublishedDocumentUrlSegment> publishedDocumentUrlSegments)
    {
        // TODO: avoid this is called as first thing on first restart after install
        IEnumerable<Guid> documentKeys = publishedDocumentUrlSegments.Select(x => x.DocumentKey).Distinct();

        Dictionary<(Guid UniqueId, int LanguageId, bool isDraft, string urlSegment), DocumentUrlDto> dtoDictionary = publishedDocumentUrlSegments
            .Select(BuildDto)
            .ToDictionary(x => (x.UniqueId, x.LanguageId, x.IsDraft, x.UrlSegment));

        var toDelete = new List<int>();
        var toInsert = dtoDictionary.Values.ToDictionary(x => (x.UniqueId, x.LanguageId, x.IsDraft, x.UrlSegment));

        foreach (IEnumerable<Guid> group in documentKeys.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Sql<ISqlContext> sql = Database.SqlContext.Sql()
                .Select<DocumentUrlDto>()
                .From<DocumentUrlDto>()
                .Where<DocumentUrlDto>(x => group.Contains(x.UniqueId))
                .ForUpdate();

            List<DocumentUrlDto> existingUrlsInBatch = Database.Fetch<DocumentUrlDto>(sql);

            foreach (DocumentUrlDto existing in existingUrlsInBatch)
            {
                if (dtoDictionary.TryGetValue((existing.UniqueId, existing.LanguageId, existing.IsDraft, existing.UrlSegment), out DocumentUrlDto? found))
                {
                    found.NodeId = existing.NodeId;

                    // If we found it, we know we should not insert it as a new record.
                    toInsert.Remove((found.UniqueId, found.LanguageId, found.IsDraft, found.UrlSegment));
                }
                else
                {
                    toDelete.Add(existing.NodeId);
                }
            }
        }

        // do the deletes, updates and inserts
        if (toDelete.Count > 0)
        {
            Database.DeleteMany<DocumentUrlDto>().Where(x => toDelete.Contains(x.NodeId)).Execute();
        }

        Database.InsertBulk(toInsert.Values);
    }

    public IEnumerable<PublishedDocumentUrlSegment> GetAll()
    {
        List<DocumentUrlDto>? dtos = Database.Fetch<DocumentUrlDto>(Database.SqlContext.Sql().Select<DocumentUrlDto>().From<DocumentUrlDto>());

        return dtos.Select(BuildModel);
    }

    public void DeleteByDocumentKey(IEnumerable<Guid> documentKeys)
    {
        foreach (IEnumerable<Guid> group in documentKeys.InGroupsOf(Constants.Sql.MaxParameterCount))
        {
            Database.Execute(Database.SqlContext.Sql().Delete<DocumentUrlDto>().WhereIn<DocumentUrlDto>(x => x.UniqueId, group));
        }
    }

    private PublishedDocumentUrlSegment BuildModel(DocumentUrlDto dto) =>
        new()
        {
            UrlSegment = dto.UrlSegment,
            DocumentKey = dto.UniqueId,
            LanguageId = dto.LanguageId,
            IsDraft = dto.IsDraft,
            IsPrimary = dto.IsPrimary
        };

    private DocumentUrlDto BuildDto(PublishedDocumentUrlSegment model)
    {
        return new DocumentUrlDto()
        {
            UrlSegment = model.UrlSegment,
            UniqueId = model.DocumentKey,
            LanguageId = model.LanguageId,
            IsDraft = model.IsDraft,
            IsPrimary = model.IsPrimary,
        };
    }
}
