using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Provides scoped units of work.
    /// </summary>
    public interface IScopeUnitOfWorkProvider
    {
        /// <summary>
        /// Gets the scope provider.
        /// </summary>
        IScopeProvider ScopeProvider { get; }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        IDatabaseContext DatabaseContext { get; }

        /// <summary>
        /// Creates a unit of work.
        /// </summary>
        /// <param name="isolationLevel">An optional isolation level.</param>
        /// <param name="readOnly">A value indicating whether the unit of work is read-only.</param>
        /// <param name="immediate">A value indicating whether the unit of work is immediate.</param>
        /// <returns>A new unit of work.</returns>
        /// <remarks>
        /// <para>A read-only unit of work does not need to be completed, and should not be used to write.</para>
        /// <para>An immediate unit of work does not queue operations but executes them immediately.</para>
        /// </remarks>
        IScopeUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool readOnly = false, bool immediate = false);
    }
}
