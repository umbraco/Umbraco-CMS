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
        /// <remarks>The new database must be disposed after being used.</remarks>
        IUmbracoDatabase CreateDatabase();

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

        /// <summary>
        /// Gets the Sql context.
        /// </summary>
        ISqlContext SqlContext { get; }

        /// <summary>
        /// Configures the database factory for upgrades.
        /// </summary>
        void ConfigureForUpgrade();
    }
}
