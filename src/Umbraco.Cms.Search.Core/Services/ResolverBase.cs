using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Search.Core.Configuration;
using Umbraco.Cms.Search.Core.Models.Configuration;

namespace Umbraco.Cms.Search.Core.Services;

internal abstract class ResolverBase<T>
    where T : class
{
    private readonly IndexOptions _indexOptions;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    protected ResolverBase(IOptions<IndexOptions> indexOptions, IServiceProvider serviceProvider, ILogger logger)
    {
        _indexOptions = indexOptions.Value;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

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
            _logger.LogError($"Could not resolve type {{type}} as {typeof(T).Name}. Make sure the type is registered in the DI.", typeToResolve.FullName);
            return null;
        }

        return resolved;
    }
}
