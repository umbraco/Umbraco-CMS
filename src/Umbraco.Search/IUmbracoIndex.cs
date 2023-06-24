namespace Umbraco.Search;

/// <summary>
///     A Marker interface for defining an Umbraco indexer
/// </summary>
public interface IUmbracoIndex<T> : IUmbracoIndex
{
    void IndexItems(T[] members);

    void RemoveFromIndex(IEnumerable<string> select);
}

public interface IUmbracoIndex
{
    string Name { get; }
    Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    long GetDocumentCount();
    bool Exists();
    void Create();
    IEnumerable<string> GetFieldNames();
}
