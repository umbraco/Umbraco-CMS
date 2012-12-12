using Umbraco.Core.Configuration;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="PetaPocoUnitOfWork"/>
    /// </summary>
    public class PetaPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly string _connectionString;
        private readonly string _providerName;

        /// <summary>
        /// Parameterless constructor
        /// </summary>
        public PetaPocoUnitOfWorkProvider()
        {
            _connectionString = GlobalSettings.UmbracoConnectionName;
        }

        /// <summary>
        /// Constructor to explicitly set the connectionstring to use
        /// </summary>
        /// <param name="connectionString">Connection String to use</param>
        public PetaPocoUnitOfWorkProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Constructor to explicitly set the connectionstring and provider name to use,
        /// which will avoid the lookup of any additional config settings.
        /// </summary>
        /// <param name="connectionString">Connection String to use</param>
        /// <param name="providerName">Database Provider</param>
        public PetaPocoUnitOfWorkProvider(string connectionString, string providerName)
        {
            _connectionString = connectionString;
            _providerName = providerName;
        }

	    #region Implementation of IUnitOfWorkProvider

        /// <summary>
		/// Creates a Unit of work with a new UmbracoDatabase instance for the work item/transaction.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Each PetaPoco UOW uses it's own Database object, not the shared Database object that comes from
		/// the DatabaseContext.Current.Database. This is because each transaction should use it's own Database
		/// and we Dispose of this Database object when the UOW is disposed.
		/// </remarks>
	    public IDatabaseUnitOfWork GetUnitOfWork()
        {
            var database = string.IsNullOrEmpty(_providerName)
                               ? new UmbracoDatabase(_connectionString)
                               : new UmbracoDatabase(_connectionString, _providerName);
            return new PetaPocoUnitOfWork(database);
        }

        #endregion

		/// <summary>
		/// Static helper method to return a new unit of work
		/// </summary>
		/// <returns></returns>
		internal static IDatabaseUnitOfWork CreateUnitOfWork()
		{
            var provider = new PetaPocoUnitOfWorkProvider(GlobalSettings.UmbracoConnectionName);
			return provider.GetUnitOfWork();
		}
    }
}