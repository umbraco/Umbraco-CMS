namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Defines a Unit of Work Provider
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IUnitOfWorkProvider<T>
    {
        IUnitOfWork<T> GetUnitOfWork();
    }
}