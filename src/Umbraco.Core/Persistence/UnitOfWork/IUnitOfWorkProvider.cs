namespace Umbraco.Core.Persistence.UnitOfWork
{
    public interface IUnitOfWorkProvider
    {
        IUnitOfWork GetUnitOfWork();
    }
}