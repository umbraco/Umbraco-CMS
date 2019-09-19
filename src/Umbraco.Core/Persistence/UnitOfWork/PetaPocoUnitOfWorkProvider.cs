using System;
using Umbraco.Core.Logging;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Persistence.UnitOfWork
{
    /// <summary>
    /// Represents a Unit of Work Provider for creating a <see cref="ScopeUnitOfWork"/>
    /// </summary>
    public class PetaPocoUnitOfWorkProvider : ScopeUnitOfWorkProvider
    {       
        [Obsolete("Use the constructor specifying an ILogger instead")]
        public PetaPocoUnitOfWorkProvider()
            : base(new ScopeProvider(new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, LoggerResolver.Current.Logger)))
        { }

        [Obsolete("Use the constructor specifying an ILogger instead")]
        public PetaPocoUnitOfWorkProvider(string connectionString, string providerName)
            : base(new ScopeProvider(new DefaultDatabaseFactory(connectionString, providerName, LoggerResolver.Current.Logger)))
        { }

        public PetaPocoUnitOfWorkProvider(ILogger logger)
            : base(new ScopeProvider(new DefaultDatabaseFactory(Constants.System.UmbracoConnectionName, logger)))
        { }

        /// <summary>
        /// Constructor accepting custom connection string and provider name
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="connectionString">Connection String to use with Database</param>
        /// <param name="providerName">Database Provider for the Connection String</param>
        public PetaPocoUnitOfWorkProvider(ILogger logger, string connectionString, string providerName)
            : base(new ScopeProvider(new DefaultDatabaseFactory(connectionString, providerName, logger)))
        { }

        public PetaPocoUnitOfWorkProvider(IScopeProvider scopeProvider)
            : base(scopeProvider)
        { }
    }
}