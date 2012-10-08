namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal class PetaPocoUnitOfWorkProvider : IUnitOfWorkProvider
    {
        #region Implementation of IUnitOfWorkProvider

        public IUnitOfWork GetUnitOfWork()
        {
            return new PetaPocoUnitOfWork();
        }

        #endregion
    }
}