namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopApiAccessService : IApiAccessService
{
    /// <inheritdoc />
    public bool HasPublicAccess() => false;

    /// <inheritdoc />
    public bool HasPreviewAccess() => false;
}
