using System.Threading;
using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Provides access to the PetaPoco database as Singleton, so the database is created once in app lifecycle.
    /// This is necessary for transactions to work properly
    /// </summary>
    public sealed class DatabaseFactory
    {
        #region Singleton

        private static Database _database;
        private static volatile DatabaseFactory _instance;
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

        private DatabaseFactory() { }

        public static DatabaseFactory Current
        {
            get
            {
                using (new WriteLock(Lock))
                {
                    if (_instance == null)
                    {
                        _instance = new DatabaseFactory();
                        _database = new Database(GlobalSettings.DbDsn);
                    }
                }

                return _instance;
            }
        }

        #endregion

        /// <summary>
        /// Returns an instance of the PetaPoco database
        /// </summary>
        public Database Database
        {
            get { return _database; }
        }
    }
}