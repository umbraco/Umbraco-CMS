namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Represents a factory interface for creating mappers that are specific to a particular data provider.
/// </summary>
public interface IProviderSpecificMapperFactory
{
    /// <summary>
    /// Gets the name of the provider associated with this mapper factory.
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Gets a collection of NPoco mappers that are specific to the current database provider.
    /// </summary>
    NPocoMapperCollection Mappers { get; }
}
