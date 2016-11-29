namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a provider that can create units of work to work on databases.
    /// </summary>
    public interface IDatabaseUnitOfWorkProvider
	{
        /// <summary>
        /// Gets the database context.
        /// </summary>
        DatabaseContext DatabaseContext { get; }

        /// <summary>
        /// Creates a unit of work.
        /// </summary>
        /// <returns>A new unit of work.</returns>
		IDatabaseUnitOfWork CreateUnitOfWork();
	}
}