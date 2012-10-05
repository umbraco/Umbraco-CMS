using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    internal class PetaPocoUnitOfWork : IUnitOfWork<Database>
    {
        private readonly Transaction _petaTransaction;
        private readonly Database _storage;

        public PetaPocoUnitOfWork()
        {
            var connectionString = GlobalSettings.DbDsn;
            _storage = new Database(connectionString);
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