using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IPublicAccessRepository : IReadWriteQueryRepository<Guid, PublicAccessEntry>
    { }
}
