namespace Umbraco.Cms.Api.Management.Preview;

public interface IPreviewHub
{
    // define methods implemented by client
    // ReSharper disable InconsistentNaming

    Task refreshed(Guid key);

    Task allRefreshed();

    // ReSharper restore InconsistentNaming
}
