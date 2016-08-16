namespace Umbraco.Core.DependencyInjection
{
    /// <summary>
    /// Represents a collection builder.
    /// </summary>
    /// <typeparam name="TCollection">The type of the collection.</typeparam>
    /// <typeparam name="TItem">The type of the items.</typeparam>
    public interface ICollectionBuilder<out TCollection, TItem>
        where TCollection : IBuilderCollection<TItem>
    {
        /// <summary>
        /// Gets a collection.
        /// </summary>
        /// <returns>A collection.</returns>
        /// <remarks>The lifetime of the collection depends on how the builder was registered.</remarks>
        TCollection GetCollection();
    }
}