using Umbraco.Cms.Core.Services.OperationStatus;
using Umbraco.New.Cms.Core.Models;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopApiContentQueryService : IApiContentQueryService
{
    /// <inheritdoc />
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
        => Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, new PagedModel<Guid>());
}
