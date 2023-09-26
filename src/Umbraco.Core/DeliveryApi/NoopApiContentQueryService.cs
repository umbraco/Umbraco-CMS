using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopApiContentQueryService : IApiContentQueryService
{
    [Obsolete($"Use the {nameof(ExecuteQuery)} method that accepts {nameof(ProtectedAccess)}. Will be removed in V14.")]
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, int skip, int take)
        => Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, new PagedModel<Guid>());

    /// <inheritdoc />
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, ProtectedAccess protectedAccess, int skip, int take)
        => Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, new PagedModel<Guid>());
}
