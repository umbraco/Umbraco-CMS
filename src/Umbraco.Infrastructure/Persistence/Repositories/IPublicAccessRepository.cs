using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IPublicAccessRepository : IReadWriteQueryRepository<Guid, PublicAccessEntry>
    { }
}
