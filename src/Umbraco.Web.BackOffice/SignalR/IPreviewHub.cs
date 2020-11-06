using System.Threading.Tasks;

namespace Umbraco.Web.BackOffice.SignalR
{
    public interface IPreviewHub
    {
        // define methods implemented by client
        // ReSharper disable InconsistentNaming

        Task refreshed(int id);

        // ReSharper restore InconsistentNaming
    }
}
