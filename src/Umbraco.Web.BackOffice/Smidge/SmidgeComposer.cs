using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Runtime;

namespace Umbraco.Web.BackOffice.Smidge
{
    public sealed class SmidgeComposer : IComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IRuntimeMinifier, SmidgeRuntimeMinifier>(Lifetime.Scope);
        }
    }
}
