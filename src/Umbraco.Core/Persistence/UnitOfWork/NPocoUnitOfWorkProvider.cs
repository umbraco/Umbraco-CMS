using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="NPocoUnitOfWork"/>.
    /// </summary>
    public class NPocoUnitOfWorkProvider : IDatabaseUnitOfWorkProvider
    {
        private readonly IDatabaseFactory _dbFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="NPocoUnitOfWorkProvider"/> class with an <see cref="IDatabaseFactory"/>.
        /// </summary>
        /// <param name="dbFactory"></param>
        public NPocoUnitOfWorkProvider(IDatabaseFactory dbFactory)
        {
            Mandate.ParameterNotNull(dbFactory, "dbFactory");
            _dbFactory = dbFactory;
        }

        // for unit tests only
        // will re-create a new DefaultDatabaseFactory each time it is called
        internal NPocoUnitOfWorkProvider(ILogger logger)
            : this(new DefaultDatabaseFactory(GlobalSettings.UmbracoConnectionName, logger))
        { }

        #region Implementation of IUnitOfWorkProvider

        /// <summary>
        /// Creates a unit of work around a database obtained from the database factory.
        /// </summary>
        /// <returns>A unit of work.</returns>
        /// <remarks>The unit of work will execute on the current database returned by the database factory.</remarks>
        public IDatabaseUnitOfWork GetUnitOfWork()
        {
            // get a database from the factory - might be the "ambient" database eg
            // the one that's enlisted with the HttpContext - so it's not always a
            // "new" database.
            var database = _dbFactory.CreateDatabase();
            return new NPocoUnitOfWork(database);
        }

        #endregion
    }
}