using System;
using System.Web.Http;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Composing.MSDI;

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
        protected LightInjectContainer(ServiceContainer container, IServiceCollection services)
            : base(container, services)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="LightInjectContainer"/> class.
        /// </summary>
        public new static DefaultServiceCollection Create()
            => new DefaultServiceCollection();

        public new static IFactory CreateFactory(IServiceCollection serviceCollection)
            => new LightInjectContainer(CreateServiceContainer(), serviceCollection);

        /// <inheritdoc />
        public override void ConfigureForWeb()
        {
            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedLightInjectScopeManagerProvider for explanations of what we are doing here
            if (!(LightinjectContainer.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            LightinjectContainer.EnableMvc(); // does container.EnablePerWebRequestScope()
            LightinjectContainer.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)
            LightinjectContainer.EnableWebApi(GlobalConfiguration.Configuration);
        }
    }
}
