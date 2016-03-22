using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="NPocoUnitOfWork"/>
    /// </summary>
    public class NPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly IDatabaseFactory _dbFactory;

        // fixme.npoco STOP creating database factory all the time!!!
        // there should be one and only one

        /// <summary>
        /// Parameterless constructor uses defaults
        /// </summary>
        public NPocoUnitOfWorkProvider(ILogger logger)
            : this(new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, logger))
        {

        }

        /// <summary>
        /// Constructor accepting custom connectino string and provider name
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString">Connection String to use with Database</param>
        /// <param name="providerName">Database Provider for the Connection String</param>
        public NPocoUnitOfWorkProvider(ILogger logger, string connectionString, string providerName)
            : this(new DefaultDatabaseFactory(connectionString, providerName, logger))
        { }

        /// <summary>
        /// Constructor accepting an IDatabaseFactory instance
        /// </summary>
        /// <param name="dbFactory"></param>
        public NPocoUnitOfWorkProvider(IDatabaseFactory dbFactory)
        {
            Mandate.ParameterNotNull(dbFactory, "dbFactory");
            _dbFactory = dbFactory;
        }

        #region Implementation of IUnitOfWorkProvider

        /// <summary>
        /// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// fixme.npoco this is not true - _dbFactory.CreateDatabase() will keep reusing the db that's in cache?!
        ///   and DatabaseContext.Database does it too - so the whole comment here is moot
        ///   kill this:
        /// Each UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// </remarks>
        public IDatabaseUnitOfWork GetUnitOfWork()
        {
            return new NPocoUnitOfWork(_dbFactory.CreateDatabase());
        }

        #endregion

        /// <summary>
        /// Static helper method to return a new unit of work
        /// </summary>
        /// <returns></returns>
        internal static IDatabaseUnitOfWork CreateUnitOfWork(ILogger logger)
        {
            var provider = new NPocoUnitOfWorkProvider(logger);
            return provider.GetUnitOfWork();
        }
    }
}