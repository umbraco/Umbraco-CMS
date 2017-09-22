using Umbraco.Core.Events;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IScopeUnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// Gets the scope.
        /// </summary>
        IScope Scope { get; }

        /// <summary>
        ///  Gets the event messages.
        /// </summary>
        EventMessages Messages { get; }

        /// <summary>
        /// Gets the events dispatcher.
        /// </summary>
        IEventDispatcher Events { get; }

        /// <summary>
        /// Gets the database.
        /// </summary>
        IUmbracoDatabase Database { get; }

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        ISqlContext SqlContext { get; }

        /// <summary>
        /// Read-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
        void ReadLock(params int[] lockIds);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
        void WriteLock(params int[] lockIds);
    }
}
