namespace Umbraco.Cms.Core.DeliveryApi;

public sealed class NoopRequestPreviewService : IRequestPreviewService
{
    /// <inheritdoc />
    public bool IsPreview() => false;
}

