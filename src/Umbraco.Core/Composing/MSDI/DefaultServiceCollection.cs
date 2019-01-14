using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Core.Exceptions;

namespace Umbraco.Core.Composing.MSDI
{
    public class DefaultServiceCollection : List<ServiceDescriptor>, IServiceCollection, IRegister
    {
        public object Concrete => this;

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
        {
            Add(new ServiceDescriptor(typeof(TService), sp => factory((IFactory)sp), lifetimes[lifetime]));
        }

        public void RegisterInstance(Type serviceType, object instance)
        {
            Add(new ServiceDescriptor(serviceType, instance));
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
            return RegisterFactory.CreateFactory(this);
        }
    }
}
