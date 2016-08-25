using LightInject;

namespace Umbraco.Core.Components
{
    public interface IUmbracoComponent
    {
        void Compose(ServiceContainer container);

        void Terminate();
    }
}
