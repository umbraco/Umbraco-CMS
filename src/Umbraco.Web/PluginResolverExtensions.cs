using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Web.Routing;
using umbraco.BusinessLogic;
using umbraco.BusinessLogic.Utils;

namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginResolver
	/// </summary>
	public static class PluginResolverExtensions
	{

		private static volatile IEnumerable<IRequestDocumentResolver> _lookups;
		private static readonly object Locker = new object();

		/// <summary>
		/// Returns all available ILookup objects
		/// </summary>
		/// <param name="plugins"></param>
		/// <returns></returns>
		internal static IEnumerable<IRequestDocumentResolver> ResolveLookups(this PluginResolver plugins)
		{
			if (_lookups == null)
			{
				lock (Locker)
				{
					if (_lookups == null)
					{
						var typeFinder = new TypeFinder2();
						var lookupTypes = typeFinder.FindClassesOfType<IRequestDocumentResolver>();
						var lookups = new List<IRequestDocumentResolver>();
						foreach (var l in lookupTypes)
						{
							try
							{
								var typeInstance = Activator.CreateInstance(l) as IRequestDocumentResolver;
								lookups.Add(typeInstance);
							}
							catch (Exception ex)
							{
								//TODO: Need to fix logging so this doesn't bork if no SQL connection
								//Log.Add(LogTypes.Error, -1, "Error loading ILookup: " + ex.ToString());
							}
						}
						//set the global
						_lookups = lookups;
					}
				}
			}
			return _lookups;
		}

	}
}