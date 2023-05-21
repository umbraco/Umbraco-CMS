using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Core.Web.Mvc;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Common.Attributes;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.Common.Controllers;

/// <summary>
///     Provides a base class for plugin controllers.
/// </summary>
public abstract class PluginController : Controller, IDiscoverable
{
    private static readonly ConcurrentDictionary<Type, PluginControllerMetadata> MetadataStorage = new();

    protected PluginController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger)
    {
        UmbracoContextAccessor = umbracoContextAccessor;
        DatabaseFactory = databaseFactory;
        Services = services;
        AppCaches = appCaches;
        ProfilingLogger = profilingLogger;
    }

    /// <summary>
    ///     Gets the Umbraco context.
    /// </summary>
    public virtual IUmbracoContext UmbracoContext
    {
        get
        {
            IUmbracoContext umbracoContext = UmbracoContextAccessor.GetRequiredUmbracoContext();
            return umbracoContext;
        }
    }

    /// <summary>
    ///     Gets the database context accessor.
    /// </summary>
    public virtual IUmbracoContextAccessor UmbracoContextAccessor { get; }

    /// <summary>
    ///     Gets the database context.
    /// </summary>
    public IUmbracoDatabaseFactory DatabaseFactory { get; }

    /// <summary>
    ///     Gets or sets the services context.
    /// </summary>
    public ServiceContext Services { get; }

    /// <summary>
    ///     Gets or sets the application cache.
    /// </summary>
    public AppCaches AppCaches { get; }

    /// <summary>
    ///     Gets or sets the profiling logger.
    /// </summary>
    public IProfilingLogger ProfilingLogger { get; }

    /// <summary>
    ///     Gets metadata for this instance.
    /// </summary>
    internal PluginControllerMetadata Metadata => GetMetadata(GetType());

    // for debugging purposes
    internal Guid InstanceId { get; } = Guid.NewGuid();

    /// <summary>
    ///     Gets metadata for a controller type.
    /// </summary>
    /// <param name="controllerType">The controller type.</param>
    /// <returns>Metadata for the controller type.</returns>
    public static PluginControllerMetadata GetMetadata(Type controllerType) =>
        MetadataStorage.GetOrAdd(controllerType, type =>
        {
            // plugin controller? back-office controller?
            PluginControllerAttribute? pluginAttribute =
                controllerType.GetCustomAttribute<PluginControllerAttribute>(false);
            IsBackOfficeAttribute? backOfficeAttribute = controllerType.GetCustomAttribute<IsBackOfficeAttribute>(true);

            return new PluginControllerMetadata
            {
                AreaName = pluginAttribute?.AreaName,
                ControllerName = ControllerExtensions.GetControllerName(controllerType),
                ControllerNamespace = controllerType.Namespace,
                ControllerType = controllerType,
                IsBackOffice = backOfficeAttribute != null,
            };
        });
}
