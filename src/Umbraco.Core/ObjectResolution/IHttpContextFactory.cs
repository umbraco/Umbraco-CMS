using System.Web;

namespace Umbraco.Core.ObjectResolution
{
    public interface IHttpContextFactory
    {
        HttpContextBase Context { get; }
    }
}