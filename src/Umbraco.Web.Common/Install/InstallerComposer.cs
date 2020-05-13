using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Install
{
    public class InstallerComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<InstallAreaRoutes>();
        }
    }
}
