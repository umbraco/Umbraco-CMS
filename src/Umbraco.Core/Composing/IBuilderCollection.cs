namespace Umbraco.Cms.Core.Composing;

/// <summary>
///     Represents a builder collection, ie an immutable enumeration of items.
/// </summary>
/// <typeparam name="TItem">The type of the items.</typeparam>
public interface IBuilderCollection<out TItem> : IEnumerable<TItem>
{
    /// <summary>
    ///     Gets the number of items in the collection.
    /// </summary>
    int Count { get; }
}
