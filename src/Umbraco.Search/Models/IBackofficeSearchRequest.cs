using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Search.Models;

public class BackofficeSearchRequest : IBackofficeSearchRequest
{
    private readonly string? _culture;

    public BackofficeSearchRequest(string query, UmbracoEntityTypes entityType, long page, int pageSize,
        string? searchFrom, bool ignoreUserStartNodes = false, string? culture = null)
    {
        _culture = culture;
        Query = query;
        EntityType = entityType;
        Page = page;
        PageSize = pageSize;
        SearchFrom = searchFrom;
        IgnoreUserStartNodes = ignoreUserStartNodes;
    }

    public BackofficeSearchRequest(string query, UmbracoEntityTypes entityType, long page, int pageSize)
    {
        Query = query;
        EntityType = entityType;
        Page = page;
        PageSize = pageSize;
    }

    public BackofficeSearchRequest(string query, UmbracoObjectTypes objectType, long pageIndex, int pageSize,
        string? searchFrom)
    {
        Query = query;
        ObjectType = objectType;
        Page = pageIndex;
        PageSize = pageSize;
        SearchFrom = searchFrom;
    }

    public UmbracoObjectTypes ObjectType { get; set; }
    public UmbracoEntityTypes EntityType { get; set; }
    public string Query { get; set; }
    public long Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchFrom { get; set; }
    public bool IgnoreUserStartNodes { get; set; }
    public string? Culture { get; set; }
}

public interface IBackofficeSearchRequest
{
    public UmbracoEntityTypes EntityType { get; set; }
    public UmbracoObjectTypes ObjectType { get; set; }
    public string Query { get; set; }
    public long Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchFrom { get; set; }
    public bool IgnoreUserStartNodes { get; set; }
    string? Culture { get; set; }
}
