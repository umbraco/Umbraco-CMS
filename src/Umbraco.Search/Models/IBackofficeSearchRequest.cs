using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Search.Models;

public class BackofficeSearchRequest : IBackofficeSearchRequest
{
    public BackofficeSearchRequest(string query, UmbracoEntityTypes entityType, long page, int pageSize,
        string? searchFrom, bool ignoreUserStartNodes)
    {
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

    public UmbracoEntityTypes EntityType { get; set; }
    public string Query { get; set; }
    public long Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchFrom { get; set; }
    public bool IgnoreUserStartNodes { get; set; }
}

public interface IBackofficeSearchRequest
{
    public UmbracoEntityTypes EntityType { get; set; }
    public string Query { get; set; }
    public long Page { get; set; }
    public int PageSize { get; set; }
    public string? SearchFrom { get; set; }
    public bool IgnoreUserStartNodes { get; set; }
}
