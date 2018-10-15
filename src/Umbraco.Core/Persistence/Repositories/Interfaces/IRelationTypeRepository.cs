﻿using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IRelationTypeRepository : IRepositoryQueryable<int, IRelationType>, IReadRepository<Guid, IRelationType>
    { }
}