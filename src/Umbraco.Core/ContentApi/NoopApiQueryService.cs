namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopApiQueryService : IApiQueryService
{
    /// <inheritdoc />
    public IEnumerable<Guid> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts)
        => Enumerable.Empty<Guid>();
}
