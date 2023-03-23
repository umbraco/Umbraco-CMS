using Umbraco.New.Cms.Core.Models.Installer;

namespace Umbraco.New.Cms.Core.Factories;

/// <summary>
/// Creates <see cref="DatabaseSettingsModel"/> based on the currently configured providers.
/// </summary>
public interface IDatabaseSettingsFactory
{
    /// <summary>
    /// Creates a collection of database settings models for the currently installed database providers
    /// </summary>
    /// <returns>Collection of database settings.</returns>
    /// <exception cref="InvalidOperationException">Thrown if a connection string is preconfigured, but provider name is missing.</exception>
    ICollection<DatabaseSettingsModel> GetDatabaseSettings();
}
