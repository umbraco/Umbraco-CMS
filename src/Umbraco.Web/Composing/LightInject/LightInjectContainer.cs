using System;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using LightInject;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.Composing.LightInject;

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
        protected LightInjectContainer(ServiceContainer container)
            : base(container)
        { }

        /// <summary>
        /// Creates a new instance of the <see cref="LightInjectContainer"/> class.
        /// </summary>
        public new static LightInjectContainer Create()
            => new LightInjectContainer(CreateServiceContainer());

        /// <inheritdoc />
        public override void ConfigureForWeb()
        {
            // IoC setup for LightInject for MVC/WebApi
            // see comments on MixedLightInjectScopeManagerProvider for explanations of what we are doing here
            if (!(Container.ScopeManagerProvider is MixedLightInjectScopeManagerProvider smp))
                throw new Exception("Container.ScopeManagerProvider is not MixedLightInjectScopeManagerProvider.");
            Container.EnableMvc(); // does container.EnablePerWebRequestScope()
            Container.ScopeManagerProvider = smp; // reverts - we will do it last (in WebRuntime)
            Container.EnableWebApi(GlobalConfiguration.Configuration);
        }

        protected override ILifetime GetLifetime(Lifetime lifetime, Type type)
        {
            switch (lifetime)
            {
                case Lifetime.Transient:
                    return null;
                case Lifetime.Request:
                    //LightInject behaves slightly differently than all containers and based on feedback from the LightInject authors
                    //it seems best to use PerRequestLifeTime for controllers even though controllers will work
                    //just fine with PerScopeLifetime but this is just being 'safe'.
                    //See: https://github.com/seesharper/LightInject/issues/494#issuecomment-519273614
                    //Normally this will return PerScopeLifetime because in LightInject that means "one per request".
                    //See: base class comments
                    //See: https://github.com/umbraco/Umbraco-CMS/issues/6044#issuecomment-518949758

                    return type.Inherits<IController>() || type.Inherits<IHttpController>()
                        ? (ILifetime)new PerRequestLifeTime()
                        : new PerScopeLifetime();
                case Lifetime.Scope:
                    return new PerScopeLifetime();
                case Lifetime.Singleton:
                    return new PerContainerLifetime();
                default:
                    throw new NotSupportedException($"Lifetime {lifetime} is not supported.");
            }
        }
    }
}
