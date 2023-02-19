using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search;

public interface IUmbracoSearcher<T> : IUmbracoSearcher
{
}

public interface IUmbracoSearcher
{
    public IUmbracoSearchResult Search(string term, int page, int pageSize);

    string Name { get;  }
}
