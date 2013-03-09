using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.ObjectResolution;

namespace Umbraco.Core.Persistence.SqlSyntax
{
    /// <summary>
    /// A resolver to return all ISqlSyntaxProvider objects
    /// </summary>
    internal sealed class SqlSyntaxProvidersResolver : ManyObjectsResolverBase<SqlSyntaxProvidersResolver, ISqlSyntaxProvider>
    {
        /// <summary>
		/// Constructor
		/// </summary>
        /// <param name="syntaxProviders"></param>
        internal SqlSyntaxProvidersResolver(IEnumerable<Type> syntaxProviders)
            : base(syntaxProviders)
		{

		}

		/// <summary>
        /// Gets the <see cref="ISqlSyntaxProvider"/> implementations.
		/// </summary>
        public IEnumerable<ISqlSyntaxProvider> SqlSyntaxProviders
		{
			get
			{
				return Values;
			}
		}

        /// <summary>
        /// Gets a <see cref="ISqlSyntaxProvider"/> by its attributed provider.
        /// </summary>
        /// <param name="providerName">ProviderName from the ConnectionString settings</param>
        /// <returns><see cref="ISqlSyntaxProvider"/> that corresponds to the attributed provider or the default Sql Server Syntax Provider.</returns>
        public ISqlSyntaxProvider GetByProviderNameOrDefault(string providerName)
        {
            var provider =
                Values.FirstOrDefault(
                    x =>
                    x.GetType()
                     .FirstAttribute<SqlSyntaxProviderAttribute>()
                     .ProviderName.ToLowerInvariant()
                     .Equals(providerName.ToLowerInvariant()));

            if (provider != null)
                return provider;

            return Values.First(x => x.GetType() == typeof (SqlServerSyntaxProvider));
        }
    }
}