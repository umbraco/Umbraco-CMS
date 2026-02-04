using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IApiContentQueryService"/> that always returns an empty result.
/// </summary>
public sealed class NoopApiContentQueryService : IApiContentQueryService
{
    /// <inheritdoc />
    public Attempt<PagedModel<Guid>, ApiContentQueryOperationStatus> ExecuteQuery(string? fetch, IEnumerable<string> filters, IEnumerable<string> sorts, ProtectedAccess protectedAccess, int skip, int take)
        => Attempt.SucceedWithStatus(ApiContentQueryOperationStatus.Success, new PagedModel<Guid>());
}
