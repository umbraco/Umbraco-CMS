using Umbraco.Core.Events;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IScopeUnitOfWork : IDatabaseUnitOfWork
    {
        IScope Scope { get; }
        EventMessages Messages { get; }
        IEventDispatcher Events { get; }
        void Flush();
    }
}