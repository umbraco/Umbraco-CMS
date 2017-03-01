using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a file unit of work
    /// </summary>
    public class FileUnitOfWorkProvider : ScopeUnitOfWorkProvider // fixme - what's the point?
    {
        [Obsolete("Use the ctor specifying a IScopeProvider instead")]
        public FileUnitOfWorkProvider()
            : this(new ScopeProvider(new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, LoggerResolver.Current.Logger)))
        { }

        public FileUnitOfWorkProvider(IScopeProvider scopeProvider)
         : base(scopeProvider)
        { }

        // fixme - returning a IScopeUnitOfWork instead of a IUnitOfWork here is a breaking change!
        // this is bad, we're going to end up creating // units of work which is ... really bad
        public override IScopeUnitOfWork GetUnitOfWork()
        {
            // fixme - no point returning a FileUnitOfWork if its equivalent to ScopeUnitOfWork
            return new ScopeUnitOfWork(ScopeProvider);
        }
    }
}