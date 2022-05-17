using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Represents a collection builder.
/// </summary>
public interface ICollectionBuilder
{
    /// <summary>
    ///     Registers the builder so it can build the collection, by
    ///     registering the collection and the types.
    /// </summary>
    void RegisterWith(IServiceCollection services);
}

/// <summary>
///     Represents a collection builder.
/// </summary>
/// <typeparam name="TCollection">The type of the collection.</typeparam>
/// <typeparam name="TItem">The type of the items.</typeparam>
public interface ICollectionBuilder<out TCollection, TItem> : ICollectionBuilder
    where TCollection : IBuilderCollection<TItem>
{
    /// <summary>
    ///     Creates a collection.
    /// </summary>
    /// <returns>A collection.</returns>
    /// <remarks>Creates a new collection each time it is invoked.</remarks>
    TCollection CreateCollection(IServiceProvider factory);
}
