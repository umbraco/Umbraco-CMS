using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Composing;
using Umbraco.Core;
namespace Umbraco.Web.Routing
{
    public class UrlProviderComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IContextUrlProviderFactory, ContextUrlProviderFactory>(Lifetime.Singleton);
        }
    }
}
