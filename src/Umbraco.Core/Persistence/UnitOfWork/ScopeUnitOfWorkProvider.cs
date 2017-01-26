using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public abstract class ScopeUnitOfWorkProvider : IScopeUnitOfWorkProvider
    {
        /// <summary>
        /// Constructor accepting a <see cref="IScopeProvider"/> instance
        /// </summary>
        /// <param name="scopeProvider"></param>
        protected ScopeUnitOfWorkProvider(IScopeProvider scopeProvider)
        {
            Mandate.ParameterNotNull(scopeProvider, "scopeProvider");
            ScopeProvider = scopeProvider;
        }

        /// <inheritdoc />
        public IScopeProvider ScopeProvider { get; private set; }

        // explicit implementation
        IDatabaseUnitOfWork IDatabaseUnitOfWorkProvider.GetUnitOfWork()
        {
            return new ScopeUnitOfWork(ScopeProvider);
        }

        /// <summary>
        /// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// fixme NO we dispose of it when the transaction completes
        /// </remarks>
        public virtual IScopeUnitOfWork GetUnitOfWork()
        {
            return new ScopeUnitOfWork(ScopeProvider);
        }

        /// <summary>
        /// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// fixme NO
        /// </remarks>
        public IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel)
        {
            return new ScopeUnitOfWork(ScopeProvider, isolationLevel);
        }
    }
}