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
        // fixme - should we be able to specify ALL scope options?
        IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel);

        // creates a readonly unit of work
        // the readonly unit of work will not accept operations, and will auto-complete
        // of course it would be a bad idea to use it for anything else than reading
        // fixme - implement that one FOR REAL not as an extension method
        // the extension method was a ugly hack
        // or maybe we want a autocommit flag in the ctor?
        //IScopeUnitOfWork GetReadOnlyUnitOfWork();
        //IScopeUnitOfWork GetReadOnlyUnitOfWork(bool autocommit = false);
    }
}