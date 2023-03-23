namespace Umbraco.Cms.Core.ContentApi;

public interface IApiAccessService
{
    /// <summary>
    ///     Retrieves information on whether or not the API currently allows public access.
    /// </summary>
    bool HasPublicAccess();

    /// <summary>
    ///     Retrieves information on whether or not the API currently allows preview access.
    /// </summary>
    bool HasPreviewAccess();
}
