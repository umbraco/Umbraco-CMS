using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Provides scoped units of work.
    /// </summary>
    public interface IScopeUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        // gets the scope provider
        IScopeProvider ScopeProvider { get; }

        // creates a unit of work
        // redefine the method to indicate it returns an IScopeUnitOfWork and
        // not anymore only an IDatabaseUnitOfWork as IDatabaseUnitOfWorkProvider does
        // fixme - merge IScope... and IDatabase...
        new IScopeUnitOfWork CreateUnitOfWork();

        // creates a unit of work
        // support specifying an isolation level
        // support auto-commit - but beware! it will be committed, whatever happens
        IScopeUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool readOnly = false, bool immediate = false);

        // fixme explain
        IDatabaseContext DatabaseContext { get; }
    }
}