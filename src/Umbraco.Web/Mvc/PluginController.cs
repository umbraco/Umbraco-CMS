using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Services;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A base class for all plugin controllers to inherit from
    /// </summary>
    public abstract class PluginController : Controller
    {
        protected PluginController()
        {
            InstanceId = Guid.NewGuid();
        }

        /// <summary>
        /// stores the metadata about plugin controllers
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PluginControllerMetadata> MetadataStorage = new ConcurrentDictionary<Type, PluginControllerMetadata>();

        protected override void Execute(System.Web.Routing.RequestContext requestContext)
        {
            if(UmbracoContext == null)
                UmbracoContext = UmbracoContext.Current;

            base.Execute(requestContext);
        }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        private UmbracoHelper _umbracoHelper;
        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public UmbracoHelper Umbraco
        {
            get
            {
                if (_umbracoHelper != null || UmbracoContext != null)
                    return _umbracoHelper ?? (_umbracoHelper = new UmbracoHelper(UmbracoContext));
                return null;
            }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public UmbracoContext UmbracoContext { get; set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public ApplicationContext ApplicationContext
        {
            get
            {
                if (UmbracoContext != null)
                    return UmbracoContext.Application;
                return null;
            }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get
            {
                if (ApplicationContext != null)
                    return ApplicationContext.Services;
                return null;
            }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get
            {
                if (ApplicationContext != null)
                    return ApplicationContext.DatabaseContext;
                return null;
            }
        }

        /// <summary>
        /// Returns the metadata for this instance
        /// </summary>
        internal PluginControllerMetadata Metadata
        {
            get { return GetMetadata(this.GetType()); }
        }

        /// <summary>
        /// Returns the metadata for a PluginController
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static PluginControllerMetadata GetMetadata(Type type)
        {

            return MetadataStorage.GetOrAdd(type, type1 =>
                {
                    var attribute = type.GetCustomAttribute<PluginControllerAttribute>(false);

                    var meta = new PluginControllerMetadata()
                    {
                        AreaName = attribute == null ? null : attribute.AreaName,
                        ControllerName = ControllerExtensions.GetControllerName(type),
                        ControllerNamespace = type.Namespace,
                        ControllerType = type
                    };

                    MetadataStorage.TryAdd(type, meta);

                    return meta;
                });

        }
    }
}