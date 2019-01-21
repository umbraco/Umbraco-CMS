using System;
using System.Collections.Generic;
using System.Linq;
using LightInject;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Reflection;
using ServiceMap = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<string, LightInject.ServiceRegistration>>;

/*********************************************************************************
    The MIT License (MIT)

    Copyright (c) 2017 bernhard.richter@gmail.com

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
    LightInject.Validation version 1.0.1
    http://www.lightinject.net/
    http://twitter.com/bernhardrichter
******************************************************************************/

namespace Umbraco.Tests.Composing
{
    public static class LightInjectValidation
    {
        private static readonly ConcurrentDictionary<Type, int> LifeSpans = new ConcurrentDictionary<Type, int>();

        private const string NotDisposeMessageServiceType =
            @"The service {0} is being injected as a constructor argument into {1} implements IDisposable, " +
            "but is registered without a lifetime (transient). LightInject will not be able to dispose the instance represented by {0}. " +
            "If the intent was to manually control the instantiation and destruction, inject Func<{0}> instead. " +
            "Otherwise register `{0}` with a lifetime (PerContainer, PerRequest or PerScope).";

        private const string NotDisposeMessageImplementingType =
            @"The service {0} represented by {1} is being injected as a constructor argument into {2} implements IDisposable, " +
            "but is registered without a lifetime (transient). LightInject will not be able to dispose the instance represented by {0}. " +
            "If the intent was to manually control the instantiation and destruction, inject Func<{0}> instead. " +
            "Otherwise register `{0}` with a lifetime (PerContainer, PerRequest or PerScope).";


        private const string MissingDeferredDependency =
            @"The injected '{0}' does not contain a registration for the underlying type '{1}'. " +
            "Ensure that '{1}' is registered so that the service can be resolved by '{0}'";

        /*
        The service 'NameSpace.IBar' that is being injected into 'NameSpace.Foo' is registered with
with a 'Transient' lifetime while the 'NameSpace.Foo' is registered with the 'PerScope' lifetime.
Ensure that 'NameSpace.IBar' is registered with a lifetime that is equal to or has a longer lifetime than the 'PerScope' lifetime.
        */
        private const string CaptiveDependency =
            @"The service '{0}' that is being injected into {1} is registered with " +
            "a '{2}' lifetime while the {1} is registered with the '{3}' lifetime. " +
            "Ensure that '{0}' is registered with a lifetime that is equal to or has a longer lifetime than the '{3}' lifetime. " +
            "Alternatively ensure that `{1}` is registered with a lifetime that is equal to or " +
            "has a shorter lifetime than `{2}` lifetime.";

        private const string MissingDependency =
                "Class: 'NameSpace.Foo', Parameter 'NameSpace.IBar bar' -> The injected service NameSpace IBar is not registered."
            ;


        static LightInjectValidation()
        {
            LifeSpans.TryAdd(typeof(PerRequestLifeTime), 10);
            LifeSpans.TryAdd(typeof(PerScopeLifetime), 20);
            LifeSpans.TryAdd(typeof(PerContainerLifetime), 30);
        }

        public static IEnumerable<ValidationResult> Validate(this ServiceContainer container)
        {
            var serviceMap = container.AvailableServices.GroupBy(sr => sr.ServiceType).ToDictionary(gr => gr.Key,
                gr => gr.ToDictionary(sr => sr.ServiceName, sr => sr, StringComparer.OrdinalIgnoreCase));

            var verifyableServices = container.AvailableServices.Where(sr => sr.ImplementingType != null);

            return verifyableServices.SelectMany(sr =>
                ValidateConstructor(serviceMap, sr, container.ConstructorSelector.Execute(sr.ImplementingType)));
        }

        private static IReadOnlyCollection<ValidationResult> ValidateConstructor(ServiceMap serviceMap,
            ServiceRegistration serviceRegistration, ConstructorInfo constructorInfo)
        {
            var result = new Collection<ValidationResult>();

            foreach (var parameter in constructorInfo.GetParameters())
            {
                var validationTarget = new ValidationTarget(serviceRegistration, parameter);
                Validate(validationTarget, serviceMap, result);
            }
            return result;
        }

