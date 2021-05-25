using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IContentRepository<int, IMedia>, IReadRepository<Guid, IMedia>
    {
        IMedia GetMediaByPath(string mediaPath);
        bool RecycleBinSmells();
    }
}
