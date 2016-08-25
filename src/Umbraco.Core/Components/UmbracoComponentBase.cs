using LightInject;

namespace Umbraco.Core.Components
{
    public abstract class UmbracoComponentBase : IUmbracoComponent
    {
        public virtual void Compose(ServiceContainer container)
        { }

        public virtual void Terminate()
        { }
    }
}
