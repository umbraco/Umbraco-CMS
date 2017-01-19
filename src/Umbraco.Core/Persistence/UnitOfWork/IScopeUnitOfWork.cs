using Umbraco.Core.Events;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal interface IScopeUnitOfWork : IDatabaseUnitOfWork
    {
        IEventManager EventManager { get; }
    }
}