        private static void Validate(ValidationTarget validationTarget, ServiceMap serviceMap, ICollection<ValidationResult> result)
        {
            var registration = GetServiceRegistration(serviceMap, validationTarget);
            if (registration == null)
            {
                if (validationTarget.ServiceType.IsFunc() || validationTarget.ServiceType.IsLazy())
                {
                    var serviceType = validationTarget.ServiceType.GenericTypeArguments[0];
                    var underlyingvalidationTarget = validationTarget.WithServiceDescription(serviceType, string.Empty);
                    registration = GetServiceRegistration(serviceMap, underlyingvalidationTarget);

                    if (registration != null)
                    {
                        return;
                    }

                    if (serviceMap.ContainsAmbiguousRegistrationFor(serviceType))
                    {
                        result.Add(new ValidationResult("", ValidationSeverity.Ambiguous, underlyingvalidationTarget));
                    }
                    else
                    {
                        string message = string.Format(MissingDeferredDependency, validationTarget.ServiceType, underlyingvalidationTarget.ServiceType);
                        result.Add(new ValidationResult(message, ValidationSeverity.MissingDependency, underlyingvalidationTarget));
                    }
                }
                else if (validationTarget.ServiceType.IsGenericType && validationTarget.ServiceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var serviceType = validationTarget.ServiceType.GenericTypeArguments[0];
                    var underlyingvalidationTarget = validationTarget.WithServiceDescription(serviceType, string.Empty);
                    var registrations = GetServiceRegistrations(serviceMap, underlyingvalidationTarget);
                    if (registrations.Any()) return;

                    // strict: there has to be at least 1
                    string message = string.Format(MissingDeferredDependency, validationTarget.ServiceType, underlyingvalidationTarget.ServiceType);
                    result.Add(new ValidationResult(message, ValidationSeverity.MissingDependency, underlyingvalidationTarget));
                }
                else
                {
                    if (serviceMap.ContainsAmbiguousRegistrationFor(validationTarget.ServiceType))
                    {
                        result.Add(new ValidationResult("", ValidationSeverity.Ambiguous, validationTarget));
                    }
                    else
                    {
                        result.Add(new ValidationResult("", ValidationSeverity.MissingDependency, validationTarget));
                    }
                }
            }
            else
            {
                ValidateDisposable(validationTarget, result, registration);
                ValidateLifetime(validationTarget, registration, result);
            }
        }

        private static void ValidateDisposable(ValidationTarget validationTarget, ICollection<ValidationResult> result,
            ServiceRegistration registration)
        {
            if (registration.ServiceType.Implements<IDisposable>())
            {
                var message = string.Format(NotDisposeMessageServiceType, registration.ServiceType,
                    validationTarget.DeclaringService.ImplementingType);
                result.Add(new ValidationResult(message, ValidationSeverity.NotDisposed, validationTarget));
            }

            else if (registration.ImplementingType != null && registration.ImplementingType.Implements<IDisposable>())
            {
                var message = string.Format(NotDisposeMessageImplementingType, registration.ImplementingType,
                    registration.ServiceType,
                    validationTarget.DeclaringService.ImplementingType);
                result.Add(new ValidationResult(message, ValidationSeverity.NotDisposed, validationTarget));
            }
        }


        private static void ValidateLifetime(ValidationTarget validationTarget, ServiceRegistration dependencyRegistration, ICollection<ValidationResult> result)
        {
            if (GetLifespan(validationTarget.DeclaringService.Lifetime) > GetLifespan(dependencyRegistration.Lifetime))
            {
                var message = string.Format(CaptiveDependency, dependencyRegistration.ServiceType,
                    validationTarget.DeclaringService.ServiceType, GetLifetimeName(dependencyRegistration.Lifetime),
                    GetLifetimeName(validationTarget.DeclaringService.Lifetime));
                result.Add(new ValidationResult(message, ValidationSeverity.Captive, validationTarget));
            }
        }

