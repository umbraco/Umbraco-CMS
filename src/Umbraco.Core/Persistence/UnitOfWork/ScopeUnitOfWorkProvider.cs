using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public abstract class ScopeUnitOfWorkProvider : IScopeUnitOfWorkProvider
    {
        public IScopeProvider Provider { get; private set; }

        /// <summary>
        /// Constructor accepting a <see cref="IScopeProvider"/> instance
        /// </summary>
        /// <param name="scopeProvider"></param>
        protected ScopeUnitOfWorkProvider(IScopeProvider scopeProvider)
        {
            Mandate.ParameterNotNull(scopeProvider, "scopeProvider");
            Provider = scopeProvider;
        }

        //explicit implementation
        IDatabaseUnitOfWork IDatabaseUnitOfWorkProvider.GetUnitOfWork()
        {
            return new ScopeUnitOfWork(Provider);
        }

        /// <summary>
        /// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// </remarks>
        public virtual IScopeUnitOfWork GetUnitOfWork()
        {
            return new ScopeUnitOfWork(Provider);
        }

        /// <summary>
        /// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// </remarks>
        public IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel)
        {
            return new ScopeUnitOfWork(Provider, isolationLevel);
        }
    }
}