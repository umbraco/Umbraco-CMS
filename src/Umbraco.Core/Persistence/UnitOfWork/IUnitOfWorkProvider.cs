namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
    /// Represents a provider that can create units of work.
    /// </summary>
    public interface IUnitOfWorkProvider
    {
        /// <summary>
        /// Creates a unit of work.
        /// </summary>
        /// <returns>A new unit of work.</returns>
        IUnitOfWork CreateUnitOfWork();
    }
}