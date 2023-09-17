﻿using System.Runtime.CompilerServices;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Models.Search;
using Umbraco.Cms.Core.Web;
using Umbraco.Search.Diagnostics;

namespace Umbraco.Search;

public interface IUmbracoSearcher<T> : IUmbracoSearcher
{
}

public interface IUmbracoSearcher
{
    public UmbracoSearchResults Search(string term, int page, int pageSize);

    string Name { get; }
    UmbracoSearchResults? NativeQuery(string query, int page, int pageSize);

    IEnumerable<PublishedSearchResult> SearchDescendants(
        IPublishedContent content,
        string term);

    IEnumerable<PublishedSearchResult> SearchChildren(IPublishedContent content, string term);

    IUmbracoSearchResults Search(ISearchRequest searchRequest);
    public ISearchRequest CreateSearchRequest();

    IEnumerable<PublishedSearchResult> GetAll();
    ISearchEngine? SearchEngine { get; }
}
