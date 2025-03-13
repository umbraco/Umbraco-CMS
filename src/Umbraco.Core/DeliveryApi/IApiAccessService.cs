namespace Umbraco.Cms.Core.DeliveryApi;

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

    /// <summary>
    ///     Retrieves information on whether or not the API currently allows access to media.
    /// </summary>
    bool HasMediaAccess() => false;
}
