using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Logging;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// Used to return the correct syntax provider for a given provider name
    /// </summary>
    public sealed class SqlSyntaxProviders 
    {
        private readonly IEnumerable<ISqlSyntaxProvider> _syntaxProviders;

        internal static SqlSyntaxProviders CreateDefault(ILogger logger)
        {
            return new SqlSyntaxProviders(new ISqlSyntaxProvider[]
            {
                new MySqlSyntaxProvider(logger),
                new SqlCeSyntaxProvider(),
                new SqlServerSyntaxProvider()
            });
        }

        /// <summary>
		/// Constructor
		/// </summary>
        /// <param name="syntaxProviders"></param>
        public SqlSyntaxProviders(IEnumerable<ISqlSyntaxProvider> syntaxProviders)
        {
            if (syntaxProviders == null) throw new ArgumentNullException("syntaxProviders");
            _syntaxProviders = syntaxProviders;
        }

        /// <summary>
        /// Gets a <see cref="ISqlSyntaxProvider"/> by its attributed provider.
        /// </summary>
        /// <param name="providerName">ProviderName from the ConnectionString settings</param>
        /// <returns><see cref="ISqlSyntaxProvider"/> that corresponds to the attributed provider or the default Sql Server Syntax Provider.</returns>
        public ISqlSyntaxProvider GetByProviderNameOrDefault(string providerName)
        {
            var provider =
                _syntaxProviders.FirstOrDefault(
                    x =>
                    x.GetType()
                     .FirstAttribute<SqlSyntaxProviderAttribute>()
                     .ProviderName.ToLowerInvariant()
                     .Equals(providerName.ToLowerInvariant()));

            if (provider != null)
                return provider;

            //default
            return _syntaxProviders.First(x => x.GetType() == typeof(SqlServerSyntaxProvider));
        }
    }
}