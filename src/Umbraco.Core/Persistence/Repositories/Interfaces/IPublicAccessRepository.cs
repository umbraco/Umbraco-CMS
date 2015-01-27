using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IPublicAccessRepository : IRepositoryQueryable<Guid, PublicAccessEntry>
    {

    }
}