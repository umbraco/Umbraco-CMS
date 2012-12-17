namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
    /// Defines a Unit of Work Provider
    /// </summary>
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork GetUnitOfWork();
    }
}