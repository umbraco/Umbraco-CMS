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
        public ScopeUnitOfWorkProvider(IScopeProvider scopeProvider, ISqlContext sqlContext, RepositoryFactory repositoryFactory)
        {
            ScopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            SqlContext = sqlContext ?? throw new ArgumentNullException(nameof(sqlContext));
            _repositoryFactory = repositoryFactory ?? throw new ArgumentNullException(nameof(repositoryFactory));
        }

        /// <inheritdoc />
        public IScopeProvider ScopeProvider { get; }

        /// <inheritdoc />
        public ISqlContext SqlContext { get; }

        /// <inheritdoc />
        public IScopeUnitOfWork CreateUnitOfWork(IsolationLevel isolationLevel = IsolationLevel.Unspecified, bool readOnly = false, bool immediate = false)
        {
            return new ScopeUnitOfWork(ScopeProvider, SqlContext, _repositoryFactory, isolationLevel, readOnly, immediate);
        }
    }
}
