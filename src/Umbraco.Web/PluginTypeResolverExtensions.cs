using System;
using System.Collections.Generic;
using System.Threading;
using Umbraco.Core;
using Umbraco.Web.Routing;


namespace Umbraco.Web
{
	/// <summary>
	/// Extension methods for the PluginTypeResolver
	/// </summary>
	public static class PluginTypeResolverExtensions
	{
		private static IEnumerable<IDocumentLookup> _lookups;
		private static readonly ReaderWriterLockSlim LookupsLocker = new ReaderWriterLockSlim();

		/// <summary>
		/// Returns all available ILookup objects
		/// </summary>
		/// <param name="resolver"></param>
		/// <returns></returns>
		internal static IEnumerable<IDocumentLookup> ResolveLookups(this PluginTypeResolver resolver)
		{
			if (_lookups == null)
			{
				using (new WriteLock(LookupsLocker))
				{
					var lookupTypes = resolver.TypeFinder.FindClassesOfType<IDocumentLookup>();
					var lookups = new List<IDocumentLookup>();
					foreach (var l in lookupTypes)
					{
						try
						{
							var typeInstance = Activator.CreateInstance(l) as IDocumentLookup;
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
			return _lookups;
		}

	}
}