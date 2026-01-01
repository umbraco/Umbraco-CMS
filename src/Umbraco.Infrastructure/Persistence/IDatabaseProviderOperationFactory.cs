namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Defines a factory for the creation of handlers for specific database operations that have been optimized for a given provider.
/// </summary>
public interface IDatabaseProviderOperationFactory
{
    /// <summary>
    /// Retrieves an instance of an <see cref="IPropertyDataReplacerOperation"/> for the specified provider name.
    /// </summary>
    /// <param name="providerName">The name of the provider for which to obtain the property data replacer.</param>
    /// <returns>An <see cref="IPropertyDataReplacerOperation"/> instance associated with the specified provider name.</returns>
    IPropertyDataReplacerOperation GetPropertyDataReplacerOperation(string providerName);
}
