using System.Web.Http;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Runtime
{
    public class WebFinalComponent : IComponent
    {
        public void Initialize()
        {
            // ensure WebAPI is initialized, after everything
            GlobalConfiguration.Configuration.EnsureInitialized();
        }

        public void Terminate()
        { }
    }
}
