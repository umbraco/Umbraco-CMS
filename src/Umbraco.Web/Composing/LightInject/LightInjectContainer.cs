using System;
using System.Web.Http;
using LightInject;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Web.Composing.LightInject
{
    /// <summary>
    /// Implements DI with LightInject.
    /// </summary>
    public class LightInjectContainer : Core.Composing.LightInject.LightInjectContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectContainer"/> with a LightInject container.
        /// </summary>
        protected LightInjectContainer(ServiceContainer container)
            : base(container)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="LightInjectContainer"/> class.
        /// </summary>
        public new static LightInjectContainer Create()
            => new LightInjectContainer(CreateServiceContainer());

        /// <inheritdoc />
        public override void ConfigureForWeb()
        {
            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedLightInjectScopeManagerProvider for explanations of what we are doing here
            if (!(Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            Container.EnableMvc(); // does container.EnablePerWebRequestScope()
            Container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)
            Container.EnableWebApi(GlobalConfiguration.Configuration);
        }
    }
}
