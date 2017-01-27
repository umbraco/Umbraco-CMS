namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal static class ScopeUnitOfWorkProviderExtensions
    {
        /// <summary>
        /// Hack: When we need a unit of work for readonly operations, the uow must be committed else the
        /// outer scope will rollback, so this is a hack to create a unit of work, pre-commit it and return it
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        public static IScopeUnitOfWork GetReadOnlyUnitOfWork(this IScopeUnitOfWorkProvider provider)
        {
            var uow = provider.GetUnitOfWork();
            uow.Commit();
            return uow;
        }
    }
}