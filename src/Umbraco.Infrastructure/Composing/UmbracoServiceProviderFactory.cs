using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using Umbraco.Core.Composing.LightInject;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Used to create Umbraco's container and cross-wire it up before the applicaton starts
    /// </summary>
    public class UmbracoServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
    {
        public UmbracoServiceProviderFactory(ServiceContainer container)
        {            
            _container = new LightInjectContainer(container);
        }

        /// <summary>
        /// Default ctor for use in Host Builder configuration
        /// </summary>
        public UmbracoServiceProviderFactory()
        {
            var container = new ServiceContainer(ContainerOptions.Default.Clone().WithMicrosoftSettings().WithAspNetCoreSettings());
            UmbracoContainer = _container = new LightInjectContainer(container);
            IsActive = true;
        }

        // see here for orig lightinject version https://github.com/seesharper/LightInject.Microsoft.DependencyInjection/blob/412566e3f70625e6b96471db5e1f7cd9e3e1eb18/src/LightInject.Microsoft.DependencyInjection/LightInject.Microsoft.DependencyInjection.cs#L263
        // we don't really need all that, we're manually creating our container with the correct options and that
        // is what we'll return in CreateBuilder

        IServiceCollection _services;
        readonly LightInjectContainer _container;

        internal LightInjectContainer GetContainer() => _container;

        /// <summary>
        /// When the empty ctor is used this returns if this factory is active
        /// </summary>
        public static bool IsActive { get; private set; }

        /// <summary>
        /// When the empty ctor is used this returns the created IRegister
        /// </summary>
        public static IRegister UmbracoContainer { get; private set; }

        /// <summary>
        /// Create the container with the required settings for aspnetcore3
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public IServiceContainer CreateBuilder(IServiceCollection services)
        {
            _services = services;            
            return _container.Container;
        }

        /// <summary>
        /// This cross-wires the container just before the application calls "Configure"
        /// </summary>
        /// <param name="containerBuilder"></param>
        /// <returns></returns>
        public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        {
            var provider = containerBuilder.CreateServiceProvider(_services);
            return provider;
        }

    }
}
