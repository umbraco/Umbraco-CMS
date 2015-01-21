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
    LightInject.Mvc version 1.0.0.4
    http://seesharper.github.io/LightInject/
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
    using System.Web.Mvc;
    using LightInject.Mvc;

    /// <summary>
    /// Extends the <see cref="IServiceContainer"/> interface with a method that 
    /// enables dependency injection in an ASP.NET MVC application.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal static class MvcContainerExtensions
    {        
        /// <summary>
        /// Registers all <see cref="Controller"/> implementations found in the given <paramref name="assemblies"/>.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        /// <param name="assemblies">A list of assemblies from which to register <see cref="Controller"/> implementations.</param>
        public static void RegisterControllers(this IServiceRegistry serviceRegistry, params Assembly[] assemblies)
        {                                    
            foreach (var assembly in assemblies)
            {
                var controllerTypes =
                    assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IController).IsAssignableFrom(t));
                foreach (var controllerType in controllerTypes)
                {
                    serviceRegistry.Register(controllerType, new PerRequestLifeTime());
                }                
            }
        }

        /// <summary>
        /// Registers all <see cref="Controller"/> implementations found in this assembly.
        /// </summary>
        /// <param name="serviceRegistry">The target <see cref="IServiceRegistry"/>.</param>
        public static void RegisterControllers(this IServiceRegistry serviceRegistry)
        {
            RegisterControllers(serviceRegistry, Assembly.GetCallingAssembly());            
        }

        /// <summary>
        /// Enables dependency injection in an ASP.NET MVC application.
        /// </summary>
        /// <param name="serviceContainer">The target <see cref="IServiceContainer"/>.</param>
        public static void EnableMvc(this IServiceContainer serviceContainer)
        {
            ((ServiceContainer)serviceContainer).EnablePerWebRequestScope();
            SetDependencyResolver(serviceContainer);
            InitializeFilterAttributeProvider(serviceContainer);
        }

        private static void SetDependencyResolver(IServiceContainer serviceContainer)
        {
            DependencyResolver.SetResolver(new LightInjectMvcDependencyResolver(serviceContainer));
        }

        private static void InitializeFilterAttributeProvider(IServiceContainer serviceContainer)
        {
            RemoveExistingFilterAttributeFilterProviders();
            var filterProvider = new LightInjectFilterProvider(serviceContainer);
            FilterProviders.Providers.Add(filterProvider);
        }

        private static void RemoveExistingFilterAttributeFilterProviders()
        {
            var existingFilterAttributeProviders =
                FilterProviders.Providers.OfType<FilterAttributeFilterProvider>().ToArray();
            foreach (FilterAttributeFilterProvider filterAttributeFilterProvider in existingFilterAttributeProviders)
            {
                FilterProviders.Providers.Remove(filterAttributeFilterProvider);
            }
        }
    }
}

namespace Umbraco.Web.LightInject.Mvc
{
    using System;    
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    
    /// <summary>
    /// An <see cref="IDependencyResolver"/> adapter for the LightInject service container.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LightInjectMvcDependencyResolver : IDependencyResolver
    {
        private readonly IServiceContainer serviceContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectMvcDependencyResolver"/> class.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="IServiceContainer"/> instance to 
        /// be used for resolving service instances.</param>
        internal LightInjectMvcDependencyResolver(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
        }

        /// <summary>
        /// Resolves singly registered services that support arbitrary object creation.
        /// </summary>
        /// <returns>
        /// The requested service or object.
        /// </returns>
        /// <param name="serviceType">The type of the requested service or object.</param>
        public object GetService(Type serviceType)
        {
            return serviceContainer.TryGetInstance(serviceType);
        }

        /// <summary>
        /// Resolves multiply registered services.
        /// </summary>
        /// <returns>
        /// The requested services.
        /// </returns>
        /// <param name="serviceType">The type of the requested services.</param>
        public IEnumerable<object> GetServices(Type serviceType)
        {
            return serviceContainer.GetAllInstances(serviceType);
        }
    }

    /// <summary>
    /// A <see cref="FilterAttributeFilterProvider"/> that uses an <see cref="IServiceContainer"/>    
    /// to inject property dependencies into <see cref="Filter"/> instances.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    internal class LightInjectFilterProvider : FilterAttributeFilterProvider
    {
        private readonly IServiceContainer serviceContainer;

        /// <summary>
        /// Initializes a new instance of the <see cref="LightInjectFilterProvider"/> class.
        /// </summary>
        /// <param name="serviceContainer">The <see cref="IServiceContainer"/> instance 
        /// used to inject property dependencies.</param>
        public LightInjectFilterProvider(IServiceContainer serviceContainer)
        {
            this.serviceContainer = serviceContainer;
            serviceContainer.RegisterInstance<IFilterProvider>(this);
        }

        /// <summary>
        /// Aggregates the filters from all of the filter providers into one collection.
        /// </summary>
        /// <returns>
        /// The collection filters from all of the filter providers.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param>
        /// <param name="actionDescriptor">The action descriptor.</param>
        public override IEnumerable<Filter> GetFilters(ControllerContext controllerContext, ActionDescriptor actionDescriptor)
        {
            var filters = base.GetFilters(controllerContext, actionDescriptor).ToArray();
            foreach (var filter in filters)
            {
                serviceContainer.InjectProperties(filter.Instance);                
            }

            return filters;
        }                
    }
}