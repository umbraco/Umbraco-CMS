using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Web.Compilation;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Linq.Expressions;

namespace umbraco.MacroEngines
{

	[Obsolete("This class has been superceded by Umbraco.Core.Dynamics.ExtensionMethodFinder")]	
    public static class ExtensionMethodFinder
    {
		public static MethodInfo FindExtensionMethod(Type thisType, object[] args, string name, bool argsContainsThis)
		{
			return Umbraco.Core.Dynamics.ExtensionMethodFinder.FindExtensionMethod(thisType, args, name, argsContainsThis);
		}
    }
}
