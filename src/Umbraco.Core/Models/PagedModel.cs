﻿namespace Umbraco.New.Cms.Core.Models;

public class PagedModel<T>
{
    public PagedModel(long total, IEnumerable<T> items)
    {
        Total = total;
        Items = items;
    }

    public IEnumerable<T> Items { get; } = Enumerable.Empty<T>();

    public long Total { get; }
}
