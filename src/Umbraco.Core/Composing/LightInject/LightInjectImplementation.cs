/*********************************************************************************
    The MIT License (MIT)

    Copyright (c) 2016 bernhard.richter@gmail.com

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
    LightInject.Microsoft.DependencyInjection version 2.0.5
    http://www.lightinject.net/
    http://twitter.com/bernhardrichter
******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LightInject;
using Microsoft.Extensions.DependencyInjection;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1101:PrefixLocalCallsWithThis", Justification = "No inheritance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Single source file deployment.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1633:FileMustHaveHeader", Justification = "Custom header.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "All public members are documented.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:FieldsMustBePrivate", Justification = "Performance")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("MaintainabilityRules", "SA1403", Justification = "One source file")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("DocumentationRules", "SA1649", Justification = "One source file")]

namespace Umbraco.Core.Composing.LightInject
{
    /// <summary>
    /// Extends the <see cref="IServiceContainer"/> interface.
    /// </summary>
    public static class DependencyInjectionContainerExtensions
    {
        private static readonly MethodInfo GetInstanceMethod;
        private static readonly MethodInfo GetNamedInstanceMethod;

        static DependencyInjectionContainerExtensions()
        {
            GetInstanceMethod =
                typeof(IServiceFactory).GetTypeInfo().DeclaredMethods
                    .Single(m => m.Name == "GetInstance" && !m.IsGenericMethod && m.GetParameters().Length == 1);
            GetNamedInstanceMethod =
                typeof(IServiceFactory)
                    .GetTypeInfo().DeclaredMethods
                    .Single(m => m.Name == "GetInstance" && !m.IsGenericMethod && m.GetParameters().Length == 2 && m.GetParameters().Last().ParameterType == typeof(string));
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider"/> based on the given <paramref name="serviceCollection"/>.
        /// </summary>
        /// <param name="container">The target <see cref="IServiceContainer"/>.</param>
        /// <param name="serviceCollection">The <see cref="IServiceCollection"/> that contains information about the services to be registered.</param>
        /// <returns>A configured <see cref="IServiceProvider"/>.</returns>
        public static IServiceProvider CreateServiceProvider(this IServiceContainer container, IServiceCollection serviceCollection)
        {
            RegisterServices(container, serviceCollection);
            return container.GetInstance<IServiceProvider>().CreateScope().ServiceProvider;
        }

        private static void RegisterServices(IServiceContainer container, IServiceCollection serviceCollection)
        {
            container.Register<IServiceProvider>(factory => new LightInjectServiceProvider(container), new PerContainerLifetime());
            container.Register<IServiceScopeFactory>(factory => new LightInjectServiceScopeFactory(container), new PerContainerLifetime());
            var registrations = serviceCollection.Select(CreateServiceRegistration).ToList();

            var groupedRegistrations = registrations.GroupBy(sr => sr.ServiceType);
            foreach (var groupedRegistration in groupedRegistrations)
            {
                groupedRegistration.Last().ServiceName = string.Empty;
                if (!groupedRegistration.Key.GetTypeInfo().IsGenericTypeDefinition && groupedRegistration.Count() > 1)
                {
                    container.Register(CreateEnumerableServiceRegistration(groupedRegistration.Key, groupedRegistration));
                }
            }

            foreach (var registration in registrations)
            {
                container.Register(registration);
            }
        }

        private static ServiceRegistration CreateEnumerableServiceRegistration(
            Type elementType,
            IEnumerable<ServiceRegistration> serviceRegistrations)
        {
            var serviceFactoryParameter = Expression.Parameter(typeof(IServiceFactory), "serviceFactory");
            Type enumerableType = typeof(IEnumerable<>).MakeGenericType(elementType);
            var getInstanceExpressions = new List<Expression>();

            foreach (var serviceRegistration in serviceRegistrations)
            {
                getInstanceExpressions.Add(CreateGetInstanceExpression(serviceFactoryParameter, serviceRegistration.ServiceType, serviceRegistration.ServiceName));
            }

            var newArrayExpression = Expression.NewArrayInit(elementType, getInstanceExpressions);
            var lambdaExpression = Expression.Lambda(newArrayExpression, serviceFactoryParameter);

            ServiceRegistration enumerableRegistration = new ServiceRegistration();
            enumerableRegistration.ServiceType = enumerableType;
            enumerableRegistration.ServiceName = string.Empty;
            enumerableRegistration.FactoryExpression = lambdaExpression.Compile();
            return enumerableRegistration;
        }

        private static Expression CreateGetInstanceExpression(ParameterExpression serviceFactoryExpression, Type serviceType, string serviceName)
        {
            MethodCallExpression getInstanceMethodExpression;
            if (serviceName == string.Empty)
            {
                getInstanceMethodExpression = Expression.Call(serviceFactoryExpression, GetInstanceMethod, Expression.Constant(serviceType));
            }
            else
            {
                getInstanceMethodExpression = Expression.Call(serviceFactoryExpression, GetNamedInstanceMethod, Expression.Constant(serviceType), Expression.Constant(serviceName));
            }

            return Expression.Convert(getInstanceMethodExpression, serviceType);
        }

        private static ServiceRegistration CreateServiceRegistration(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationFactory != null)
            {
                return CreateServiceRegistrationForFactoryDelegate(serviceDescriptor);
            }

            if (serviceDescriptor.ImplementationInstance != null)
            {
                return CreateServiceRegistrationForInstance(serviceDescriptor);
            }

            return CreateServiceRegistrationServiceType(serviceDescriptor);
        }

        private static ServiceRegistration CreateServiceRegistrationServiceType(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.ImplementingType = serviceDescriptor.ImplementationType;
            return registration;
        }

        private static ServiceRegistration CreateServiceRegistrationForInstance(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.Value = serviceDescriptor.ImplementationInstance;
            return registration;
        }

        private static ServiceRegistration CreateServiceRegistrationForFactoryDelegate(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = CreateBasicServiceRegistration(serviceDescriptor);
            registration.FactoryExpression = CreateFactoryDelegate(serviceDescriptor);
            return registration;
        }

        private static ServiceRegistration CreateBasicServiceRegistration(ServiceDescriptor serviceDescriptor)
        {
            ServiceRegistration registration = new ServiceRegistration();
            registration.Lifetime = ResolveLifetime(serviceDescriptor);
            registration.ServiceType = serviceDescriptor.ServiceType;
            registration.ServiceName = Guid.NewGuid().ToString();
            return registration;
        }

        private static ILifetime ResolveLifetime(ServiceDescriptor serviceDescriptor)
        {
            if (serviceDescriptor.ImplementationInstance != null)
            {
                return null;
            }

            ILifetime lifetime = null;

            switch (serviceDescriptor.Lifetime)
            {
                case ServiceLifetime.Scoped:
                    lifetime = new PerScopeLifetime();
                    break;
                case ServiceLifetime.Singleton:
                    lifetime = new PerContainerLifetime();
                    break;
                case ServiceLifetime.Transient:
                    lifetime = new PerRequestLifeTime();
                    break;
            }

            return lifetime;
        }

        private static Delegate CreateFactoryDelegate(ServiceDescriptor serviceDescriptor)
        {
            var openGenericMethod = typeof(DependencyInjectionContainerExtensions).GetTypeInfo().GetDeclaredMethod("CreateTypedFactoryDelegate");
            var closedGenericMethod = openGenericMethod.MakeGenericMethod(serviceDescriptor.ServiceType);
            return (Delegate)closedGenericMethod.Invoke(null, new object[] { serviceDescriptor });
        }

        private static Func<IServiceFactory, T> CreateTypedFactoryDelegate<T>(ServiceDescriptor serviceDescriptor)
        {
            return serviceFactory => (T)serviceDescriptor.ImplementationFactory(serviceFactory.GetInstance<IServiceProvider>());
        }
    }

    /// <summary>
    /// An <see cref="IServiceProvider"/> that uses LightInject as the underlying container.
    /// </summary>
    internal class LightInjectServiceProvider : IServiceProvider
    {
        private readonly IServiceFactory serviceFactory;

        public LightInjectServiceProvider(IServiceFactory serviceFactory)
        {
            this.serviceFactory = serviceFactory;
        }

        /// <summary>
        /// Gets an instance of the given <paramref name="serviceType"/>.
        /// </summary>
        /// <param name="serviceType">The service type to return.</param>
        /// <returns>An instance of the given <paramref name="serviceType"/>.</returns>
        public object GetService(Type serviceType)
        {
            return serviceFactory.TryGetInstance(serviceType);
        }
    }

    internal class LightInjectServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IServiceContainer container;

        public LightInjectServiceScopeFactory(IServiceContainer container)
        {
            this.container = container;
        }

        public IServiceScope CreateScope()
        {
            return new LightInjectServiceScope(container.BeginScope());
        }
    }

    internal class LightInjectServiceScope : IServiceScope
    {
        private readonly Scope scope;

        public LightInjectServiceScope(Scope scope)
        {
            this.scope = scope;
            ServiceProvider = new LightInjectServiceProvider(scope);
        }

        public IServiceProvider ServiceProvider { get; }

        public void Dispose()
        {
            scope.Dispose();
        }
    }
}
