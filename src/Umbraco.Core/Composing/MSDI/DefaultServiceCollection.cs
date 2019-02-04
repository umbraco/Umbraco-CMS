using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Composing.MSDI
{
    public class DefaultServiceCollection : List<ServiceDescriptor>, IServiceCollection, IRegister
    {
        public object Concrete => this;
        private IFactory createdFactory = null;

        readonly Dictionary<Lifetime, ServiceLifetime> lifetimes = new Dictionary<Lifetime, ServiceLifetime>
        {
            { Lifetime.Request, ServiceLifetime.Scoped },
            { Lifetime.Singleton, ServiceLifetime.Singleton },
            { Lifetime.Scope, ServiceLifetime.Scoped },
            { Lifetime.Transient, ServiceLifetime.Transient }
        };

        public void Register(Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            Add(new ServiceDescriptor(serviceType, serviceType, lifetimes[lifetime]));
        }

        public void Register(Type serviceType, Type implementingType, Lifetime lifetime = Lifetime.Transient)
        {
            Add(new ServiceDescriptor(serviceType, implementingType, lifetimes[lifetime]));
        }

        public void Register<TService>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            Add(new ServiceDescriptor(typeof(TService), sp => factory(createdFactory), lifetimes[lifetime]));
        }

        public void Register(Type serviceType, object instance)
        {
            Add(new ServiceDescriptor(serviceType, instance));
        }

        /// <inheritdoc />
        public void RegisterFor<TService, TTarget>(Lifetime lifetime = Lifetime.Transient)
            where TService : class
            => RegisterFor<TService, TTarget>(typeof(TService), lifetime);

        /// <inheritdoc />
        public void RegisterFor<TService, TTarget>(Type implementingType, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("Not sure this is going to work.");

            // note that there can only be one implementation or instance registered "for" a service
            // Register(typeof(TService), implementingType, GetTargetedServiceName(implementingType, typeof(TTarget)), lifetimes[lifetime]);
        }

        /// <inheritdoc />
        public void RegisterFor<TService, TTarget>(Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("Not sure this is going to work.");

            // note that there can only be one implementation or instance registered "for" a service
            // Register(f => factory(f), GetTargetedServiceName<TService, TTarget>(), lifetimes[lifetime]);
        }

        /// <inheritdoc />
        public void RegisterFor<TService, TTarget>(TService instance)
            where TService : class
        {
            throw new NotImplementedException("Not sure this is going to work.");

            // Register(typeof(TService), instance, GetTargetedServiceName(instance.GetType(), typeof(TTarget)));
        }

        public void RegisterAuto(Type serviceBaseType)
        {
            // TODO: Delete, doesn't seem necessary
        }

        public void ConfigureForWeb()
        {
            // TODO: Must definitely be pushed to later in process

            //GlobalConfiguration.Configuration.DependencyResolver = new MsDiWebApiDependencyResolver(this);

            //System.Web.Mvc.DependencyResolver.SetResolver(new MsDiDependencyResolver(this));

            //GlobalConfiguration.Configuration.Services.Replace(
            //    typeof(IHttpControllerActivator),
            //    new MsDiActivator(this));
        }

        public IFactory CreateFactory()
        {
            return createdFactory = RegisterFactory.CreateFactory(this);
        }

        internal static string GetTargetedServiceName<TImplementation, TTarget>()
        {
            var implType = typeof(TImplementation);
            var targetType = typeof(TTarget);
            return GetTargetedServiceName(implType, targetType);
        }

        private static string GetTargetedServiceName(Type implType, Type targetType)
        {
            return implType.FullName + ":" + targetType.FullName;
        }
    }
}
