using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

/// <summary>
/// Provides shared logic for resolving a DI-registered provider implementation (searcher or indexer) for a given index alias.
/// </summary>
/// <typeparam name="T">The type of provider implementation to resolve.</typeparam>
internal abstract class ResolverBase<T>
    where T : class
{
    private readonly IndexOptions _indexOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ResolverBase{T}"/> class.
    /// </summary>
    /// <param name="indexOptions">The options describing the registered index registrations.</param>
    /// <param name="serviceProvider">The service provider used to resolve the registered provider implementation.</param>
    /// <param name="logger">The logger used to record resolution failures.</param>
    protected ResolverBase(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger logger)
    {
        _indexOptions = indexOptions.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Resolves the provider implementation registered for the given index alias.
    /// </summary>
    /// <param name="indexAlias">The index alias to resolve.</param>
    /// <param name="getTypeToResolve">Selects the implementation type to resolve from the index's registration.</param>
    /// <returns>The resolved implementation, or null if the index alias is not registered or the type could not be resolved.</returns>
    protected T? Resolve(string indexAlias, Func<IndexRegistration, Type> getTypeToResolve)
    {
        IndexRegistration? indexRegistration = _indexOptions.GetIndexRegistration(indexAlias);
        if (indexRegistration is null)
        {
            _logger.LogWarning("No index registration was found for index alias: {indexAlias}", indexAlias);
            return null;
        }

        Type typeToResolve = getTypeToResolve(indexRegistration);
        if (_serviceProvider.GetService(typeToResolve) is not T resolved)
        {
            _logger.LogError("Could not resolve type {type} as {name}. Make sure the type is registered in the DI.", typeToResolve.FullName, typeof(T).Name);
            return null;
        }

        return resolved;
    }
}
