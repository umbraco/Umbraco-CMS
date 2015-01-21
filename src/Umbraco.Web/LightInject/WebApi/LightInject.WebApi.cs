/*****************************************************************************   
    The MIT License (MIT)

    Copyright (c) 2014 bernhard.richter@gmail.com

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
******************************************************************************
    LightInject.WebApi version 1.0.0.3
    http://www.lightinject.net/
    http://twitter.com/bernhardrichter    
******************************************************************************/

using Umbraco.Core.LightInject;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1403:FileMayOnlyContainASingleNamespace", Justification = "Extension methods must be visible")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]

namespace Umbraco.Web.LightInject
{
    using System.Linq;
    using System.Reflection;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;
    using LightInject.WebApi;

    /// <summary>
    /// Extends the <see cref="IServiceContainer"/> interface with methods that 
    /// enables dependency injection in a Web API application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class WebApiContainerExtensions
    {
        /// <summary>
        /// Enables dependency injection in a Web API application.
        /// </summary>
        /// <param name="serviceContainer">The target <see cref="IServiceContainer"/>.</param>
        /// <param name="httpConfiguration">The <see cref="HttpConfiguration"/> that represents the configuration of this Web API application.</param>
        public static void EnableWebApi(this IServiceContainer serviceContainer, HttpConfiguration httpConfiguration)
        {
            httpConfiguration.DependencyResolver = new LightInjectWebApiDependencyResolver(serviceContainer);
            var provider = httpConfiguration.Services.GetFilterProviders();
            httpConfiguration.Services.RemoveAll(typeof(IFilterProvider), o => true);
            httpConfiguration.Services.Add(typeof(IFilterProvider), new LightInjectWebApiFilterProvider(serviceContainer, provider));
        }

        /// <summary>
        /// Registers all <see cref="ApiController"/> implementations found in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="assemblies">A list of assemblies from which to register <see cref="ApiController"/> implementations.</param>
        public static void RegisterApiControllers(this IServiceRegistry serviceRegistry, params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                var controllerTypes = assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IHttpController).IsAssignableFrom(t));
                foreach (var controllerType in controllerTypes)
                {
                    serviceRegistry.Register(controllerType, new PerRequestLifeTime());
                }
            }
        }

        /// <summary>
        /// Registers all <see cref="ApiController"/> implementations found in this assembly.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        public static void RegisterApiControllers(this IServiceRegistry serviceRegistry)
        {
            RegisterApiControllers(serviceRegistry, Assembly.GetCallingAssembly());            
        }
    }
}

namespace Umbraco.Web.LightInject.WebApi
{
    using System;    
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Http;
    using System.Web.Http.Controllers;
    using System.Web.Http.Dependencies;
    using System.Web.Http.Filters;

    /// <summary>
    /// An <see cref="IDependencyResolver"/> adapter for the LightInject service container 
    /// that enables <see cref="ApiController"/> instances and their dependencies to be 
    /// resolved through the service container.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LightInjectWebApiDependencyResolver : IDependencyResolver
    {
        private readonly IServiceContainer serviceContainer;        

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectWebApiDependencyResolver"/> class.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="IServiceContainer"/> instance to 
        /// be used for resolving service instances.</param>
        internal LightInjectWebApiDependencyResolver(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
        }

        /// <summary>
        /// Disposes the underlying <see cref="IServiceContainer"/>.
        /// </summary>
        public void Dispose()
        {
            serviceContainer.Dispose();
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of the requested service.</param>
        /// <returns>The requested service instance if available, otherwise null.</returns>                
        public object GetService(Type serviceType)
        {
            return serviceContainer.TryGetInstance(serviceType);
        }

        /// <summary>
        /// Gets all instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The type of services to resolve.</param>
        /// <returns>A list that contains all implementations of the <paramref name="serviceType"/>.</returns>                
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return serviceContainer.GetAllInstances(serviceType);
        }

        /// <summary>
        /// Starts a new <see cref="IDependencyScope"/> that represents 
        /// the scope for services registered with <see cref="PerScopeLifetime"/>.
        /// </summary>
        /// <returns>
        /// A new <see cref="IDependencyScope"/>.
        /// </returns>
        public IDependencyScope BeginScope()
        {
            return new LightInjectWebApiDependencyScope(serviceContainer, serviceContainer.BeginScope());
        }
    }

    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LightInjectWebApiDependencyScope : IDependencyScope
    {
        private readonly IServiceContainer serviceContainer;
        private readonly Scope scope;

        public LightInjectWebApiDependencyScope(IServiceContainer serviceContainer, Scope scope)
        {
            this.serviceContainer = serviceContainer;
            this.scope = scope;
        }

        public object GetService(Type serviceType)
        {
            return serviceContainer.GetInstance(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return serviceContainer.GetAllInstances(serviceType);
        }

        public void Dispose()
        {
            scope.Dispose();
        }
    }

    /// <summary>
    /// A <see cref="IFilterProvider"/> that uses an <see cref="IServiceContainer"/>    
    /// to inject property dependencies into <see cref="IFilter"/> instances.
    /// </summary>    
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LightInjectWebApiFilterProvider : IFilterProvider
    {
        private readonly IServiceContainer serviceContainer;
        private readonly IEnumerable<IFilterProvider> filterProviders;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectWebApiFilterProvider"/> class.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="IServiceContainer"/> instance 
        /// used to inject property dependencies.</param>
        /// <param name="filterProviders">The list of existing filter providers.</param>
        public LightInjectWebApiFilterProvider(IServiceContainer serviceContainer, IEnumerable<IFilterProvider> filterProviders)
        {
            this.serviceContainer = serviceContainer;
            this.filterProviders = filterProviders;
        }

        /// <summary>
        /// Returns an enumeration of filters.
        /// </summary>
        /// <returns>
        /// An enumeration of filters.
        /// </returns>
        /// <param name="configuration">The HTTP configuration.</param><param name="actionDescriptor">The action descriptor.</param>
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {                        
            var filters = filterProviders.SelectMany(p => p.GetFilters(configuration, actionDescriptor)).ToArray();

            foreach (var filterInfo in filters)
            {
                serviceContainer.InjectProperties(filterInfo.Instance);
            }

            return filters;
        }
    }
}