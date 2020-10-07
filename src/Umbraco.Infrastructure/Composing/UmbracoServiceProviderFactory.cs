using LightInject;
using LightInject.Microsoft.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Umbraco.Composing;
using Umbraco.Core.Composing.LightInject;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;

namespace Umbraco.Core.Composing
{
    /// <summary>
    /// Used to create Umbraco's container and cross-wire it up before the applicaton starts
    /// </summary>
    public class UmbracoServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
    {
        public UmbracoServiceProviderFactory(ServiceContainer container, bool initializeCurrent)
        {
            _container = new LightInjectContainer(container);
            _initializeCurrent = initializeCurrent;
        }

        /// <summary>
        /// Creates an ASP.NET Core compatible service container
        /// </summary>
        /// <returns></returns>
        public static ServiceContainer CreateServiceContainer() => new ServiceContainer(
            ContainerOptions.Default.Clone()
                .WithMicrosoftSettings()
                //.WithAspNetCoreSettings() //TODO WithAspNetCoreSettings changes behavior that we need to discuss
            );

        /// <summary>
        /// Default ctor for use in Host Builder configuration
        /// </summary>
        public UmbracoServiceProviderFactory()
        {
            var container = CreateServiceContainer();
            UmbracoContainer = _container = new LightInjectContainer(container);
            IsActive = true;
            _initializeCurrent = true;
        }

        // see here for orig lightinject version https://github.com/seesharper/LightInject.Microsoft.DependencyInjection/blob/412566e3f70625e6b96471db5e1f7cd9e3e1eb18/src/LightInject.Microsoft.DependencyInjection/LightInject.Microsoft.DependencyInjection.cs#L263
        // we don't really need all that, we're manually creating our container with the correct options and that
        // is what we'll return in CreateBuilder

        IServiceCollection _services;
        readonly LightInjectContainer _container;
        private readonly bool _initializeCurrent;

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

            if (_initializeCurrent)
            {
                // after cross wiring, configure "Current"
                Current.Initialize(
                    _container.GetInstance<ILogger<object>>(),
                    _container.GetInstance<IOptions<SecuritySettings>>().Value,
                    _container.GetInstance<IOptions<GlobalSettings>>().Value,
                    _container.GetInstance<IIOHelper>(),
                    _container.GetInstance<Umbraco.Core.Hosting.IHostingEnvironment>(),
                    _container.GetInstance<IBackOfficeInfo>(),
                    _container.GetInstance<Umbraco.Core.Logging.IProfiler>());
            }


            return provider;
        }

    }
}
