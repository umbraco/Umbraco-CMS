using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Core.Cache;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.Repositories.Implement
{
    /// <summary>
    /// Represent an abstract Repository for NPoco based repositories
    /// </summary>
    public abstract class NPocoRepositoryBase<TId, TEntity> : EntityRepositoryBase<TId, TEntity>
        where TEntity : class, IEntity
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoRepositoryBase{TId, TEntity}"/> class.
        /// </summary>
        protected NPocoRepositoryBase(IScopeAccessor scopeAccessor, AppCaches cache, ILogger<NPocoRepositoryBase<TId, TEntity>> logger)
            : base(scopeAccessor, cache, logger)
        { }
    }
}
