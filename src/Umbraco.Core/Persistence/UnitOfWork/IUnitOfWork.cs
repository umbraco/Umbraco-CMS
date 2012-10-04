using System;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        Database Storage { get; } //TODO consider replacing 'Database' with a datastorage adapter, so there is no direct dependency on PetaPoco
    }
}