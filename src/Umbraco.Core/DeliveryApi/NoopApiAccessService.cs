namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IApiAccessService"/> that denies all access.
/// </summary>
public sealed class NoopApiAccessService : IApiAccessService
{
    /// <inheritdoc />
    public bool HasPublicAccess() => false;

    /// <inheritdoc />
    public bool HasPreviewAccess() => false;
}
