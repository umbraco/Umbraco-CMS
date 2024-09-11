using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Cms.Core.Services.Querying.RecycleBin;

public interface IMediaRecycleBinQueryService
{
    Task<Attempt<IMediaEntitySlim?, RecycleBinQueryResultType>> GetOriginalParentAsync(Guid trashedMediaId);
}
