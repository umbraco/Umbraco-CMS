using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMediaRepository : IRepositoryVersionable<int, IMedia>, IRecycleBinRepository<IMedia>, IReadRepository<Guid, IMedia>
    {
        IMedia GetMediaByPath(string mediaPath);
    }
}
