using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Core.Composing.MSDI.Named
{
    /*
    MIT License

    Copyright (c) 2019 James Jackson-South

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
    */

    /// <summary>
    /// Extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a service/implementation relationship to the collectior for the given target type. 
        /// </summary>
        /// <typeparam name="TService">The type of service to add.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <typeparam name="TTarget">The target type that this implementation should be used for.</typeparam>
        /// <param name="serviceCollection">The collection of service descriptors.</param>
        /// <param name="serviceLifetime">The lifetime of the service.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddServiceFor<TService, TImplementation, TTarget>(
            this IServiceCollection serviceCollection,
            ServiceLifetime serviceLifetime,
            ServiceLifetime targetLifetime)
            where TImplementation : class
            where TTarget : class
        {
            string name = typeof(TImplementation).FullName + ":" + typeof(TTarget).FullName;
            AddNamedServiceImpl<TService, TImplementation>(serviceCollection, name, serviceLifetime);

            // Now register our target type. 
            // This uses our factory to determine the type to tell the provider to resolve for our specified type
            // and resolves all other types as normal via the provider.
            serviceCollection.Add(new ServiceDescriptor(
                typeof(TTarget),
                provider =>
                ActivatorUtilities.CreateInstance<TTarget>(provider, provider.GetNamedService<TService>(name)),
                targetLifetime));

            return serviceCollection;
        }

        /// <summary>
        /// Adds a service/implementation relationship to the collection, keyed to the given name. 
        /// </summary>
        /// <typeparam name="TService">The type of service to add.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <param name="serviceCollection">The collection of service descriptors.</param>
        /// <param name="name">The name to register the service against.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddNamedService<TService, TImplementation>(this IServiceCollection serviceCollection, string name, ServiceLifetime lifetime)
            where TImplementation : class
        {
            AddNamedServiceImpl<TService, TImplementation>(serviceCollection, name, lifetime);
            return serviceCollection;
        }

        /// <summary>
        /// Adds a service to the collection specifying what named dependencies to assign to named parameters.
        /// </summary>
        /// <typeparam name="TService">The type of service to add.</typeparam>
        /// <typeparam name="TImplementation">The implementation type for the service.</typeparam>
        /// <param name="serviceCollection">The collection of service descriptors.</param>
        /// <param name="lifetime">The lifetime of the service.</param>
        /// <param name="dependencies">
        /// The collection of named dependencies.
        /// {registeredType, registeredName, parameterName}
        /// </param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddServiceWithNamedDependencies<TService, TImplementation>(
            this IServiceCollection serviceCollection,
            ServiceLifetime lifetime,
            params NamedDependency[] dependencies)
            where TImplementation : class
        {
            INamedServiceFactory[] factories = serviceCollection.Where(x => typeof(INamedServiceFactory).IsAssignableFrom(x.ServiceType))
                .Select(x => x.ImplementationInstance).Cast<INamedServiceFactory>().ToArray();

            // Gather a set of parameters from our target ttype that best match our named dependencies.
            ParameterInfo[] parameters = GetMatchingConstructorParameters<TImplementation>(out ConstructorInfo constructorInfo, dependencies);

            // Create an array of parameter factories that we can pass to our target factory.
            // We can reuse this for every request keeping overheads low and performance high.
            var argsFactory = new Func<IServiceProvider, object>[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
            {
                ParameterInfo parameter = parameters[i];
                string parameterName = parameter.Name;
                Type parameterType = parameter.ParameterType;

                NamedDependency dependency = Array.Find(dependencies, x => x.ParameterName == parameterName && parameterType.IsAssignableFrom(x.ServiceType));
                if (dependency != default)
                {
                    INamedServiceFactory factory = Array.Find(factories, x => x.ServiceType == dependency.ServiceType);
                    if (factory != null)
                    {
                        argsFactory[i] = p => factory.Resolve(dependency.ServiceName, p);
                        continue;
                    }
                }

                argsFactory[i] = p => p.GetService(parameterType);
            }

            serviceCollection.Add(new ServiceDescriptor(typeof(TService), provider =>
            {
                object[] args = new object[argsFactory.Length];
                for (int i = 0; i < argsFactory.Length; i++)
                {
                    args[i] = argsFactory[i].Invoke(provider);
                }

                return constructorInfo.Invoke(args);
            }, lifetime));

            return serviceCollection;
        }

        /// <summary>
        /// Returns an instance of the service type matching the given name.
        /// </summary>
        /// <typeparam name="TService">The type of service to return.</typeparam>
        /// <param name="provider">The service provider for retrieving service objects.</param>
        /// <param name="name">The name the service type is registered as.</param>
        /// <returns>The <see cref="TService"/>.</returns>
        public static TService GetNamedService<TService>(this IServiceProvider provider, string name)
        {
            NamedServiceFactory<TService> factory = provider.GetServices<NamedServiceFactory<TService>>().LastOrDefault();
            if (factory is null)
            {
                throw new InvalidOperationException($"No service for type {typeof(TService)} named '{name}' has been registered.");
            }

            return factory.Resolve(provider, name);
        }

        private static void AddNamedServiceImpl<TService, TImplementation>(IServiceCollection serviceCollection, string name, ServiceLifetime lifetime)
            where TImplementation : class
        {
            ServiceDescriptor descriptor = serviceCollection.LastOrDefault(x => x.ServiceType == typeof(NamedServiceFactory<TService>));
            var factory = descriptor?.ImplementationInstance as NamedServiceFactory<TService>;
            if (factory is null)
            {
                factory = new NamedServiceFactory<TService>();
                serviceCollection.AddSingleton(factory);
            }

            factory.Register<TImplementation>(name);

            // We don't want to register using the service descriptor since that would mean multiple TService types
            // would be registered causing resolution problems for non-named registrations.
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    serviceCollection.AddSingleton<TImplementation>();
                    break;
                case ServiceLifetime.Scoped:
                    serviceCollection.AddScoped<TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    serviceCollection.AddTransient<TImplementation>();
                    break;
            }
        }

        private static ParameterInfo[] GetMatchingConstructorParameters<T>(out ConstructorInfo constructorInfo, params NamedDependency[] dependencies)
        {
            int bestMatch = 0;
            constructorInfo = null;
            ParameterInfo[] parameters = Array.Empty<ParameterInfo>();
            ConstructorInfo[] constructorInfos = typeof(T).GetConstructors();

            // Loop through the constructors. 
            // We look for the constructor that has largest number of matching parameters.
            for (int i = 0; i < constructorInfos.Length; i++)
            {
                int currentMatch = 0;
                ParameterInfo[] currentParameters = constructorInfos[i].GetParameters();
                for (int j = 0; j < currentParameters.Length; j++)
                {
                    ParameterInfo parameter = currentParameters[j];
                    for (int k = 0; k < dependencies.Length; k++)
                    {
                        NamedDependency dependency = dependencies[k];
                        if (parameter.Name == dependency.ParameterName
                            && parameter.ParameterType.IsAssignableFrom(dependency.ServiceType))
                        {
                            currentMatch++;
                        }
                    }

                    if (currentMatch > bestMatch)
                    {
                        bestMatch = currentMatch;
                        parameters = currentParameters;
                        constructorInfo = constructorInfos[i];
                    }
                }
            }

            return parameters;
        }
    }
}
