﻿using Umbraco.Cms.Core.Models.Search;

namespace Umbraco.Search;

public interface IUmbracoSearcher<T> : IUmbracoSearcher
{
}

public interface IUmbracoSearcher
{
    public UmbracoSearchResults Search(string term, int page, int pageSize);

    string Name { get;  }
    UmbracoSearchResults? NativeQuery(string query, int page, int pageSize);
}
