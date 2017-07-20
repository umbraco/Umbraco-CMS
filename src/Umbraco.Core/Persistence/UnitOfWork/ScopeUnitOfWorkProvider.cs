using System;
using System.Data;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    public class ScopeUnitOfWorkProvider : IScopeUnitOfWorkProvider
    {
        private readonly RepositoryFactory _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeUnitOfWorkProvider"/> class.
        /// </summary>
        public ScopeUnitOfWorkProvider(IScopeProvider scopeProvider, IDatabaseContext databaseContext, RepositoryFactory repositoryFactory)
        {
            ScopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            DatabaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        }

        /// <inheritdoc />
        public IScopeProvider ScopeProvider { get; }

        /// <inheritdoc />
        public IDatabaseContext DatabaseContext { get; }

        /// <inheritdoc />
        public IScopeUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool readOnly = false, bool immediate = false)
        {
            return new ScopeUnitOfWork(ScopeProvider, DatabaseContext, _repositoryFactory, isolationLevel, readOnly, immediate);
        }
    }
}
