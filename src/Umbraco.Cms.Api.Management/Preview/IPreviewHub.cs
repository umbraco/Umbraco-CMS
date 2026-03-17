namespace Umbraco.Cms.Api.Management.Preview;

/// <summary>
/// Represents a contract for a preview hub in the Umbraco CMS Management API.
/// </summary>
public interface IPreviewHub
{
    // define methods implemented by client
    // ReSharper disable InconsistentNaming

    /// <summary>
    /// Notifies clients that the preview with the specified key has been refreshed.
    /// </summary>
    /// <param name="key">The unique identifier of the refreshed preview.</param>
    /// <returns>A task that represents the asynchronous notification operation.</returns>
    Task refreshed(Guid key);

    // ReSharper restore InconsistentNaming
}
