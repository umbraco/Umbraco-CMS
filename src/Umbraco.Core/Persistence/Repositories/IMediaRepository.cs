using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository2 : IMediaRepository, IContentRepository2<int, IMedia>
    {

    }
    public interface IMediaRepository : IContentRepository<int, IMedia>, IReadRepository<Guid, IMedia>
    {
        IMedia GetMediaByPath(string mediaPath);
    }
}
