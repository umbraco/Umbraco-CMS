using System;
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

        private static readonly Database _database = new Database(GlobalSettings.DbDsn);
        private static readonly Lazy<DatabaseFactory> lazy = new Lazy<DatabaseFactory>(() => new DatabaseFactory());

        public static DatabaseFactory Current { get { return lazy.Value; } }

        private DatabaseFactory()
        {
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