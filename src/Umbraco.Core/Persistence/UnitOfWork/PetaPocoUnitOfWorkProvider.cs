namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal class PetaPocoUnitOfWorkProvider : IUnitOfWorkProvider<Database>
    {
        public IUnitOfWork<Database> GetUnitOfWork()
        {
            return new PetaPocoUnitOfWork();
        }
    }
}