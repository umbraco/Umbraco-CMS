namespace Umbraco.Search;

/// <summary>
///     A Marker interface for defining an Umbraco indexer
/// </summary>
public interface IUmbracoIndex<T> : IUmbracoIndex
{
    void IndexItems(T[] members);

}

public interface IUmbracoIndex
{
    /// <summary>
    ///     When set to true Umbraco will keep the index in sync with Umbraco data automatically
    /// </summary>
    bool EnableDefaultEventHandler { get; }

    /// <summary>
    ///     When set to true the index will only retain published values
    /// </summary>
    /// <remarks>
    ///     Any non-published values will not be put or kept in the index:
    ///     * Deleted, Trashed, non-published Content items
    ///     * non-published Variants
    /// </remarks>
    bool PublishedValuesOnly { get; }

    string Name { get; }
    Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    long GetDocumentCount();
    bool Exists();
    void Create();
    IEnumerable<string> GetFieldNames();
}
