using Umbraco.Core.Events;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IScopeUnitOfWork : IDatabaseUnitOfWork
    {
        EventMessages Messages { get; }

        IEventManager EventManager { get; }
    }
}