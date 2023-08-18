using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopApiMediaQueryService : IApiMediaQueryService
{
    /// <inheritdoc />
    public Attempt<PagedModel<Guid>, ApiMediaQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
        => Attempt.SucceedWithStatus(ApiMediaQueryOperationStatus.Success, new PagedModel<Guid>());

    /// <inheritdoc />
    public IPublishedContent? GetByPath(string path) => null;
}
