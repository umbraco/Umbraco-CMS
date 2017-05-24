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
        new IScopeUnitOfWork GetUnitOfWork();

        // creates a unit of work
        // support specifying an isolation level
        // support auto-commit - but beware! it will be committed, whatever happens
        // TODO in v8 this should all be merged as one single method with optional args
        IScopeUnitOfWork GetUnitOfWork(bool readOnly);
        IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel, bool readOnly = false);
    }
}