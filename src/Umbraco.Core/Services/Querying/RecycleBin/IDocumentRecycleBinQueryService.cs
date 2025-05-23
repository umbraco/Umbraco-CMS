using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

public interface IDocumentRecycleBinQueryService
{
    Task<Attempt<IDocumentEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedDocumentId);
}
