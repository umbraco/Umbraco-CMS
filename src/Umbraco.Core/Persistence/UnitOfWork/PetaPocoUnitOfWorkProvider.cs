using System;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="PetaPocoUnitOfWork"/>
    /// </summary>
    public class PetaPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly IDatabaseFactory _dbFactory;

        [Obsolete("Use the constructor specifying an ILogger instead")]
        public PetaPocoUnitOfWorkProvider()
            : this(new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, LoggerResolver.Current.Logger))
        {

        }

        [Obsolete("Use the constructor specifying an ILogger instead")]
        public PetaPocoUnitOfWorkProvider(string connectionString, string providerName)
            : this(new DefaultDatabaseFactory(connectionString, providerName, LoggerResolver.Current.Logger))
        { }

        /// <summary>
        /// Parameterless constructor uses defaults
        /// </summary>
        public PetaPocoUnitOfWorkProvider(ILogger logger)
            : this(new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, logger))
        {

        }

        /// <summary>
        /// Constructor accepting custom connectino string and provider name
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString">Connection String to use with Database</param>
        /// <param name="providerName">Database Provider for the Connection String</param>
        public PetaPocoUnitOfWorkProvider(ILogger logger, string connectionString, string providerName)
            : this(new DefaultDatabaseFactory(connectionString, providerName, logger))
        { }

        /// <summary>
        /// Constructor accepting an IDatabaseFactory instance
        /// </summary>
        /// <param name="dbFactory"></param>
        internal PetaPocoUnitOfWorkProvider(IDatabaseFactory dbFactory)
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
        /// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
        /// the ApplicationContext.Current.DatabaseContext.Database. This is because each transaction should use it's own Database
        /// and we Dispose of this Database object when the UOW is disposed.
        /// </remarks>
        public IDatabaseUnitOfWork GetUnitOfWork()
        {
            return new PetaPocoUnitOfWork(_dbFactory.CreateDatabase());
        }

        #endregion

        /// <summary>
        /// Static helper method to return a new unit of work
        /// </summary>
        /// <returns></returns>
        internal static IDatabaseUnitOfWork CreateUnitOfWork(ILogger logger)
        {
            var provider = new PetaPocoUnitOfWorkProvider(logger);
            return provider.GetUnitOfWork();
        }
    }
}