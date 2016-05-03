namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a persistence unit of work for working with files.
    /// </summary>
    /// <remarks>The FileUnitOfWork does *not* implement transactions, so although it needs to be flushed or
    /// completed for operations to be executed, they are executed immediately without any commit or roll back
    /// mechanism.</remarks>
    internal class FileUnitOfWork : UnitOfWorkBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWork"/> class with a a repository factory.
        /// </summary>
        /// <param name="factory">A repository factory.</param>
        /// <remarks>This should be used by the FileUnitOfWorkProvider exclusively.</remarks>
        public FileUnitOfWork(RepositoryFactory factory)
            : base(factory)
        { }

        /// <summary>
        /// Creates a repository.
        /// </summary>
        /// <typeparam name="TRepository">The type of the repository.</typeparam>
        /// <param name="name">The optional name of the repository.</param>
        /// <returns>The created repository for the unit of work.</returns>
	    public override TRepository CreateRepository<TRepository>(string name = null)
        {
            return Factory.CreateRepository<TRepository>(this, name);
        }
    }
}