using Microsoft.Extensions.Caching.Memory;

namespace Umbraco.Search.InMemory;

public class UmbracoMemoryIndex<T> : IUmbracoIndex<T>
{
    public UmbracoMemoryIndex(IMemoryCache memoryCache,string name, Action<object?, EventArgs>? indexOperationComplete)
    {
        Name = name;
        IndexOperationComplete = indexOperationComplete;
    }

    public void IndexItems(T[] members) => throw new NotImplementedException();

    public void Dispose() => throw new NotImplementedException();

    public string Name { get; }
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }
    public long GetDocumentCount() => throw new NotImplementedException();

    public bool Exists() => throw new NotImplementedException();

    public void Create() => throw new NotImplementedException();

    public IEnumerable<string> GetFieldNames() => throw new NotImplementedException();

    public void RemoveFromIndex(IEnumerable<string> ids) => throw new NotImplementedException();
}

