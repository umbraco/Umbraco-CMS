using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Persistence.Repositories;

public interface IMediaRepository : IContentRepository<int, IMedia>, IReadRepository<Guid, IMedia>
{
    IMedia? GetMediaByPath(string mediaPath);

    bool RecycleBinSmells();
}
