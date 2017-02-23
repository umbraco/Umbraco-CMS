using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationRepository : IRepositoryQueryable<int, IRelation>, IReadRepository<Guid, IRelation>
    {

    }
}