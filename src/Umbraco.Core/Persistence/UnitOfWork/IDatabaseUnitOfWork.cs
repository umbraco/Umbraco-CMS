namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Represents a persistence unit of work for working with a database.
	/// </summary>
	public interface IDatabaseUnitOfWork : IUnitOfWork
	{
		UmbracoDatabase Database { get; }

	    void ReadLockNodes(params int[] lockIds);
	    void WriteLockNodes(params int[] lockIds);
    }
}