using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core;

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


		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="umbracoContext"></param>
		protected PluginController(UmbracoContext umbracoContext)
		{
			if (umbracoContext == null) throw new ArgumentNullException("umbracoContext");
			UmbracoContext = umbracoContext;
			InstanceId = Guid.NewGuid();
			Umbraco = new UmbracoHelper(umbracoContext);
		}

		/// <summary>
		/// Useful for debugging
		/// </summary>
		internal Guid InstanceId { get; private set; }

		/// <summary>
		/// Returns an UmbracoHelper object
		/// </summary>
		public UmbracoHelper Umbraco { get; private set; }

		/// <summary>
		/// Returns the current UmbracoContext
		/// </summary>
		public UmbracoContext UmbracoContext { get; private set; }

		/// <summary>
		/// Returns the current ApplicationContext
		/// </summary>
		public ApplicationContext ApplicationContext
		{
			get { return UmbracoContext.Application; }
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
			if (!TypeHelper.IsTypeAssignableFrom<PluginController>(type))
			{
				throw new InvalidOperationException("Cannot get metadata from a type that is not a PluginController");
			}

			PluginControllerMetadata meta;
			if (MetadataStorage.TryGetValue(type, out meta))
			{
				return meta;
			}

			var attribute = type.GetCustomAttribute<PluginControllerAttribute>(false);

			meta = new PluginControllerMetadata()
			{
				AreaName = attribute == null ? null : attribute.AreaName,
				ControllerName = ControllerExtensions.GetControllerName(type),
				ControllerNamespace = type.Namespace,
				ControllerType = type
			};

			MetadataStorage.TryAdd(type, meta);

			return meta;
		}
	}
}