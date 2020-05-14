using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Extensions;
using Umbraco.Web.BackOffice.Routing;

namespace Umbraco.Web.BackOffice.Runtime
{
    public class BackOfficeComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<BackOfficeAreaRoutes>();
        }
    }
}
