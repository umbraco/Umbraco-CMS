using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

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
