﻿using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Extensions;
using Umbraco.Web.WebApi;
using Current = Umbraco.Web.Composing.Current;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Provides a base class for plugin controllers.
    /// </summary>
    public abstract class PluginController : Controller, IDiscoverable
    {
        private static readonly ConcurrentDictionary<Type, PluginControllerMetadata> MetadataStorage
            = new ConcurrentDictionary<Type, PluginControllerMetadata>();

        // for debugging purposes
        internal Guid InstanceId { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets the Umbraco context.
        /// </summary>
        public virtual IUmbracoContext UmbracoContext => UmbracoContextAccessor.UmbracoContext;

        /// <summary>
        /// Gets the database context accessor.
        /// </summary>
        public virtual IUmbracoContextAccessor UmbracoContextAccessor { get; }

        /// <summary>
        /// Gets the database context.
        /// </summary>
        public IUmbracoDatabaseFactory DatabaseFactory { get; }

        /// <summary>
        /// Gets or sets the services context.
        /// </summary>
        public ServiceContext Services { get; }

        /// <summary>
        /// Gets or sets the application cache.
        /// </summary>
        public AppCaches AppCaches { get;  }

        /// <summary>
        /// Gets or sets the profiling logger.
        /// </summary>
        public IProfilingLogger ProfilingLogger { get; }

        /// <summary>
        /// Gets the Umbraco helper.
        /// </summary>
        public UmbracoHelper Umbraco { get; }

        /// <summary>
        /// Gets metadata for this instance.
        /// </summary>
        internal PluginControllerMetadata Metadata => GetMetadata(GetType());

        protected PluginController()
            : this(
                  Current.Factory.GetRequiredService<IUmbracoContextAccessor>(),
                  Current.Factory.GetRequiredService<IUmbracoDatabaseFactory>(),
                  Current.Factory.GetRequiredService<ServiceContext>(),
                  Current.Factory.GetRequiredService<AppCaches>(),
                  Current.Factory.GetRequiredService<IProfilingLogger>()
            )
        {
        }

        protected PluginController(IUmbracoContextAccessor umbracoContextAccessor, IUmbracoDatabaseFactory databaseFactory, ServiceContext services, AppCaches appCaches, IProfilingLogger profilingLogger)
        {
            UmbracoContextAccessor = umbracoContextAccessor;
            DatabaseFactory = databaseFactory;
            Services = services;
            AppCaches = appCaches;
            ProfilingLogger = profilingLogger;
        }

        /// <summary>
        /// Gets metadata for a controller type.
        /// </summary>
        /// <param name="controllerType">The controller type.</param>
        /// <returns>Metadata for the controller type.</returns>
        internal static PluginControllerMetadata GetMetadata(Type controllerType)
        {
            return MetadataStorage.GetOrAdd(controllerType, type =>
            {
                // plugin controller? back-office controller?
                var pluginAttribute = controllerType.GetCustomAttribute<PluginControllerAttribute>(false);
                var backOfficeAttribute = controllerType.GetCustomAttribute<IsBackOfficeAttribute>(true);

                return new PluginControllerMetadata
                {
                    AreaName = pluginAttribute?.AreaName,
                    ControllerName = ControllerExtensions.GetControllerName(controllerType),
                    ControllerNamespace = controllerType.Namespace,
                    ControllerType = controllerType,
                    IsBackOffice = backOfficeAttribute != null
                };
            });
        }
    }
}
