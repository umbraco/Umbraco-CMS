namespace Umbraco.Cms.Core.ContentApi;

public interface IApiQueryService
{
    IEnumerable<Guid> ExecuteQuery(string? fetch, string[] filters, string[] sorts);
}
