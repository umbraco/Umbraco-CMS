using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Compilation;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq.Expressions;
using Umbraco.Core;
using Umbraco.Core.Cache;

namespace umbraco.MacroEngines
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.ExtensionMethodFinder")]	
    public static class ExtensionMethodFinder
    {
		public static MethodInfo FindExtensionMethod(Type thisType, object[] args, string name, bool argsContainsThis)
		{
            var runtimeCache = ApplicationContext.Current != null ? ApplicationContext.Current.ApplicationCache.RuntimeCache : new NullCacheProvider();

            return Umbraco.Core.Dynamics.ExtensionMethodFinder.FindExtensionMethod(runtimeCache, thisType, args, name, argsContainsThis);
		}
    }
}
