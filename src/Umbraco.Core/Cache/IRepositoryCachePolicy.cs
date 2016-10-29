using System;
using System.Collections.Generic;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Cache
{
    internal interface IRepositoryCachePolicy<TEntity, TId> : IDisposable
        where TEntity : class, IAggregateRoot
    {
        TEntity Get(TId id, Func<TId, TEntity> getFromRepo);
        TEntity Get(TId id);
        bool Exists(TId id, Func<TId, bool> getFromRepo);
        
        void CreateOrUpdate(TEntity entity, Action<TEntity> persistMethod);
        void Remove(TEntity entity, Action<TEntity> persistMethod);
        TEntity[] GetAll(TId[] ids, Func<TId[], IEnumerable<TEntity>> getFromRepo);
    }
}