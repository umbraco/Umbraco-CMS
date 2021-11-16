using System;

namespace Umbraco.Core.Persistence
{
    /// <summary>
    /// Creates and manages the "ambient" database.
    /// </summary>
    public interface IUmbracoDatabaseFactory : IDisposable
    {
        /// <summary>
        /// Creates a new database.
        /// </summary>
        /// <remarks>
        /// <para>The new database must be disposed after being used.</para>
        /// <para>Creating a database causes the factory to initialize if it is not already initialized.</para>
        /// </remarks>
        IUmbracoDatabase CreateDatabase();

        /// <summary>
        /// Gets a value indicating whether the database factory is configured, i.e. whether
        /// its connection string and provider name have been set. The factory may however not
        /// be initialized (see <see cref="Initialized"/>).
        /// </summary>
        bool Configured { get; }

        /// <summary>
        /// Gets a value indicating whether the database factory is initialized, i.e. whether
        /// its internal state is ready and it has been possible to connect to the database.
        /// </summary>
        bool Initialized { get; }

        /// <summary>
        /// Gets the connection string.
        /// </summary>
        /// <remarks>May return <c>null</c> if the database factory is not configured.</remarks>
        string ConnectionString { get; }

        /// <summary>
        /// Gets the provider name.
        /// </summary>
        /// <remarks>May return <c>null</c> if the database factory is not configured.</remarks>
        string ProviderName { get; }

        /// <summary>
        /// Gets a value indicating whether the database factory is configured (see <see cref="Configured"/>),
        /// and it is possible to connect to the database. The factory may however not be initialized (see
        /// <see cref="Initialized"/>).
        /// </summary>
        bool CanConnect { get; }

        /// <summary>
        /// Configures the database factory.
        /// </summary>
        void Configure(string connectionString, string providerName);

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        /// <remarks>
        /// <para>Getting the Sql context causes the factory to initialize if it is not already initialized.</para>
        /// </remarks>
        ISqlContext SqlContext { get; }

        /// <summary>
        /// Configures the database factory for upgrades.
        /// </summary>
        void ConfigureForUpgrade();
    }
}
