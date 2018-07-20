using System;
using System.Web.Http;
using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Composing.LightInject
{
    /// <summary>
    /// Implements <see cref="IContainer"/> with LightInject.
    /// </summary>
    public class LightInjectContainer : Core.Composing.LightInject.LightInjectContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        public LightInjectContainer(ServiceContainer container)
            : base(container)
        { }

        /// <inheritdoc />
        public override void ConfigureForWeb()
        {
            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedLightInjectScopeManagerProvider for explainations of what we are doing here
            if (!(Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            Container.EnableMvc(); // does container.EnablePerWebRequestScope()
            Container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)
            Container.EnableWebApi(GlobalConfiguration.Configuration);
        }
    }
}
