using System;
using Umbraco.Core.Persistence.Querying;
using Umbraco.Core.Persistence.SqlSyntax;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Creates and manages the "ambient" database.
	/// </summary>
	public interface IDatabaseFactory : IDisposable
	{
        /// <summary>
        /// Gets (creates or retrieves) the "ambient" database connection.
        /// </summary>
        /// <returns>The "ambient" database connection.</returns>
		UmbracoDatabase GetDatabase();

	    void Configure(string connectionString, string providerName);

        bool Configured { get; }

        bool CanConnect { get; }
        
        IQueryFactory QueryFactory { get; }
	}
}