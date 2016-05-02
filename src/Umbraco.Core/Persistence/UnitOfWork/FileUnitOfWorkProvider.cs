namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="FileUnitOfWork"/>
    /// </summary>
    public class FileUnitOfWorkProvider : IUnitOfWorkProvider
    {
        private readonly RepositoryFactory _repositoryFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUnitOfWorkProvider"/> class with a repository factory.
        /// </summary>
        /// <param name="repositoryFactory">A repository factory.</param>
        public FileUnitOfWorkProvider(RepositoryFactory repositoryFactory)
        {
            Mandate.ParameterNotNull(repositoryFactory, nameof(repositoryFactory));
            _repositoryFactory = repositoryFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileUnitOfWorkProvider"/> class.
        /// </summary>
        /// <remarks>FOR UNIT TESTS ONLY</remarks>
        internal FileUnitOfWorkProvider()
        {
            // careful, _repositoryFactory remains null!
        }

        public IUnitOfWork CreateUnitOfWork()
        {
            return new FileUnitOfWork(_repositoryFactory);
        }
    }
}