namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IUnitOfWorkProvider<T>
    {
        IUnitOfWork<T> GetUnitOfWork();
    }
}