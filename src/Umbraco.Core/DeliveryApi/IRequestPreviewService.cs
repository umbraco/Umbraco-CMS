namespace Umbraco.Cms.Core.DeliveryApi;

/// <summary>
///     Defines a service that determines whether the current request is in preview mode.
/// </summary>
public interface IRequestPreviewService
{
    /// <summary>
    ///     Retrieves information on whether or not to output draft content for preview.
    /// </summary>
    bool IsPreview();
}
