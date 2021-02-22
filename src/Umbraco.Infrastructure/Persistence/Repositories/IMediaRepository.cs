using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IContentRepository<int, IMedia>, IReadRepository<Guid, IMedia>
    {
        IMedia GetMediaByPath(string mediaPath);
    }
}
