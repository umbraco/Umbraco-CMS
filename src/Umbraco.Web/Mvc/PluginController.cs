using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// A base class for all plugin controllers to inherit from
    /// </summary>
    public abstract class PluginController : Controller
    {
        /// <summary>
        /// stores the metadata about plugin controllers
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PluginControllerMetadata> MetadataStorage = new ConcurrentDictionary<Type, PluginControllerMetadata>();

        private UmbracoHelper _umbracoHelper;        

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="umbracoContext"></param>
        protected PluginController(UmbracoContext umbracoContext)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
        }

        protected PluginController(UmbracoContext umbracoContext, UmbracoHelper umbracoHelper)
        {
            if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
            if (umbracoHelper == null) throw new ArgumentNullException("umbracoHelper");
            UmbracoContext = umbracoContext;
            InstanceId = Guid.NewGuid();
            _umbracoHelper = umbracoHelper;
        }

        /// <summary>
        /// Useful for debugging
        /// </summary>
        internal Guid InstanceId { get; private set; }

        /// <summary>
        /// Returns the MemberHelper instance
        /// </summary>
        public MembershipHelper Members
        {
            get { return Umbraco.MembershipHelper; }
        }

        /// <summary>
        /// Returns an UmbracoHelper object
        /// </summary>
        public virtual UmbracoHelper Umbraco
        {
            get { return _umbracoHelper ?? (_umbracoHelper = new UmbracoHelper(UmbracoContext)); }
        }

        /// <summary>
        /// Returns an ILogger
        /// </summary>
        public ILogger Logger
        {
            get { return ProfilingLogger.Logger; }
        }

        /// <summary>
        /// Returns a ProfilingLogger
        /// </summary>
        public virtual ProfilingLogger ProfilingLogger
        {
            get { return UmbracoContext.Application.ProfilingLogger; }
        }

        /// <summary>
        /// Returns the current UmbracoContext
        /// </summary>
        public virtual UmbracoContext UmbracoContext { get; private set; }

        /// <summary>
        /// Returns the current ApplicationContext
        /// </summary>
        public virtual ApplicationContext ApplicationContext
        {
            get { return UmbracoContext.Application; }
        }

        /// <summary>
        /// Returns a ServiceContext
        /// </summary>
        public ServiceContext Services
        {
            get { return ApplicationContext.Services; }
        }

        /// <summary>
        /// Returns a DatabaseContext
        /// </summary>
        public DatabaseContext DatabaseContext
        {
            get { return ApplicationContext.DatabaseContext; }
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
                    var pluginAttribute = type.GetCustomAttribute<PluginControllerAttribute>(false);
                    //check if any inherited class of this type contains the IsBackOffice attribute
                    var backOfficeAttribute = type.GetCustomAttribute<IsBackOfficeAttribute>(true);

                    var meta = new PluginControllerMetadata()
                    {
                        AreaName = pluginAttribute == null ? null : pluginAttribute.AreaName,
                        ControllerName = ControllerExtensions.GetControllerName(type),
                        ControllerNamespace = type.Namespace,
                        ControllerType = type,
                        IsBackOffice = backOfficeAttribute != null
                    };

                    MetadataStorage.TryAdd(type, meta);

                    return meta;
                });

        }
    }
}