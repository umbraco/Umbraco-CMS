using Umbraco.Cms.Core.Models.Entities;

namespace Umbraco.Search.InMemory;

public class InMemoryIndexManager : IInMemoryIndexManager
{
    private readonly IEnumerable<IUmbracoIndex?> _umbracoIndices;
    private readonly IEnumerable<IUmbracoSearcher> _umbracoSearchers;

    public InMemoryIndexManager(
        IEnumerable<IUmbracoIndex?> umbracoIndices,
        IEnumerable<IUmbracoSearcher> umbracoSearchers)
    {
        _umbracoIndices = umbracoIndices;
        _umbracoSearchers = umbracoSearchers;
    }

    public IUmbracoIndex? GetIndex(string index)
    {
        return _umbracoIndices.FirstOrDefault(x => x?.Name == index);
    }

    public IUmbracoIndex<T>? GetIndex<T>(string index) where T : IUmbracoEntity
    {
        return _umbracoIndices.FirstOrDefault(x => x?.Name == index) as IUmbracoIndex<T>;
    }

    public IUmbracoSearcher? GetSearcher(string index)
    {
        return _umbracoSearchers.FirstOrDefault(x => x?.Name == index);
    }

    public IUmbracoSearcher<T>? GetSearcher<T>(string index) where T : IUmbracoEntity
    {
        return _umbracoSearchers.FirstOrDefault(x => x?.Name == index) as IUmbracoSearcher<T>;
    }
}

public interface IInMemoryIndexManager
{
    IUmbracoIndex? GetIndex(string index);
    IUmbracoIndex<T>? GetIndex<T>(string index) where T : IUmbracoEntity;
    IUmbracoSearcher? GetSearcher(string index);
    IUmbracoSearcher<T>? GetSearcher<T>(string index) where T : IUmbracoEntity;
}
