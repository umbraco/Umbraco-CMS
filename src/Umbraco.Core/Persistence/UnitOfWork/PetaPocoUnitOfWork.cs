namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal class PetaPocoUnitOfWork : IUnitOfWork<Database>
    {
        private readonly Transaction _petaTransaction;
        private readonly Database _storage;

        public PetaPocoUnitOfWork()
        {
            _storage = new Database(@"server=.\SQLEXPRESS;database=UmbracoPOC-Site;user id=umbraco;password=umbraco", "System.Data.SqlClient");
            _petaTransaction = new Transaction(_storage);
        }

        public void Dispose()
        {
            _petaTransaction.Dispose();
        }

        public void Commit()
        {
            _petaTransaction.Complete();
        }

        public Database Storage
        {
            get { return _storage; }
        }
    }
}