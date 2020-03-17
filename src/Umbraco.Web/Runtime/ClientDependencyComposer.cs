using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;
using Umbraco.Web.JavaScript;

namespace Umbraco.Web.Runtime
{
    public sealed class ClientDependencyComposer : ComponentComposer<ClientDependencyComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.RegisterUnique<IRuntimeMinifier, ClientDependencyRuntimeMinifier>();
        }
    }
}
