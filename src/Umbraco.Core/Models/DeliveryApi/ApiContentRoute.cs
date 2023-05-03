﻿namespace Umbraco.Cms.Core.Models.DeliveryApi;

public class ApiContentRoute : IApiContentRoute
{
    public ApiContentRoute(string path, ApiContentStartItem startItem)
    {
        Path = path;
        StartItem = startItem;
    }

    public string Path { get; }

    public IApiContentStartItem StartItem { get; }
}
