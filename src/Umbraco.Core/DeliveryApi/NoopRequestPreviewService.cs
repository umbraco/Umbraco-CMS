namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     A no-operation implementation of <see cref="IRequestPreviewService"/> that always returns <c>false</c> for preview mode.
/// </summary>
public sealed class NoopRequestPreviewService : IRequestPreviewService
{
    /// <inheritdoc />
    public bool IsPreview() => false;
}

