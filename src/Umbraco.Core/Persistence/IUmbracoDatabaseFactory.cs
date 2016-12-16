using System;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Creates and manages the "ambient" database.
	/// </summary>
	public interface IUmbracoDatabaseFactory : IDatabaseContext, IDisposable
	{
        /// <summary>
        /// Gets (creates if needed) the ambient database.
        /// </summary>
        /// <exception cref="InvalidOperationException">There is no ambient database scope.</exception>
        /// <remarks>The ambient database is the database owned by the ambient scope. It should not
        /// be disposed, as the scope takes care of disposing it when appropriate.</remarks>
		IUmbracoDatabase GetDatabase();

        /// <summary>
        /// Gets (creates if needed) the ambient database.
        /// </summary>
        /// <remarks>This is just a shortcut to GetDatabase.</remarks>
        IUmbracoDatabase Database { get; } // fixme keep?

        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <remarks>The new database is not part of any scope and must be disposed after being used.</remarks>
	    IUmbracoDatabase CreateDatabase();

        /// <summary>
        /// Creates a new database scope.
        /// </summary>
        /// <param name="database">A database for the scope.</param>
        /// <remarks>
        /// <para>The new database scope becomes the ambient scope and may be nested under
        /// an already existing ambient scope.</para>
        /// <para>In most cases, <paramref name="database"/> should be null. It can be used to force the temporary
        /// usage of another database instance. Use with care.</para>
        /// </remarks>
	    IDatabaseScope CreateScope(IUmbracoDatabase database = null);

        /// <summary>
        /// Gets a value indicating whether the database factory is configured.
        /// </summary>
        bool Configured { get; }

        /// <summary>
        /// Gets a value indicating whether the database can connect.
        /// </summary>
        bool CanConnect { get; }

        /// <summary>
        /// Configures the database factory.
        /// </summary>
        void Configure(string connectionString, string providerName);
    }
}