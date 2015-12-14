﻿using System;
using Umbraco.Core.Models;

namespace Umbraco.Core.Persistence.Repositories
{
    public interface IMemberTypeRepository : IRepositoryQueryable<int, IMemberType>, IReadRepository<Guid, IMemberType>
    {
         
    }
}