using System;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IContentRepository : IRepositoryQueryable<int, IContent>
    {
        IEnumerable<IContent> GetAllVersions(int id);
        IContent GetByVersion(int id, Guid versionId);
    }
}