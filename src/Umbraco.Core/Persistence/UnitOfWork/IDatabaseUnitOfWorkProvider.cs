namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Defines a Unit of Work Provider for working with an IDatabaseUnitOfWork
	/// </summary>
	public interface IDatabaseUnitOfWorkProvider
	{
		IDatabaseUnitOfWork GetUnitOfWork();
    }
}