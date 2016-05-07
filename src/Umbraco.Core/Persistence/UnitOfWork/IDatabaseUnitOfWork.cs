namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Represents a persistence unit of work for working with a database.
	/// </summary>
	public interface IDatabaseUnitOfWork : IUnitOfWork
	{
		UmbracoDatabase Database { get; }

	    void ReadLock(params int[] lockIds);
	    void WriteLock(params int[] lockIds);
    }
}