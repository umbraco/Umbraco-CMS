using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Security;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Cms.Infrastructure.Persistence.Factories;
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

        IEnumerable<Guid> documentKeys = publishedDocumentUrlSegments.Select(x => x.DocumentKey).Distinct();

        Dictionary<(Guid UniqueId, int LanguageId), DocumentUrlDto> dtoDictionary = publishedDocumentUrlSegments.Select(BuildDto).ToDictionary(x=> (x.UniqueId, x.LanguageId));

        var toUpdate = new List<DocumentUrlDto>();
        var toDelete = new List<int>();
        var toInsert = dtoDictionary.Values.ToDictionary(x => (x.UniqueId, x.LanguageId));

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

                if (dtoDictionary.TryGetValue((existing.UniqueId, existing.LanguageId), out DocumentUrlDto? found))
                {
                    found.NodeId = existing.NodeId;

                    // Only update if the url segment is different
                    if (found.UrlSegment != existing.UrlSegment)
                    {
                        toUpdate.Add(found);
                    }

                    // if it's an update then it's not an insert
                    toInsert.Remove((found.UniqueId, found.LanguageId));
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

        if (toUpdate.Any())
        {
            var updater = Database.UpdateMany<DocumentUrlDto>();
            updater.OnlyFields(x=>x.UrlSegment);

            foreach (DocumentUrlDto updated in toUpdate)
            {
                updater.Execute(updated);
            }
        }

        Database.InsertBulk(toInsert.Values);
    }

    public IEnumerable<PublishedDocumentUrlSegment> GetAll()
    {
        List<DocumentUrlDto>? dtos = Database.Fetch<DocumentUrlDto>(Database.SqlContext.Sql().Select<DocumentUrlDto>().From<DocumentUrlDto>());

        return dtos.Select(BuildModel);
    }

    private PublishedDocumentUrlSegment BuildModel(DocumentUrlDto dto) =>
        new()
        {
            UrlSegment = dto.UrlSegment,
            DocumentKey = dto.UniqueId,
            LanguageId = dto.LanguageId,
        };

    private DocumentUrlDto BuildDto(PublishedDocumentUrlSegment model)
    {
        return new DocumentUrlDto()
        {
            UrlSegment = model.UrlSegment,
            UniqueId = model.DocumentKey,
            LanguageId = model.LanguageId
        };
    }
}
