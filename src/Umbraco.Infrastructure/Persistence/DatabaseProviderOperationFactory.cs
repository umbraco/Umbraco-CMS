namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Implementation of <see cref="IDatabaseProviderOperationFactory"/> that indexes
/// provider-specific operations by provider name and falls back to default implementations.
/// </summary>
internal class DatabaseProviderOperationFactory : IDatabaseProviderOperationFactory
{
    private readonly Dictionary<string, IPropertyDataReplacerOperation> _propertyDataReplacers;
    private readonly IPropertyDataReplacerOperation _defaultPropertyDataReplacerOperation;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseProviderOperationFactory"/> class.
    /// </summary>
    /// <param name="propertyDataReplacers">The collection of provider-specific property data replacers.</param>
    public DatabaseProviderOperationFactory(IEnumerable<IPropertyDataReplacerOperation> propertyDataReplacers)
    {
        _propertyDataReplacers = propertyDataReplacers
            .Where(x => x.ProviderName is not null)
            .ToDictionary(x => x.ProviderName!, StringComparer.InvariantCultureIgnoreCase);

        _defaultPropertyDataReplacerOperation = new DefaultPropertyDataReplacerOperation();
    }

    /// <inheritdoc/>
    public IPropertyDataReplacerOperation GetPropertyDataReplacerOperation(string providerName)
    {
        if (_propertyDataReplacers.TryGetValue(providerName, out IPropertyDataReplacerOperation? operation))
        {
            return operation;
        }

        return _defaultPropertyDataReplacerOperation;
    }
}
