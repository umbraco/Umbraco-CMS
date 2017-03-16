using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    // note: the concept of "file unit of work" does not make sense anymore in v7.6
    // and we should probably remove this class, which is not used anywhere, but for
    // the time being we keep it here - just not to break too many things.

    /// <summary>
    /// Represents a Unit of Work Provider for creating a file unit of work
    /// </summary>
    [Obsolete("Use the ScopeUnitOfWorkProvider instead.", false)]
    public class FileUnitOfWorkProvider : ScopeUnitOfWorkProvider
    {
        [Obsolete("Use the ctor specifying a IScopeProvider instead")]
        public FileUnitOfWorkProvider()
            : this(new ScopeProvider(new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, LoggerResolver.Current.Logger)))
        { }

        public FileUnitOfWorkProvider(IScopeProvider scopeProvider)
         : base(scopeProvider)
        { }

        public override IScopeUnitOfWork GetUnitOfWork()
        {
            return new ScopeUnitOfWork(ScopeProvider);
        }
    }
}