        public static void SetLifespan<TLifetime>(int lifeSpan) where TLifetime : ILifetime
        {
            LifeSpans.TryAdd(typeof(TLifetime), lifeSpan);
        }

        private static IEnumerable<ServiceRegistration> GetServiceRegistrations(ServiceMap serviceMap, ValidationTarget validationTarget)
        {
            return serviceMap.Where(x => validationTarget.ServiceType.IsAssignableFrom(x.Key)).SelectMany(x => x.Value.Values);
        }

        private static ServiceRegistration GetServiceRegistration(ServiceMap serviceMap, ValidationTarget validationTarget)
        {
            if (!serviceMap.TryGetValue(validationTarget.ServiceType, out var registrations))
            {
                return null;
            }

            if (registrations.TryGetValue(string.Empty, out var registration))
            {
                return registration;
            }

            if (registrations.Count == 1)
            {
                return registrations.Values.First();
            }

            if (registrations.TryGetValue(validationTarget.ServiceName, out registration))
            {
                return registration;
            }

            return null;
        }

        private static string GetLifetimeName(ILifetime lifetime)
        {
            if (lifetime == null)
            {
                return "Transient";
            }
            return lifetime.GetType().Name;
        }

        private static int GetLifespan(ILifetime lifetime)
        {
            if (lifetime == null)
            {
                return 0;
            }
            if (LifeSpans.TryGetValue(lifetime.GetType(), out var lifespan))
            {
                return lifespan;
            }
            return 0;
        }
    }


    public class ValidationTarget
    {
        public ServiceRegistration DeclaringService { get; }
        public ParameterInfo Parameter { get; }
        public Type ServiceType { get; }
        public string ServiceName { get; }


        public ValidationTarget(ServiceRegistration declaringRegistration, ParameterInfo parameter) : this(declaringRegistration, parameter, parameter.ParameterType, string.Empty)
        {
        }


        public ValidationTarget(ServiceRegistration declaringService, ParameterInfo parameter, Type serviceType, string serviceName)
        {
            DeclaringService = declaringService;
            Parameter = parameter;
            ServiceType = serviceType;
            ServiceName = serviceName;


            if (serviceType.GetTypeInfo().IsGenericType && serviceType.GetTypeInfo().ContainsGenericParameters)
            {
                ServiceType = serviceType.GetGenericTypeDefinition();
            }

        }

        public ValidationTarget WithServiceDescription(Type serviceType, string serviceName)
        {
            return new ValidationTarget(DeclaringService, Parameter, serviceType, serviceName);
        }

    }





    public class ValidationResult
    {
        public ValidationResult(string message, ValidationSeverity severity, ValidationTarget validationTarget)
        {
            Message = message;
            Severity = severity;
            ValidationTarget = validationTarget;
        }

        public string Message { get; }

        public ValidationSeverity Severity { get; }
        public ValidationTarget ValidationTarget { get; }
    }

    public enum ValidationSeverity
    {
        NoIssues,
        Captive,
        NotDisposed,
        MissingDependency,
        Ambiguous
    }

    internal static class TypeExtensions
    {
        public static bool Implements<TBaseType>(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(TBaseType));
        }

        public static bool IsFunc(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Func<>);
        }

        public static bool IsLazy(this Type type)
        {
            var typeInfo = type.GetTypeInfo();
            return typeInfo.IsGenericType && typeInfo.GetGenericTypeDefinition() == typeof(Lazy<>);
        }
    }

    internal static class ServiceMapExtensions
    {
        public static bool ContainsAmbiguousRegistrationFor(this ServiceMap serviceMap, Type serviceType)
        {
            if (!serviceMap.TryGetValue(serviceType, out var registrations))
            {
                return false;
            }
            return registrations.Count > 1;
        }
    }
}
