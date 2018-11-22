namespace Umbraco.Web.SignalR
{
    public interface IPreviewHub
    {
        // define methods implemented by client
        // ReSharper disable InconsistentNaming

        void refreshed(int id);

        // ReSharper restore InconsistentNaming
    }
}
