using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopApiQueryService : IApiQueryService
{
    /// <inheritdoc />
    public PagedModel<Guid> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
        => new();
}
