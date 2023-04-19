namespace Umbraco.Cms.Core.DeliveryApi;

public interface IRequestPreviewService
{
    /// <summary>
    ///     Retrieves information on whether or not to output draft content for preview.
    /// </summary>
    bool IsPreview();
}
