namespace Umbraco.Cms.Core.ContentApi;

public interface IApiQueryService
{
    /// <summary>
    ///     Gets children by querying.
    /// </summary>
    IEnumerable<Guid> GetChildren(Guid id);
}
