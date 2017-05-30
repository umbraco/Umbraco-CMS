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

        /// <inheritdoc />
        public virtual IScopeUnitOfWork GetUnitOfWork()
        {
            return new ScopeUnitOfWork(ScopeProvider);
        }

        /// <inheritdoc />
        public IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel)
        {
            return new ScopeUnitOfWork(ScopeProvider, isolationLevel);
        }

        /// <inheritdoc />
        public IScopeUnitOfWork GetUnitOfWork(bool readOnly)
        {
            return new ScopeUnitOfWork(ScopeProvider, readOnly: readOnly);
        }

        /// <inheritdoc />
        public IScopeUnitOfWork GetUnitOfWork(IsolationLevel isolationLevel, bool readOnly)
        {
            return new ScopeUnitOfWork(ScopeProvider, isolationLevel, readOnly: readOnly);
        }
    }
}