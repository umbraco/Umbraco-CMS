using System;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IUnitOfWork<T> : IDisposable
    {
        void Commit();
        T Storage { get; }
    }
}