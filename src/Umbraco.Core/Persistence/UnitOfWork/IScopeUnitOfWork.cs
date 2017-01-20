using Umbraco.Core.Events;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IScopeUnitOfWork : IDatabaseUnitOfWork
    {
        IEventManager EventManager { get; }
    }
}