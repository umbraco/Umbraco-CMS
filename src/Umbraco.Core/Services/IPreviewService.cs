namespace Umbraco.Cms.Core.Services;

public interface IPreviewService
{
    /// <summary>
    /// Enters preview mode for a given user that calls this
    /// </summary>
    void EnterPreview();

    /// <summary>
    /// Exits preview mode for a given user that calls this
    /// </summary>
    void EndPreview();
}
