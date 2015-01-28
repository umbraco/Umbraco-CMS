using System;
using System.Collections;
using System.Collections.Generic;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IPublicAccessRepository : IRepositoryQueryable<Guid, PublicAccessEntry>
    {
    }
}