using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Dictionary;

namespace Umbraco.Web.Runtime
{
    // web's final composer composes after all user composers
    // and *also* after ICoreComposer (in case IUserComposer is disabled)
    [ComposeAfter(typeof(IUserComposer))]
    [ComposeAfter(typeof(ICoreComposer))]
    public class WebFinalComposer : ComponentComposer<WebFinalComponent>
    {

        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            // now that user composers have had a chance to register their own factory, we can add the result of the factory
            // to the container. 
            composition.Register(f => f.GetInstance<ICultureDictionaryFactory>().CreateDictionary(), Lifetime.Singleton);
        }

    }
}
