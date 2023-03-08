namespace Umbraco.Cms.Core.ContentApi;

public interface IApiQueryService
{
    IEnumerable<Guid> ExecuteQuery(Dictionary<string, string> queryParams, string fieldValue);
}
