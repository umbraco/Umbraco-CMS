using Microsoft.Extensions.Caching.Memory;
using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Search.InMemory;

public class UmbracoMemoryIndex<T> : IUmbracoIndex<T> where T : IUmbracoEntity
{
    private readonly IMemoryCache _memoryCache;

    public UmbracoMemoryIndex(IMemoryCache memoryCache, string name)
    {
        _memoryCache = memoryCache;
        Name = name;
    }

    public void IndexItems(T[] members)
    {
        var existingObjects = _memoryCache.Get(Name) as List<T>;
        if (existingObjects == null)
        {
            existingObjects = members.ToList();
        }
        else
        {
            existingObjects = existingObjects.Where(x => members.All(y => y.Id != x.Id)).Union(members).ToList();
        }

        _memoryCache.Set(Name, existingObjects);
    }

    public void Dispose()
    {
        _memoryCache.Remove(Name);
    }

    public string Name { get; }

    /// <summary>
    ///    An event that is triggered when an index operation is complete
    /// </summary>
    public Action<object?, EventArgs>? IndexOperationComplete { get; set; }

    public long GetDocumentCount()
    {
        var existingObjects = _memoryCache.Get(Name) as List<T>;
        if (existingObjects != null)
        {
            return existingObjects.Count();
        }

        return 0;
    }

    public bool Exists()
    {
        return true;
    }

    public void Create()
    {
    }

    public IEnumerable<string> GetFieldNames()
    {
        var existingObjects = _memoryCache.Get(Name) as List<T>;
        if (existingObjects != null)
        {
            var properties = existingObjects.SelectMany(x => x.GetType().GetProperties()).Select(x => x.Name)
                .Distinct();
            return properties;
        }

        return new List<string>();
    }

    public void RemoveFromIndex(IEnumerable<string> ids)
    {
        var existingObjects = _memoryCache.Get(Name) as List<T>;
        if (existingObjects != null)
        {
            existingObjects = existingObjects.Where(x => ids.All(y => y != x.Id.ToString())).ToList();
            _memoryCache.Set(Name, existingObjects);
        }
    }
}
