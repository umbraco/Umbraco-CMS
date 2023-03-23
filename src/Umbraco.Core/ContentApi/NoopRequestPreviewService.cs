namespace Umbraco.Cms.Core.ContentApi;

public sealed class NoopRequestPreviewService : IRequestPreviewService
{
    /// <inheritdoc />
    public bool IsPreview() => false;
}

