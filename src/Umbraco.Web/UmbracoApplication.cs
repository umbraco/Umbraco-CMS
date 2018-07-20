using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Runtime;

namespace Umbraco.Web
{
    /// <summary>
    /// Represents the Umbraco global.asax class.
    /// </summary>
    public class UmbracoApplication : UmbracoApplicationBase
    {
        protected override IRuntime GetRuntime()
        {
            return new WebRuntime(this);
        }

        protected override IContainer GetContainer()
        {
            return new Web.Composing.LightInject.LightInjectContainer(new LightInject.ServiceContainer());
        }
    }
}
