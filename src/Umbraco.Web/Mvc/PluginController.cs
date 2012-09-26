using System;
using System.Collections.Concurrent;
using System.Web.Mvc;
using Umbraco.Core;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// A base class for all plugin controllers to inherit from
	/// </summary>
	public abstract class PluginController : Controller, IRequiresUmbracoContext
	{
		/// <summary>
		/// stores the metadata about plugin controllers
		/// </summary>
		private static readonly ConcurrentDictionary<Type, PluginControllerMetadata> Metadata = new ConcurrentDictionary<Type, PluginControllerMetadata>();

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="umbracoContext"></param>
		protected PluginController(UmbracoContext umbracoContext)
		{
			UmbracoContext = umbracoContext;
			InstanceId = Guid.NewGuid();
		}

		/// <summary>
		/// Useful for debugging
		/// </summary>
		internal Guid InstanceId { get; private set; }

		public UmbracoContext UmbracoContext { get; set; }

		/// <summary>
		/// Returns the metadata for this instance
		/// </summary>
		internal PluginControllerMetadata GetMetadata()
		{
			PluginControllerMetadata meta;
			if (Metadata.TryGetValue(this.GetType(), out meta))
			{
				return meta;
			}

			var attribute = this.GetType().GetCustomAttribute<PluginControllerAttribute>(false);

			meta = new PluginControllerMetadata()
			{
				AreaName = attribute == null ? null : attribute.AreaName,
				ControllerName = ControllerExtensions.GetControllerName(this.GetType()),
				ControllerNamespace = this.GetType().Namespace,
				ControllerType = this.GetType()
			};

			Metadata.TryAdd(this.GetType(), meta);

			return meta;
		}
	}
}