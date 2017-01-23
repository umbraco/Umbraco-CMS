using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="FileUnitOfWork"/>
    /// </summary>
    public class FileUnitOfWorkProvider : ScopeUnitOfWorkProvider
    {
        public FileUnitOfWorkProvider()
            : this(new ScopeProvider(new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, LoggerResolver.Current.Logger)))
        {
        }

        public FileUnitOfWorkProvider(IScopeProvider scopeProvider) : base(scopeProvider)
        {
        }

        public override IScopeUnitOfWork GetUnitOfWork()
        {
            return new FileUnitOfWork(Provider);
        }

    }
}