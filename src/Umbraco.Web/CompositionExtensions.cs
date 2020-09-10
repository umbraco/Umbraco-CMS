﻿using System;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.Actions;
using Umbraco.Web.ContentApps;
using Umbraco.Web.Dashboards;
using Umbraco.Web.Editors;
using Umbraco.Web.HealthCheck;
using Umbraco.Web.Media.EmbedProviders;
using Umbraco.Web.Mvc;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Search;
using Umbraco.Web.Sections;
using Umbraco.Web.Tour;
using Umbraco.Web.Trees;
using Current = Umbraco.Web.Composing.Current;

// the namespace here is intentional -  although defined in Umbraco.Web assembly,
// this class should be visible when using Umbraco.Core.Components, alongside
// Umbraco.Core's own CompositionExtensions class

// ReSharper disable once CheckNamespace
namespace Umbraco.Web
{
    /// <summary>
    /// Provides extension methods to the <see cref="Composition"/> class.
    /// </summary>
    public static class WebCompositionExtensions
    {
        public static IFactory CreateFactory(this Composition composition)
        {
            throw new NotImplementedException("MSDI");
        }

        public static void RegisterUnique<TService>(this Composition composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void RegisterUnique<TService>(this Composition composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService>(this Composition composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }


        public static void Register<TService, TInstance>(this Composition composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService, TInstance>(this Composition composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService>(this Composition composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register(this Composition composition, Type type, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException("MSDI");
        }

        public static void RegisterUnique<TService>(this IRegister composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void RegisterUnique<TService>(this IRegister composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService>(this IRegister composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }


        public static void Register<TService, TInstance>(this IRegister composition, Func<IFactory, TService> factory, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService, TInstance>(this IRegister composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register<TService>(this IRegister composition, Lifetime lifetime = Lifetime.Transient)
            where TService : class
        {
            throw new NotImplementedException("MSDI");
        }

        public static void Register(this IRegister composition, Type type, Lifetime lifetime = Lifetime.Transient)
        {
            throw new NotImplementedException("MSDI");
        }


        #region Collection Builders


        /// <summary>
        /// Gets the filtered controller factories collection builder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <returns></returns>
        public static FilteredControllerFactoryCollectionBuilder FilteredControllerFactory(this Composition composition)
            => composition.WithCollectionBuilder<FilteredControllerFactoryCollectionBuilder>();



        #endregion

        #region Uniques

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <typeparam name="T">The type of the content last chance finder.</typeparam>
        /// <param name="composition">The composition.</param>
        public static void SetContentLastChanceFinder<T>(this Composition composition)
            where T : class, IContentLastChanceFinder
        {
            composition.RegisterUnique<IContentLastChanceFinder, T>();
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, Func<IFactory, IContentLastChanceFinder> factory)
        {
            composition.RegisterUnique(factory);
        }

        /// <summary>
        /// Sets the content last chance finder.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="finder">A last chance finder.</param>
        public static void SetContentLastChanceFinder(this Composition composition, IContentLastChanceFinder finder)
        {
            composition.Services.AddUnique(_ => finder);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <typeparam name="T">The type of the site domain helper.</typeparam>
        /// <param name="composition"></param>
        public static void SetSiteDomainHelper<T>(this Composition composition)
            where T : class, ISiteDomainHelper
        {
            composition.Services.AddUnique<ISiteDomainHelper, T>();
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="factory">A function creating a helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, Func<IFactory, ISiteDomainHelper> factory)
        {
            composition.Services.AddUnique(factory);
        }

        /// <summary>
        /// Sets the site domain helper.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="helper">A helper.</param>
        public static void SetSiteDomainHelper(this Composition composition, ISiteDomainHelper helper)
        {
            composition.Services.AddUnique(_ => helper);
        }

        /// <summary>
        /// Sets the default controller for rendering template views.
        /// </summary>
        /// <typeparam name="TController">The type of the controller.</typeparam>
        /// <param name="composition">The composition.</param>
        /// <remarks>The controller type is registered to the container by the composition.</remarks>
        public static void SetDefaultRenderMvcController<TController>(this Composition composition)
            => composition.SetDefaultRenderMvcController(typeof(TController));

        /// <summary>
        /// Sets the default controller for rendering template views.
        /// </summary>
        /// <param name="composition">The composition.</param>
        /// <param name="controllerType">The type of the controller.</param>
        /// <remarks>The controller type is registered to the container by the composition.</remarks>
        public static void SetDefaultRenderMvcController(this Composition composition, Type controllerType)
        {
            //composition.OnCreatingFactory["Umbraco.Core.DefaultRenderMvcController"] = () =>
            //{
            //    // no need to register: all IRenderMvcController are registered
            //    //composition.Register(controllerType, Lifetime.Request);
            //    Current.DefaultRenderMvcControllerType = controllerType;
            //};
        }

        #endregion
    }
}
