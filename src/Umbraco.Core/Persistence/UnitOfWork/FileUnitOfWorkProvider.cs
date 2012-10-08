namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="FileUnitOfWork"/>
    /// </summary>
    internal class FileUnitOfWorkProvider : IUnitOfWorkProvider
    {
        #region Implementation of IUnitOfWorkProvider

        public IUnitOfWork GetUnitOfWork()
        {
            return new FileUnitOfWork();
        }

        #endregion
    }
}