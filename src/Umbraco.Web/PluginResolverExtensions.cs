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

        private static volatile IEnumerable<Type> _lookups;
        private static readonly object Locker = new object();

        /// <summary>
        /// Returns all available ILookup objects
        /// </summary>
        /// <param name="plugins"></param>
        /// <returns></returns>
        internal static IEnumerable<Type> ResolveLookups(this PluginResolver plugins)
        {
            if (_lookups == null)
            {
                lock(Locker)
                {
                    if (_lookups == null)
                    {
						_lookups = TypeFinder.FindClassesOfType<ILookup>();   
                    }
                }
            }
            return _lookups;
        }

    }
}